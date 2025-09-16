using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class ButtonAudioPlayer : MonoBehaviour
{
    public AudioClip ButtonClip;

    private AudioSource _audioSource;
    void Awake()
    {
        _audioSource = gameObject.GetComponent<AudioSource>();

    }
    public void PlayAudio()
    {
        _audioSource.clip = ButtonClip;
        _audioSource.loop = false;
        _audioSource.Play();

    }
}
