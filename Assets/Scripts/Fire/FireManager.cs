using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Gestiona la propagación del fuego en el grid utilizando simulación por ticks.
/// Calcula dinámicamente la expansión del fuego según agresividad, viento y probabilidades.
public class FireManager : MonoBehaviour
{
    [Header("Dependencies")]
    /// Sistema de grid que gestiona los estados de las hectáreas.
    [SerializeField] private GridSystem gridSystem;

    [Header("Settings")]
    /// Intervalo en segundos entre cada simulación de propagación del fuego.
    [SerializeField] private float tickRate = 0.1f;
    /// Probabilidad base de que el fuego se propague a una celda vecina intacta.
    [SerializeField] private float fireAggressive = 0.6f;
    /// Multiplicador de influencia del viento en la propagación del fuego.
    [SerializeField] private float windForce = 0.2f;
    /// Cantidad de ticks que tarda una hectárea en pasar de "En Fuego" a "Quemada".
    [SerializeField] private int cantTicksToBurn = 2;

    /// Se dispara cuando no hay más fuegos activos en el grid.
    public event Action OnFireExtinguished;
    /// Se dispara cuando cambia la cantidad de celdas activas en fuego. Parámetro: cantidad actual.
    public event Action<int> OnFireActiveCountChanged;
    /// Se dispara cuando cambia la cantidad total de hectáreas quemadas. Parámetro: cantidad actual.
    public static event Action<int> OnBurnedHectaresCountChanged;

    /// Cantidad total de hectáreas quemadas hasta el momento.
    public int CountBurnedHectares { get; private set; }
    /// Total de celdas en el grid (Width × Height).
    public int TotalHectareas => gridSystem.TotalCells;

    /// Celdas actualmente en fuego. Se reutiliza cada tick para evitar asignaciones.
    private List<int> _activeFires;
    /// Celdas candidatas a encenderse en el próximo tick.
    private List<int> _nextFires;
    /// Contador de ticks que cada celda ha estado en fuego (byte: 0-255 es suficiente).
    private byte[] _timeOnFire;
    /// Multiplicadores pre-calculados del viento para cada una de las 8 direcciones.
    private float[] _windMultipliers = new float[8];

    /// Dirección normalizada del viento actual.
    private Vector2 _windDirection;
    /// Yield reutilizable para el loop principal de fuego.
    private WaitForSeconds _tickYield;

    /// Offsets pre-calculados hacia los 8 vecinos (izq, der, abajo, arriba, esquinas).
    private int[] _neighborOffsets;

    /// Inicializa colecciones, cálculo de yield y offsets de vecinos.
    private void Awake()
    {
        gridSystem ??= GetComponent<GridSystem>();

        int total = gridSystem.TotalCells;
        _activeFires = new List<int>(total / 10); // Asignación inicial prudente
        _nextFires = new List<int>(total / 10);
        _timeOnFire = new byte[total];

        _tickYield = new WaitForSeconds(tickRate);

        // Direcciones: Izq, Der, Abajo, Arriba, Arr-Der, Arr-Izq, Ab-Der, Ab-Izq
        int w = gridSystem.Width;
        _neighborOffsets = new int[] { -1, 1, -w, w, w + 1, w - 1, -w + 1, -w - 1 };
    }

    private void OnEnable()
    {
        GameManager.OnGameStarted += IgniteRandomSpot;
        WindSystem.OnWindChanged += UpdateWind;
    }

    private void OnDisable()
    {
        GameManager.OnGameStarted -= IgniteRandomSpot;
        WindSystem.OnWindChanged -= UpdateWind;
    }

    /// Inicia la corrutina principal de simulación del fuego.
    private void Start() => StartCoroutine(FireTickRoutine());

    /// Loop principal que ejecuta la propagación del fuego en intervalos regulares.
    private IEnumerator FireTickRoutine()
    {
        while (true)
        {
            yield return _tickYield;

            if (_activeFires.Count == 0) continue; // Sleep idle

            ProcessFireSpread();
        }
    }

