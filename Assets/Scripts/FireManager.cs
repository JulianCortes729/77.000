using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireManager : MonoBehaviour
{

    [SerializeField] private float tickRate = 1f; // Frecuencia de actualización del fuego en segundos
    [SerializeField] private GridSystem gridSystem; // Referencia al sistema de datos de la cuadrícula
    private HashSet<int> burningHectares; // Conjunto de índices de hectáreas actualmente en llamas
    private HashSet<int> proximasHectareas; // Lista temporal para almacenar las hectáreas que se encenderán en el próximo tick
    WaitForSeconds waitForSeconds; // Variable para almacenar la espera entre ticks de fuego

    private int[] tiempoEnLlamasPorHectarea; // Array para llevar el tiempo en llamas de cada hectárea

    public event Action OnFireExtinguished; // Evento que se dispara cuando el fuego se extingue completamente

    public int countBurnedHectares; // Contador de hectáreas quemadas, se puede usar para estadísticas o condiciones de victoria/derrota

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

        IgniteRandomSpot(); // Enciende una hectárea aleatoria al inicio para iniciar la propagación del fuego

        StartCoroutine(FireTickRoutine()); // Inicia la rutina de ticks de fuego
    }
    IEnumerator FireTickRoutine()
    {
        while (true)
        {
            // Aquí se implementaría la lógica de propagación del fuego
            // Por ejemplo, iterar sobre las hectáreas en llamas y propagar a las adyacentes
            yield return waitForSeconds; // Espera 1 segundo entre cada tick de fuego

            ProcessFireSpread();
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

        if (burningHectares.Count == 0)
        {
            OnFireExtinguished?.Invoke(); // Dispara el evento de extinción del fuego si no quedan hectáreas en llamas
        }
    }

    private bool EvaluarSiSeQuema(int currentCell)
    {

        tiempoEnLlamasPorHectarea[currentCell]++; // Incrementa el tiempo en llamas para la hectárea actual
        if (tiempoEnLlamasPorHectarea[currentCell] >= 5) // Si ha estado en llamas por 5 ticks, se quema completamente
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




        if (currentCell % width != 0)
        {// Hectárea a la izquierda
            int leftIndex = currentCell - 1;
            if (gridSystem.GetHectareState(leftIndex) == StateHectare.Intact)
            {
                proximasHectareas.Add(leftIndex); // Agrega la hectárea a la lista de próximas a encender
            }
        }

        if ((currentCell + 1) % width != 0) // Hectárea a la derecha
        {
            int rightIndex = currentCell + 1;
            if (gridSystem.GetHectareState(rightIndex) == StateHectare.Intact)
            {
                proximasHectareas.Add(rightIndex); // Agrega la hectárea a la lista de próximas a encender
            }
        }

        if (currentCell - width >= 0) // Hectárea abajo
        {
            int downIndex = currentCell - width;
            if (gridSystem.GetHectareState(downIndex) == StateHectare.Intact)
            {
                proximasHectareas.Add(downIndex); // Agrega la hectárea a la lista de próximas a encender
            }
        }

        if (currentCell + width < total) // Hectárea arriba
        {
            int upIndex = currentCell + width;
            if (gridSystem.GetHectareState(upIndex) == StateHectare.Intact)
            {
                proximasHectareas.Add(upIndex); // Agrega la hectárea a la lista de próximas a encender
            }
        }



    }

    public void IgniteRandomSpot()
    {
        int randomIndex = UnityEngine.Random.Range(0, gridSystem.Width * gridSystem.Height); // Genera un índice aleatorio dentro de los límites de la cuadrícula
        gridSystem.ChangeHectareState(randomIndex, StateHectare.On_Fire); // Cambia el estado de la hectárea a "En llamas"
        burningHectares.Add(randomIndex); // Agrega la hectárea a la lista de hectáreas en llamas
    }
}
