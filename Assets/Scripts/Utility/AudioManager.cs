using System;
using DG.Tweening;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource _audioSource;

    private void OnTriggerEnter(Collider other)
    {
        _audioSource.DOPitch(0.6f, 0.5f);
        
    }
    
    private void OnTriggerExit(Collider other)
    {
        _audioSource.DOPitch(1f, 0.5f);
    }
}
