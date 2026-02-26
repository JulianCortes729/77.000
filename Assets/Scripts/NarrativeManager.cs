using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;



[System.Serializable]
public struct NarrativeMilestone
{
    public int burnedHectaresThreshold; // El número clave (ej. 20000)
    [TextArea] public string message;   // El mensaje a mostrar
    public bool isTriggered;            // Para saber si ya lo mostramos y no repetirlo
}

public class NarrativeManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI narrativeText; // Referencia al componente de texto para mostrar los mensajes en la UI
    [SerializeField] private NarrativeMilestone[] milestones; // Array de hitos narrativos
    [SerializeField] private FireManager fireManager; // Referencia al FireManager para obtener el número de hectáreas quemadas
    private Queue<IEnumerator> queue; // Cola para manejar la secuencia de mensajes, asegurando que no se solapen
    private Coroutine currentCoroutine; // Para almacenar la rutina actual y evitar solapamientos


    private WaitForSecondsRealtime waitForSeconds;
    private void Awake()
    {
        waitForSeconds = new WaitForSecondsRealtime(2); // Tiempo de espera entre mensajes
        queue = new Queue<IEnumerator>();
    }

    private void OnDisable()
    {
        fireManager.OnFireManyHectares -= SendNarrativeMessage; // Desuscribirse al evento del FireManager
    }

    private void OnEnable()
    {
        fireManager.OnFireManyHectares += SendNarrativeMessage; // Suscribirse del evento del FireManager
    }   

    void SendNarrativeMessage(int cantHectares)
    {
        for (int i = 0; i < milestones.Length; i++)
        {
            if (!milestones[i].isTriggered && cantHectares >= milestones[i].burnedHectaresThreshold)
            {
                Enqueue(FadeInOut(milestones[i].message)); // Agrega la rutina de mostrar el mensaje a la cola
                milestones[i].isTriggered = true; // Marcar este hito como mostrado para no repetirlo
            }
        }
    }


    IEnumerator FadeInOut(string message)
    {
        narrativeText.text = message; // Establece el mensaje a mostrar
        narrativeText.alpha = 0; // Comienza con el texto invisible
        // Fade in
        while (narrativeText.alpha < 1)
        {
            narrativeText.alpha += Time.unscaledDeltaTime; // Incrementa la opacidad con el tiempo
            yield return null; // Espera al siguiente frame
        }
        yield return waitForSeconds; // Mantiene el mensaje visible por un tiempo
        // Fade out
        while (narrativeText.alpha > 0)
        {
            narrativeText.alpha -= Time.unscaledDeltaTime; // Decrementa la opacidad con el tiempo
            yield return null; // Espera al siguiente frame
        }
    }


    void Enqueue(IEnumerator enumerator)
    {
        queue.Enqueue(enumerator); // Agrega la rutina a la cola
        if (currentCoroutine == null)
        {
            currentCoroutine = StartCoroutine(ProcessQueue()); // Si no hay una rutina en ejecución, inicia el procesamiento de la cola
        }

    }

    public void StopAll()
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        queue.Clear();
        currentCoroutine = null;
    }

    private IEnumerator ProcessQueue()
    {
        while (queue.Count > 0)
        {
            yield return queue.Dequeue(); // Ejecuta la siguiente rutina en la cola
        }
        currentCoroutine = null;
    }
}
