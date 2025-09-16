using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class VictoryScreenSats : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI Value;
    // Start is called before the first frame update
    void Start()
    {
        Value.text = ($"X  ") + PlayerPrefs.GetString("TotalGoldValue");
    }

}
