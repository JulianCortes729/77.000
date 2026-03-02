using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;



[System.Serializable]
public struct NarrativeMilestone
{
    public int burnedHectaresThreshold; // El número clave (ej. 20000)
    [TextArea] public string message;   // El mensaje a mostrar
}

public class NarrativeManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI narrativeText; // Referencia al componente de texto para mostrar los mensajes en la UI
    [SerializeField] private NarrativeMilestone[] milestones; // Array de hitos narrativos
    

    
    private void Start()
    {
        milestones = milestones.OrderBy(m => m.burnedHectaresThreshold).ToArray(); // Ordena los hitos por su umbral de hectáreas quemadas para facilitar la comparación
    }

    private void OnDisable()
    {
        GameManager.OnGameEnded -= SendNarrativeMessage; // Desuscribe del evento de fin del juego para evitar llamadas no deseadas al método SendNarrativeMessage
    }

    private void OnEnable()
    {
        GameManager.OnGameEnded += SendNarrativeMessage; // Desuscribe del evento de fin del juego para evitar llamadas no deseadas al método SendNarrativeMessage
    }   

    void SendNarrativeMessage(int cantHectares)
    {
         for (int i = milestones.Length-1; i >= 0; i--)
         {
            if (cantHectares >= milestones[i].burnedHectaresThreshold)
            {
                narrativeText.text = milestones[i].message; // Actualiza el texto de la UI con el mensaje del hito alcanzado
                break; // Salir del bucle después de encontrar el primer hito alcanzado (el más alto)
            }
         }
    }
}
