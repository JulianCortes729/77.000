using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Direction
{
    Left,
    Right,
    Down,
    Up,
    UpRight,
    UpLeft,
    DownRight,
    DownLeft
}

public class FireManager : MonoBehaviour
{

    [SerializeField] private float tickRate = 0.1f; // Frecuencia de actualización del fuego en segundos
    [SerializeField] private GridSystem gridSystem; // Referencia al sistema de datos de la cuadrícula
    private HashSet<int> burningHectares; // Conjunto de índices de hectáreas actualmente en llamas
    private HashSet<int> proximasHectareas; // Lista temporal para almacenar las hectáreas que se encenderán en el próximo tick
    WaitForSeconds waitForSeconds; // Variable para almacenar la espera entre ticks de fuego

    private int[] tiempoEnLlamasPorHectarea; // Array para llevar el tiempo en llamas de cada hectárea

    public event Action OnFireExtinguished; // Evento que se dispara cuando el fuego se extingue completamente
    public event Action<int> OnFireActiveCountChanged; // Evento que se dispara cuando cambia el número de hectáreas en llamas, pasando el nuevo conteo como parámetro

    public int countBurnedHectares; // Contador de hectáreas quemadas, se puede usar para estadísticas o condiciones de victoria/derrota
    public int totalHectareas => gridSystem.Width * gridSystem.Height; // Propiedad para obtener el total de hectáreas en la cuadrícula

    [SerializeField] private float fireAggressive = 0.6f;
    [SerializeField] private int cantTicksToBurn = 1;
    private Vector2 windDirection;
    [SerializeField] private float windForce = 0.2f;
    private float[] windMultiplier = new float[8];
        


    private void Awake()
    {
        if (gridSystem == null)
        {
            gridSystem = GetComponent<GridSystem>();
        }

        burningHectares = new HashSet<int>(); // Inicializa el conjunto de hectáreas en llamas
        proximasHectareas = new HashSet<int>(); // Inicializa la lista de próximas hectáreas a encender

    }

    void Start()
    {
        tiempoEnLlamasPorHectarea = new int[gridSystem.Width * gridSystem.Height]; // Inicializa el array para el tiempo en llamas de cada hectárea
        waitForSeconds = new WaitForSeconds(tickRate); // Crea el objeto de espera con la frecuencia definida

        StartCoroutine(FireTickRoutine()); // Inicia la rutina de ticks de fuego
    }

    private void OnDisable()
    {
        GameManager.OnGameStarted -= IgniteRandomSpot; // Desuscribe del evento de inicio del juego para evitar llamadas no deseadas al método IgniteRandomSpot
        WindSystem.OnWindChanged -= UpdateWind; // Desuscribe del evento de cambio de viento para evitar llamadas no deseadas al método UpdateWind
    }

    private void OnEnable()
    {
        GameManager.OnGameStarted += IgniteRandomSpot; // Suscribe al evento de inicio del juego para encender una hectárea aleatoria al comenzar
        WindSystem.OnWindChanged += UpdateWind; // Suscribe al evento de cambio de viento para actualizar la dirección del viento cuando cambie
    }

    

    IEnumerator FireTickRoutine()
    {
        while (true)
        {
            // Aquí se implementaría la lógica de propagación del fuego
            // Por ejemplo, iterar sobre las hectáreas en llamas y propagar a las adyacentes
            yield return waitForSeconds; // Espera 1 segundo entre cada tick de fuego

            for(int i = 0; i<cantTicksToBurn; i++)
            {
                ProcessFireSpread();
                if (burningHectares.Count == 0)
                {
                    OnFireExtinguished?.Invoke(); // Dispara el evento de extinción del fuego si no quedan hectáreas en llamas
                    break;
                }
            }
            OnFireActiveCountChanged?.Invoke(burningHectares.Count); // Dispara el evento de cambio en el conteo de hectáreas en llamas, pasando el nuevo conteo como parámetro


        }
    }


    private void ProcessFireSpread()
    {


        proximasHectareas.Clear(); // Limpia la lista de próximas hectáreas a encender

        foreach (int currentCell in burningHectares)
        {
            // Aquí se implementaría la lógica para determinar las hectáreas adyacentes y si se encienden
            // Por ejemplo, verificar los estados de las hectáreas adyacentes y agregar a proximasHectareas si se encienden
            EvaluarVecinos(currentCell);

        }

        burningHectares.RemoveWhere(currentCell =>
        {
            // Aquí se implementaría la lógica para determinar si la hectárea se quema completamente
            // Por ejemplo, verificar el tiempo que ha estado en llamas y agregar a burnedHectareas si se quema
            bool shouldBurn = EvaluarSiSeQuema(currentCell);

            return shouldBurn; // Devuelve true para eliminar la hectárea de burningHectares si se quema
        });

        foreach (int i in proximasHectareas)
        {
            gridSystem.ChangeHectareState(i, StateHectare.On_Fire); // Cambia el estado de la hectárea a "En llamas"
            burningHectares.Add(i); // Agrega la hectárea a la lista de hectáreas en llamas

        }

        

        
    }

    private bool EvaluarSiSeQuema(int currentCell)
    {

        tiempoEnLlamasPorHectarea[currentCell]++; // Incrementa el tiempo en llamas para la hectárea actual
        if (tiempoEnLlamasPorHectarea[currentCell] >= 2) // Si ha estado en llamas por 5 ticks, se quema completamente
        {
            gridSystem.ChangeHectareState(currentCell, StateHectare.Burned); // Cambia el estado a "Quemada"
            countBurnedHectares++; // Incrementa el contador de hectáreas quemadas
            return true; // Indica que la hectárea se ha quemado completamente
        }
        else
        {
            return false; // Indica que la hectárea sigue en llamas
        }
    }

