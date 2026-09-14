using System;
using UnityEngine;

/// Estado posible de cada hectárea del grid. Usamos <c>byte</c> para ahorrar memoria
/// cuando se manejan muchos elementos (77,000).
public enum StateHectare : byte
{
    /// Hectárea sin daño.
    Intact = 0,
    /// Hectárea en llamas.
    On_Fire = 1,
    /// Hectárea ya quemada.
    Burned = 2,
    /// Hectárea protegida por cortafuegos.
    Firewall = 3
}

public class GridSystem : MonoBehaviour
{
    // C# 12 Auto-properties
    /// Anchura del grid en celdas.
    public int Width { get; private set; } = 275;

    /// Altura del grid en celdas.
    public int Height { get; private set; } = 280;

    /// Total de celdas (Width * Height).
    public int TotalCells => Width * Height;

    /// Arreglo plano con el estado de cada hectárea.
    private StateHectare[] _hectares;

    /// Evento disparado cuando cambia el estado de una hectárea.
    /// Parámetros: índice plano, nuevo estado.
    public event Action<int, StateHectare> OnHectareChanged;

    private void Awake()
    {
        _hectares = new StateHectare[TotalCells];
    }

    /// Calcula el índice plano a partir de coordenadas (x,y).
    public int GetIndex(int x, int y) => x + y * Width;

    /// Valida si las coordenadas están dentro del grid.
    public bool IsValidCoordinate(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    /// Cambia el estado de la hectárea en (x,y).
    public void ChangeHectareState(int x, int y, StateHectare newState)
    {
        if (!IsValidCoordinate(x, y)) return;
        ChangeHectareState(GetIndex(x, y), newState);
    }

    /// Cambia el estado de la hectárea por índice plano e invoca el evento.
    public void ChangeHectareState(int index, StateHectare newState)
    {
        if (_hectares[index] == newState) return; // Evitar eventos redundantes
        _hectares[index] = newState;
        OnHectareChanged?.Invoke(index, newState);
    }

    /// Obtiene el estado de la hectárea por índice plano.
    public StateHectare GetHectareState(int index) => _hectares[index];

    /// Obtiene el estado de la hectárea en coordenadas (x,y).
    public StateHectare GetHectareState(int x, int y) => _hectares[GetIndex(x, y)];

    /// Provee acceso de solo lectura a la memoria cruda del grid.
    /// Útil para sistemas paralelos o envío a shaders.
    public ReadOnlySpan<StateHectare> GetRawData() => _hectares;
}