using System.Collections;
using UnityEngine;

[RequireComponent(typeof (AudioSource))]
public class RatAudioPlayer : MonoBehaviour
{
    //public AudioMixerGroup mixerGroup;

    
    [SerializeField] private AudioClip _walkClip;
    [SerializeField] private AudioClip _runClip;
    [SerializeField] private AudioClip _jumpClip;
    [SerializeField] private AudioClip _landClip;
    [SerializeField] public AudioClip throwClip;
    [SerializeField] public AudioClip ventClip;

    private AudioSource _audioSource;

    private PlayerStates.States _playerState;

    private Coroutine PlayAudioCoroutine;
    private bool _coroutineRunning;
    
    private void Awake()
    {
      
        _audioSource = gameObject.GetComponent<AudioSource>();
        _playerState = PlayerStates.PlayerState;
        _coroutineRunning = false;
    }

    public void PlayAudio(AudioClip clip, bool loop = false)
    {
        _audioSource.clip = clip;
        _audioSource.loop = loop;
        _audioSource.Play();
    }

    public void StopAudio()
    {
        _audioSource.Stop();
    }
    void FixedUpdate()
    {
        PlayerStates.States newPlayerState = PlayerStates.PlayerState;
        if (newPlayerState != _playerState)
        {
            PlayPlayerStateSound(newPlayerState);
        }
        

        _playerState = newPlayerState;
    }
    public void PlayPlayerStateSound(PlayerStates.States state)
    {
        switch (state)
        {

            case PlayerStates.States.Idle:
                if (_audioSource.loop)
                    StopAudio();
                break;
            case PlayerStates.States.Walk:
                //this.StopCoroutine(PlayAudioCoroutine);
                //PlayAudioCoroutine = this.StartCoroutine(PlayAudioPeriodically(_walkClip, 1.0f));
                PlayAudio(_walkClip, true);

                break;
            case PlayerStates.States.Run:
                PlayAudio(_runClip, true);
                break;
            case PlayerStates.States.Jump:
                PlayAudio(_jumpClip);
                break;
            case PlayerStates.States.Landed:
                PlayAudio(_landClip);
                break;
        }
    }

    //public IEnumerator PlayAudioPeriodically(AudioClip clip, float delay)
    //{
    //    _coroutineRunning = true;
    //    while (true)
    //    {
    //        PlayAudio(clip);
    //        yield return new WaitForSeconds(delay);
    //    }
    //}
}