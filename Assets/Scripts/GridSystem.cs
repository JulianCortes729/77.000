using System;
using UnityEngine;

public enum StateHectare
{
     INTACT,
     ON_FIRE,
     BURNED,
     FIREWALL
}

public class GridSystem : MonoBehaviour
{
    private StateHectare[] hectares;

    public event Action<int, StateHectare> OnHectareChanged;

    private int width = 575; //Assuming a width 
    private int height = 400; //Assuming a height

    void Start()
    {
        hectares = new StateHectare[height * width];
    }

    Vector2Int ExtractionCoordinates(int indice)
    {
        //Extract the coordinates of the hectare that was clicked 
        Vector2Int coordinates = new Vector2Int(indice % width, indice / width); //Calculate the x and y coordinates based on the index and the width of the grid

        return coordinates; //vector2 with the coordinates of the hectare that was clicked on
    }
    int ExtractionIndice(int x, int y)
    {
        //Extract the index of the hectare that was clicked

        int indice = x + y * width; //Calculate the index based on the x and y coordinates and the width of the grid

        return indice; //index of the hectare that was clicked on
    }

    void ChangeHectareState(StateHectare newState, int x, int y)
    {
        //Change the state of the hectare at the given coordinates to the new state

        int indice = ExtractionIndice(x, y); //Extract the index of the hectare that was clicked on and change its state to FIREWALL

        ChangeHectareState(indice, newState); //Change the state of the hectare at the given index to the new state

        
    }

    public void ChangeHectareState(int indice, StateHectare newState)
    {
        hectares[indice] = newState;
        OnHectareChanged?.Invoke(indice, newState); //Invoke the event to notify that the hectare has changed
    }


}