    private void EvaluarVecinos(int currentCell)
    {
        // Usaremos solo matemática 1D para comprobar existencia de vecinos
        int width = gridSystem.Width;
        int height = gridSystem.Height;
        int total = width * height;


        bool canGoLeft = currentCell % width != 0; // Verifica si no está en el borde izquierdo
        bool canGoRight = (currentCell + 1) % width != 0; // Verifica si no está en el borde derecho
        bool canGoUp = currentCell + width < total; // Verifica si no está en el borde superior
        bool canGoDown = currentCell - width >= 0; // Verifica si no está en el borde inferior



        if (canGoLeft)// Hectárea a la izquierda
        {
            int leftIndex = currentCell - 1;

            if (gridSystem.GetHectareState(leftIndex) == StateHectare.Intact && UnityEngine.Random.value <= fireAggressive + (windForce * windMultiplier[(int)Direction.Left]))
            {
                proximasHectareas.Add(leftIndex); // Agrega la hectárea a la lista de próximas a encender
            }
        }

        if (canGoRight) // Hectárea a la derecha
        {
            int rightIndex = currentCell + 1;

            if (gridSystem.GetHectareState(rightIndex) == StateHectare.Intact && UnityEngine.Random.value <= fireAggressive + (windForce * windMultiplier[(int)Direction.Right]))
            {
                proximasHectareas.Add(rightIndex); // Agrega la hectárea a la lista de próximas a encender
            }
        }

        if (canGoDown) // Hectárea abajo
        {
            int downIndex = currentCell - width;

            if (gridSystem.GetHectareState(downIndex) == StateHectare.Intact && UnityEngine.Random.value <= fireAggressive + (windForce * windMultiplier[(int)Direction.Down]))
            {
                proximasHectareas.Add(downIndex); // Agrega la hectárea a la lista de próximas a encender
            }
        }

        if (canGoUp) // Hectárea arriba
        {
            int upIndex = currentCell + width;

            if (gridSystem.GetHectareState(upIndex) == StateHectare.Intact && UnityEngine.Random.value <= fireAggressive + (windForce * windMultiplier[(int)Direction.Up]))
            {
                proximasHectareas.Add(upIndex); // Agrega la hectárea a la lista de próximas a encender
            }
        }

        if (canGoRight && canGoUp) // Hectárea arriba a la derecha
        {
            int upRightIndex = currentCell + width + 1;

            if (gridSystem.GetHectareState(upRightIndex) == StateHectare.Intact && UnityEngine.Random.value <= fireAggressive + (windForce * windMultiplier[(int)Direction.UpRight]))
            {
                proximasHectareas.Add(upRightIndex); // Agrega la hectárea a la lista de próximas a encender
            }
        }

        if (canGoLeft && canGoUp) // Hectárea arriba a la izquierda
        {
            int upLeftIndex = currentCell + width - 1;
            if (gridSystem.GetHectareState(upLeftIndex) == StateHectare.Intact && UnityEngine.Random.value <= fireAggressive + (windForce * windMultiplier[(int)Direction.UpLeft]))
            {
                proximasHectareas.Add(upLeftIndex); // Agrega la hectárea a la lista de próximas a encender
            }
        }

        if (canGoRight && canGoDown) // Hectárea abajo a la derecha
        {
            int downRightIndex = currentCell - width + 1;
            if (gridSystem.GetHectareState(downRightIndex) == StateHectare.Intact && UnityEngine.Random.value <= fireAggressive + (windForce * windMultiplier[(int)Direction.DownRight]))
            {
                proximasHectareas.Add(downRightIndex); // Agrega la hectárea a la lista de próximas a encender
            }
        }

        if (canGoLeft && canGoDown) // Hectárea abajo a la izquierda
        {
            int downLeftIndex = currentCell - width - 1;
            if (gridSystem.GetHectareState(downLeftIndex) == StateHectare.Intact && UnityEngine.Random.value <= fireAggressive + (windForce * windMultiplier[(int)Direction.DownLeft]))
            {
                proximasHectareas.Add(downLeftIndex); // Agrega la hectárea a la lista de próximas a encender
            }
        }

    }

    public void IgniteRandomSpot()
    {
        int randomIndex = UnityEngine.Random.Range(0, gridSystem.Width * gridSystem.Height); // Genera un índice aleatorio dentro de los límites de la cuadrícula
        gridSystem.ChangeHectareState(randomIndex, StateHectare.On_Fire); // Cambia el estado de la hectárea a "En llamas"
        burningHectares.Add(randomIndex); // Agrega la hectárea a la lista de hectáreas en llamas
    }



    private void UpdateWind(Vector2 vector)
    {
        windDirection = vector; // Actualiza la dirección del viento con el nuevo valor recibido del evento de cambio de viento

        windMultiplier[0] = Vector2.Dot(windDirection.normalized, Vector2.left);
        windMultiplier[1] = Vector2.Dot(windDirection.normalized, Vector2.right);
        windMultiplier[2] = Vector2.Dot(windDirection.normalized, Vector2.down);
        windMultiplier[3] = Vector2.Dot(windDirection.normalized, Vector2.up);
        windMultiplier[4] = Vector2.Dot(windDirection.normalized, new Vector2(1, 1).normalized);
        windMultiplier[5] = Vector2.Dot(windDirection.normalized, new Vector2(-1, 1).normalized);
        windMultiplier[6] = Vector2.Dot(windDirection.normalized, new Vector2(1, -1).normalized);
        windMultiplier[7] = Vector2.Dot(windDirection.normalized, new Vector2(-1, -1).normalized);

    }

}
