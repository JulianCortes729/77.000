using System;
using System.ComponentModel;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private FireManager fireManager;
    [SerializeField] private AudioSource fireMassiveAudioSource;
    [SerializeField] private AudioSource fireAudioSource;
    [SerializeField] private int maxFiresForPeakVolume;
    private float valorAudio;

    private void OnEnable()
    {
        fireManager.OnFireActiveCountChanged += HandleFireActiveCountChanged;
    }

    private void OnDisable()
    {
        fireManager.OnFireActiveCountChanged -= HandleFireActiveCountChanged;

    }
   
    private void HandleFireActiveCountChanged(int cantHectares)
    {
        valorAudio = Mathf.InverseLerp(0, maxFiresForPeakVolume, cantHectares);

        if (cantHectares==0)
        {
            fireAudioSource.volume = 0;
            fireMassiveAudioSource.volume = 0;
        }
        else
        {
            fireMassiveAudioSource.volume = valorAudio;
            fireAudioSource.volume = 1f - valorAudio;
        }

    }

}
