using UnityEngine;

public class LeanTweenDebug : MonoBehaviour
{
    public LeanTweenType InType;
    // Start is called before the first frame update
    void Start()
    {
        LeanTween.scale(this.gameObject, new Vector3(3, 3, 3), 0.5f).setLoopPingPong().setEase(InType);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
