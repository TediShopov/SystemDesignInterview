using UnityEngine;

public class GlobalProgress : MonoBehaviour
{

    public static GlobalProgress Instance { get; private set; }

    public KeyBasedProgression RewardBar;
    public KeyBasedProgression DangerBar;

    public BarProgressData BarProgressData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

    }




}
