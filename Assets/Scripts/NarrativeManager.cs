using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public struct NarrativeMilestone
{
    [TextArea] public string message;
}

[System.Serializable]
public struct RealtimeMilestone
{
    [TextArea] public string message;
    public int threshold;
}

public class NarrativeManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI narrativeText;
    [SerializeField] private TextMeshProUGUI realtimeText;

    [Header("Data")]
    [SerializeField] private NarrativeMilestone[] milestones;
    [SerializeField] private RealtimeMilestone[] realtimeMilestones;

    public static event Action OnCharTyped;

    private static HashSet<int> _shownMilestones = new HashSet<int>(); // HashSet para búsquedas O(1)
    private Queue<string> _messageQueue = new Queue<string>();

    private WaitForSecondsRealtime _typewriterDelay;
    private WaitForSecondsRealtime _messageReadDelay;

    private int _nextMilestoneIndex = 0;
    private bool _isDisplayingMessage = false;

    private void Awake()
    {
        // Cachear Delays evita generar basura (GC Alloc) en cada frame o letra
        _typewriterDelay = new WaitForSecondsRealtime(0.05f);
        _messageReadDelay = new WaitForSecondsRealtime(3f);

        // Sort in-place en Awake está perfecto porque ocurre una sola vez
        Array.Sort(realtimeMilestones, (a, b) => a.threshold.CompareTo(b.threshold));
    }

    private void OnEnable()
    {
        GameManager.OnGameEnded += HandleGameEnded;
        FireManager.OnBurnedHectaresCountChanged += CheckRealtimeMilestones;
    }

    private void OnDisable()
    {
        GameManager.OnGameEnded -= HandleGameEnded;
        FireManager.OnBurnedHectaresCountChanged -= CheckRealtimeMilestones;
    }

    private void CheckRealtimeMilestones(int burnedCount)
    {
        if (realtimeMilestones == null || realtimeText == null || _nextMilestoneIndex >= realtimeMilestones.Length)
            return;

        if (burnedCount >= realtimeMilestones[_nextMilestoneIndex].threshold)
        {
            _messageQueue.Enqueue(realtimeMilestones[_nextMilestoneIndex].message);
            _nextMilestoneIndex++;

            if (!_isDisplayingMessage)
            {
                StartCoroutine(ProcessMessageQueue());
            }
        }
    }

    private IEnumerator ProcessMessageQueue()
    {
        _isDisplayingMessage = true;

        while (_messageQueue.Count > 0)
        {
            string nextMessage = _messageQueue.Dequeue();
            yield return StartCoroutine(TypewriterEffect(nextMessage));
            yield return _messageReadDelay;
            realtimeText.text = ""; // Opcional: borrar tras leer
        }

        _isDisplayingMessage = false;
    }

    private IEnumerator TypewriterEffect(string message)
    {
        realtimeText.text = message;

        for (int i = 0; i < message.Length; i++)
        {
            realtimeText.maxVisibleCharacters = i + 1;

            if (message[i] != ' ')
            {
                OnCharTyped?.Invoke();
            }

            yield return _typewriterDelay;
        }
    }

    // Firma modificada para coincidir con el evento nuevo
    private void HandleGameEnded(int burned, int total)
    {
        List<int> availableIndices = new List<int>(milestones.Length);

        for (int i = 0; i < milestones.Length; i++)
        {
            if (!_shownMilestones.Contains(i))
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
        _shownMilestones.Add(realIndice);
    }
}