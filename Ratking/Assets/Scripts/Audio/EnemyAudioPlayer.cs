using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyAudioPlayer : MonoBehaviour
{
    public AudioClip WalkClip;
    public AudioClip ShootClip;
    public AudioClip AlertAudioClip;
    public AudioClip SuspiciousAudioClip;

    private AudioSource _audioSource;
    void Awake()
    {
        _audioSource = gameObject.GetComponent<AudioSource>();

    }
    public void PlayAudio(AudioClip clip, bool loop)
    {
        _audioSource.clip = clip;
        _audioSource.loop = loop;
        _audioSource.Play();
        
    }

}

