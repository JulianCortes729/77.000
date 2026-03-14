using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[System.Serializable]
/// <summary>
/// Hito narrativo que contiene el texto a mostrar.
/// </summary>
public struct NarrativeMilestone
{
    /// <summary>Texto del hito.</summary>
    [TextArea] public string message;
}

[System.Serializable]
/// <summary>
/// Hito en tiempo real activado al superar un umbral de hectáreas quemadas.
/// </summary>
public struct RealtimeMilestone
{
    /// <summary>Texto del hito en tiempo real.</summary>
    [TextArea] public string message;

    /// <summary>Umbral de hectáreas quemadas para activar el hito.</summary>
    [SerializeField] public int threshold;
}

/// <summary>
/// Gestiona la presentación de mensajes narrativos y los hitos en tiempo real.
/// </summary>
public class NarrativeManager : MonoBehaviour
{
    private Coroutine typingCoroutine;
    /// <summary>Componente TMP para mensajes narrativos.</summary>
    [SerializeField] private TextMeshProUGUI narrativeText;

    /// <summary>Componente TMP para mensajes en tiempo real.</summary>
    [SerializeField] private TextMeshProUGUI realtimeText;

    /// <summary>Hitos narrativos disponibles.</summary>
    [SerializeField] private NarrativeMilestone[] milestones;

    /// <summary>Hitos con umbrales para activación.</summary>
    [SerializeField] private RealtimeMilestone[] realtimeMilestones;

    /// <summary>Índices de hitos ya mostrados en la sesión.</summary>
    private static List<int> shownMilestones = new List<int>();

    /// <summary>Índice del siguiente hito en tiempo real a evaluar.</summary>
    private int nextMilestoneIndex = 0;

    WaitForSecondsRealtime waitForSeconds; 

    public static event Action OnCharTyped;

    // Nuestra fila de espera para los mensajes
    private Queue<string> messageQueue = new Queue<string>();

    // Un candado para saber si la máquina de escribir está ocupada trabajando
    private bool isDisplayingMessage = false;

    /// <summary>
    /// Ordena los hitos en tiempo real por su umbral al inicializar.
    /// </summary>
    private void Awake()
    {
        realtimeMilestones = realtimeMilestones.OrderBy(m => m.threshold).ToArray();
        waitForSeconds = new WaitForSecondsRealtime(0.05f);
    }

    /// <summary>
    /// Suscribe eventos cuando el componente se activa.
    /// </summary>
    private void OnEnable()
    {
        GameManager.OnGameEnded += SendNarrativeMessage;
        FireManager.OnBurnedHectaresCountChanged += CheckRealtimeMilestones;
    }

    /// <summary>
    /// Anula suscripciones cuando el componente se desactiva.
    /// </summary>
    private void OnDisable()
    {
        GameManager.OnGameEnded -= SendNarrativeMessage;
        FireManager.OnBurnedHectaresCountChanged -= CheckRealtimeMilestones;
    }

    /// <summary>
    /// Verifica y muestra el siguiente hito en tiempo real si se alcanza el umbral.
    /// </summary>
    /// <param name="burnedCount">Conteo actual de hectáreas quemadas.</param>
    private void CheckRealtimeMilestones(int burnedCount)
    {
        if (realtimeMilestones == null || realtimeMilestones.Length == 0) return;
        if (realtimeText == null) return;
        if (nextMilestoneIndex == realtimeMilestones.Length) return;

        if (burnedCount >= realtimeMilestones[nextMilestoneIndex].threshold)
        {
           
            messageQueue.Enqueue(realtimeMilestones[nextMilestoneIndex].message);
            nextMilestoneIndex++;
            if(!isDisplayingMessage)
            {
                StartCoroutine(ProcessMessageQueue());
            }
        }
    }

    IEnumerator ProcessMessageQueue()
    {
        isDisplayingMessage = true; // Cierra el candado

        // Mientras haya mensajes en la fila de espera...
        while (messageQueue.Count > 0)
        {
            // Saca el siguiente mensaje de la fila (Dequeue)
            string nextMessage = messageQueue.Dequeue();

            // Unity permite que una corrutina pause y espere a que termine OTRA corrutina.
            // Pásale el texto a tu efecto teletipo y espera a que termine de tipear.
            yield return StartCoroutine(TypewriterEffect(nextMessage));

            // Dale al jugador unos segundos extra para LEER el texto completo antes de mostrar el siguiente.
            yield return new WaitForSecondsRealtime(3f);

            // (Opcional) Borrar el texto de la pantalla aquí si lo deseas
        }

        isDisplayingMessage = false; // Abre el candado cuando no hay más mensajes
    }


    IEnumerator TypewriterEffect(string nextMessage)
    {
        realtimeText.text = nextMessage;

        for (int i = 0; i < nextMessage.Length; i++)
        {
            realtimeText.maxVisibleCharacters = i + 1 ;

            if (realtimeText.text[i] != ' ')
            {
                OnCharTyped?.Invoke();
            }
            
            yield return waitForSeconds;
        }
    }

    /// <summary>
    /// Muestra un mensaje narrativo aleatorio no repetido al terminar el juego.
    /// </summary>
    private void SendNarrativeMessage()
    {
        List<int> availableIndices = new List<int>();

        for (int i = 0; i < milestones.Length; i++)
        {
            if (!shownMilestones.Contains(i))
            {
                availableIndices.Add(i);
            }
        }

        if (availableIndices.Count == 0)
        {
            narrativeText.text = "El viento patagónico repartió las cenizas por mil kilómetros. La justicia no llegó ni a la esquina. Esa es la diferencia entre la naturaleza y el Estado.";
            return;
        }

        int randomPos = UnityEngine.Random.Range(0, availableIndices.Count);
        int realIndice = availableIndices[randomPos];

        narrativeText.text = milestones[realIndice].message;
        shownMilestones.Add(realIndice);

        Debug.Log("Hitos mostrados: " + shownMilestones.Count);
    }
}
