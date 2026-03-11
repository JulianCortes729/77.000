using System;
using System.ComponentModel;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private FireManager fireManager;
    [SerializeField] private AudioSource fireAudioSource;
    [SerializeField] private AudioSource fireMassiveAudioSource;
    [SerializeField] private AudioSource charTypedAudioSource;
    [SerializeField] private AudioClip audioCharTyped;
    [SerializeField] private int maxFiresForPeakVolume;
    private float valorAudio;

    private void OnEnable()
    {
        fireManager.OnFireActiveCountChanged += HandleFireActiveCountChanged;
        NarrativeManager.OnCharTyped += HandleCharTyped;
    }

    private void OnDisable()
    {
        fireManager.OnFireActiveCountChanged -= HandleFireActiveCountChanged;
        NarrativeManager.OnCharTyped -= HandleCharTyped;

    }

    private void HandleCharTyped()
    {
       charTypedAudioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
       charTypedAudioSource.PlayOneShot(audioCharTyped);
    }

    private void HandleFireActiveCountChanged(int cantHectares)
    {
        valorAudio = Mathf.InverseLerp(0, maxFiresForPeakVolume, cantHectares);

        if (cantHectares == 0)
        {
            fireAudioSource.volume = 0;
            fireMassiveAudioSource.volume = 0;
        }
        else
        {
            fireAudioSource.volume = 1f - valorAudio;

            fireMassiveAudioSource.volume = valorAudio;
        }

    }

}