    /// Procesa la propagación del fuego: envejece celdas activas, quema las que se vencen, expande hacia vecinos intactos.
    private void ProcessFireSpread()
    {
        int prevBurnedCount = CountBurnedHectares;
        _nextFires.Clear();

        // Iteración sobre lista pre-alocada (Cache Friendly)
        for (int i = _activeFires.Count - 1; i >= 0; i--)
        {
            int cell = _activeFires[i];
            EvaluarVecinos(cell);

            _timeOnFire[cell]++;
            if (_timeOnFire[cell] >= cantTicksToBurn)
            {
                gridSystem.ChangeHectareState(cell, StateHectare.Burned);
                CountBurnedHectares++;

                // Fast removal O(1) sin garbage collection (swap con el último elemento)
                _activeFires[i] = _activeFires[^1];
                _activeFires.RemoveAt(_activeFires.Count - 1);
            }
        }

        foreach (int nextCell in _nextFires)
        {
            // Verificación doble por si varios vecinos encendieron la misma celda en este tick
            if (gridSystem.GetHectareState(nextCell) == StateHectare.Intact)
            {
                gridSystem.ChangeHectareState(nextCell, StateHectare.On_Fire);
                _activeFires.Add(nextCell);
            }
        }

        if (prevBurnedCount != CountBurnedHectares)
            OnBurnedHectaresCountChanged?.Invoke(CountBurnedHectares);

        OnFireActiveCountChanged?.Invoke(_activeFires.Count);

        if (_activeFires.Count == 0 && CountBurnedHectares > 0)
            OnFireExtinguished?.Invoke();
    }

    /// Evalúa los 8 vecinos de una celda en fuego para determinar cuáles se incendian según probabilidad y viento.
    private void EvaluarVecinos(int cell)
    {
        int w = gridSystem.Width;
        int x = cell % w;

        // Banderas de bordes (Fast bitwise)
        bool noLeft = x == 0;
        bool noRight = x == w - 1;

        // Iteramos sobre las 8 direcciones mapeadas
        for (int i = 0; i < 8; i++)
        {
            // Ignorar direcciones que cruzan los bordes horizontales
            if (noLeft && (i == 0 || i == 5 || i == 7)) continue;
            if (noRight && (i == 1 || i == 4 || i == 6)) continue;

            int neighborIndex = cell + _neighborOffsets[i];

            if (neighborIndex >= 0 && neighborIndex < TotalHectareas)
            {
                if (gridSystem.GetHectareState(neighborIndex) == StateHectare.Intact)
                {
                    float threshold = fireAggressive + (windForce * _windMultipliers[i]);
                    if (UnityEngine.Random.value <= threshold)
                    {
                        _nextFires.Add(neighborIndex);
                    }
                }
            }
        }
    }

    /// Enciende una hectárea aleatoria del grid. Se invoca al iniciarse el juego.
    private void IgniteRandomSpot()
    {
        int randomIndex = UnityEngine.Random.Range(0, TotalHectareas);
        gridSystem.ChangeHectareState(randomIndex, StateHectare.On_Fire);
        _activeFires.Add(randomIndex);
    }

    /// Actualiza la dirección del viento y recalcula multiplicadores para todas las 8 direcciones.
    private void UpdateWind(Vector2 direction)
    {
        _windDirection = direction.normalized;
        // Pre-calcular multiplicadores de viento para el shader/matemática
        Vector2[] dirs = new Vector2[] { Vector2.left, Vector2.right, Vector2.down, Vector2.up,
                                         new Vector2(1,1), new Vector2(-1,1), new Vector2(1,-1), new Vector2(-1,-1) };

        for (int i = 0; i < 8; i++)
            _windMultipliers[i] = Vector2.Dot(_windDirection, dirs[i].normalized);
    }
}