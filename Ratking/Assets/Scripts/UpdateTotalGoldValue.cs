using System;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

public class UpdateTotalGoldValue : MonoBehaviour
{
    public TMP_Text Text;
    private AudioSource _audioSource;
    public float TicksMs;
    public int RateOfChange;
    public int MinRateOfChange;
    public int MaxRateOfChange;
    public ParticleSystem ParticleSystem;

    [Header("Animation Related")]
    [Range(0.1f, 0.5f)]
    public float PercentStretch = 0.2f;
    public float PitchRangeMin;
    public float PitchRangeMax;
    //the value shown will be incrementing every frame, by some rate to reach
    //the actual value of gold the player has
    private int _current_shown;
    private int _value;
    private Coroutine _increment_coroutine;
    private int _times_effect_played = 0;
    
    public int ValueDifference => Math.Abs(_value - _current_shown);
    void Start()
    {
        this._audioSource = GetComponent<AudioSource>();
        LevelData.Inventory.OnInventoryUpdated += UpdateLabel;
        _value = 0;
        _current_shown = 0;
       // LevelData.PlayerObject.GetComponent<Inventory>().OnInventoryUpdated += UpdateLabel;
        UpdateLabel();
    }

    // Update is called once per frame
    void UpdateLabel()
    {
        if (Text != null)
        {
            this._value = LevelData.Inventory.TotalGoldValue;
            var screenToWorld = Camera.main.ScreenToWorldPoint(gameObject.transform.position);
            this.ParticleSystem.gameObject.transform.position = screenToWorld;
            if (_value > _current_shown)
            {
                this._increment_coroutine = StartCoroutine(IncrementTo());
            }
            else
            {
                SetLabelWithEffects(_value);
            }
        }
    }

    private void SetLabelWithEffects(int value)
    {
        _current_shown = value;
        Text.text = _current_shown.ToString();
        //Modulate Pitch based on how many sound are played for this gold increment
        var next_pitch = Helpers.ConvertToNewRange(_times_effect_played, 0, MaxRateOfChange, PitchRangeMin, PitchRangeMax);
        _audioSource.pitch = next_pitch + UnityEngine.Random.Range(-0.05f, 0.05f);
        _audioSource.PlayOneShot(_audioSource.clip, UnityEngine.Random.Range(0.75f, 1.0f));
        //Refresh Tween Animation State
        RefreshSquashStretch();
        ParticleSystem.Play();
    }

    IEnumerator IncrementTo()
    {
        int timesToUpdateToValue = Helpers.ClampToRangeInt(ValueDifference / RateOfChange,MinRateOfChange,MaxRateOfChange);
        int curr_rate_of_change = RateOfChange; 
        if (timesToUpdateToValue == MaxRateOfChange)
        {
            curr_rate_of_change = ValueDifference / MaxRateOfChange;
        }

        for (int i = 0; i < timesToUpdateToValue-1; i++)
        {
            _times_effect_played++;
            SetLabelWithEffects(_current_shown + Math.Min(curr_rate_of_change, ValueDifference));
            yield return new WaitForSecondsRealtime(TicksMs);
        }
        SetLabelWithEffects(_value);
        _times_effect_played = 0;
        StopCoroutine(_increment_coroutine);
    }

    public void RefreshSquashStretch()
    {
        var volume = this.transform.localScale.x * this.transform.localScale.y;
        if (!LeanTween.isTweening(this.gameObject))
        {
            LeanTween.scaleX(this.gameObject,this.transform.localScale.x - volume*PercentStretch,TicksMs).setEaseInBounce().setLoopPingPong(1);
            LeanTween.scaleY(this.gameObject,this.transform.localScale.x + volume*PercentStretch,TicksMs).setEaseInBounce().setLoopPingPong(1);
        }

        
    }
}
