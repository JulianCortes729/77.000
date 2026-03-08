using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;



[System.Serializable]
public struct NarrativeMilestone
{
    [TextArea] public string message;   // El mensaje a mostrar
    
}

public class NarrativeManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI narrativeText; // Referencia al componente de texto para mostrar los mensajes en la UI
    [SerializeField] private NarrativeMilestone[] milestones; // Array de hitos narrativos
    private static List<int> shownMilestones = new List<int>(); // Array para almacenar los índices de los hitos ya mostrados

    

    private void OnDisable()
    {
        GameManager.OnGameEnded -= SendNarrativeMessage; // Desuscribe del evento de fin del juego para evitar llamadas no deseadas al método SendNarrativeMessage
    }

    private void OnEnable()
    {
        GameManager.OnGameEnded += SendNarrativeMessage; // Desuscribe del evento de fin del juego para evitar llamadas no deseadas al método SendNarrativeMessage
    }   

    void SendNarrativeMessage()
    {
        List<int> availableIndices = new List<int>();

        for (int i = 0; i<milestones.Length; i++)
        {
            if (!shownMilestones.Contains(i))
            {
                availableIndices.Add(i); // Agrega el índice del hito a la lista de índices disponibles si no ha sido mostrado previamente
            }
        }

        if (availableIndices.Count == 0)
        {
            narrativeText.text = ""; // Si no hay hitos disponibles, muestra un mensaje indicando que se han alcanzado todos los hitos
            return;
        }

        int randomPos = Random.Range(0, availableIndices.Count); // Selecciona un índice aleatorio dentro del rango de hitos disponibles

        int realIndice = availableIndices[randomPos]; // Obtiene el índice del hito a mostrar utilizando el índice aleatorio seleccionado

        narrativeText.text = milestones[realIndice].message; // Actualiza el texto de la UI con el mensaje del hito alcanzado
        shownMilestones.Add(realIndice);

        Debug.Log("Hitos mostrados: " + shownMilestones.Count); // Imprime en la consola el índice del hito que ha sido mostrado
        
    }
}
