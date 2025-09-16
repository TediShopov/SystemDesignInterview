using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmellCooldownBar : MonoBehaviour
{

    public Slider slider;
    public Gradient gradient;
    public Image fill;
    [SerializeField]
    private SmellAbility _smellAbility;

    private float _normalizedValue;

    private float _barLifetime;

    void Awake()
    {
        slider.maxValue = SmellAbility.SMELL_COOLDOWN;
        slider.minValue = 0;

        fill.gameObject.SetActive(false);
        this.gameObject.SetActive(false);

    }
    void Update()
    {
        slider.value = SmellAbility.SMELL_COOLDOWN - _smellAbility.SmellCooldown;

        fill.color = gradient.Evaluate(slider.normalizedValue);

        if (slider.value >= slider.maxValue)
            _barLifetime -= Time.deltaTime;
        if (_barLifetime <= 0)
        {
            fill.gameObject.SetActive(false);
            this.gameObject.SetActive(false);
        }
    }
    public void Activate()
    {
        slider.value = SmellAbility.SMELL_COOLDOWN - _smellAbility.SmellCooldown;
        //fill.color = gradient.Evaluate(1);
        
        _normalizedValue = slider.value / SmellAbility.SMELL_COOLDOWN;
        fill.color = gradient.Evaluate(_normalizedValue);
        //fill.color = gradient.Evaluate(slider.normalizedValue);
        _barLifetime = 0.5f;
        fill.gameObject.SetActive(true);
        this.gameObject.SetActive(true);
    }

   


}