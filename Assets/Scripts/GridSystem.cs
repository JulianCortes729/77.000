using System;
using UnityEngine;

// Estados posibles de una hectáreapublic enum StateHectare
public enum StateHectare
{
    Intact,   // Sin daños
    On_Fire,  // En llamas
    Burned,   // Quemada
    Firewall  // Cortafuegos / barrera de fuego
}

// Gestiona la cuadrícula y los estados de cada hectárea
public class GridSystem : MonoBehaviour
{
    // Almacén lineal de estados (width * height)
    private StateHectare[] hectares;

    // Evento que notifica cambios: (indice, nuevoEstado)
    public event Action<int, StateHectare> OnHectareChanged;

    // Dimensiones de la rejilla (ajustar según mapa)
    private int width = 275;
    private int height = 200;

    void Awake()
    {
        // Inicializa el array de estados
        hectares = new StateHectare[height * width];
    }

    // Convierte índice lineal a coordenadas (x, y)
    public Vector2Int ExtractionCoordinates(int indice)
    {
        Vector2Int coordinates = new Vector2Int(indice % width, indice / width);
        return coordinates;
    }

    // Convierte coordenadas (x, y) a índice lineal
    public int ExtractionIndice(int x, int y)
    {
        int indice = x + y * width;
        return indice;
    }

    // Cambia el estado usando coordenadas (x, y)
    public void ChangeHectareState(StateHectare newState, int x, int y)
    {
        int indice = ExtractionIndice(x, y);
        ChangeHectareState(indice, newState);
    }

    // Cambia el estado por índice y dispara el evento de notificación
    public void ChangeHectareState(int indice, StateHectare newState)
    {
        hectares[indice] = newState;
        OnHectareChanged?.Invoke(indice, newState);
    }

    public StateHectare GetHectareState(int indice)
    {
        return hectares[indice];
    }

    public StateHectare GetHectareState(int x, int y)
    {
        int indice = ExtractionIndice(x, y);
        return GetHectareState(indice);
    }

    public int Width => width;
    public int Height => height;

    public StateHectare[] Hectares => hectares; // Exponer el array completo si es necesario
}
