using UnityEngine;

public class ButtonPrompTween : MonoBehaviour
{
    public LeanTweenType TweenType;

    public float Duration;
    public float Delay;

    public Vector3 RelativeScale;
    // Start is called before the first frame update
    void Start()
    {

        var finalScale = new Vector3(RelativeScale.x * this.gameObject.transform.localScale.x, RelativeScale.y * this.gameObject.transform.localScale.y, RelativeScale.z * this.gameObject.transform.localScale.z);
        LeanTween.scale(this.gameObject, finalScale, Duration).setLoopPingPong().setEase(TweenType);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
