using UnityEngine;


public class SoundExpansionAnimation : MonoBehaviour
{
    public GameObject Caster;
    [SerializeField] private CircleCollider2D SoundCollider;
    [SerializeField] private SpriteRenderer DebugSoundSprite;
    public float TimeOfExpansion = 1.0f;
    [SerializeField] public AnimationCurve SoundExpansionCurve;
    private float _maxRadiusToReach;

    public float _timeExpanding = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        SoundCollider = this.GetComponent<CircleCollider2D>();
        DebugSoundSprite = this.GetComponent<SpriteRenderer>();
        SoundCollider.enabled = false;
    }

    public void OnExpandedFully()
    {
        SoundCollider.enabled = true;
        Destroy(this.gameObject, 0.2f);
    }


    public void SetupSoundObject(float radius, GameObject caster = null)
    {
        Caster = caster;
        this.gameObject.transform.localScale = new Vector3(0, 0, 0);
        _timeExpanding = 0;
        _maxRadiusToReach = radius;
    }

    void FixedUpdate()
    {
        _timeExpanding += Time.fixedDeltaTime;

        float timePassed = _timeExpanding / TimeOfExpansion;

        Vector3 defaultScale = new Vector3(1, 1, 1);
        if (_timeExpanding <= TimeOfExpansion)
        {
            this.transform.localScale = SoundExpansionCurve.Evaluate(timePassed) * _maxRadiusToReach * defaultScale;
        }
        else
        {
            this.transform.localScale = 1 * _maxRadiusToReach * defaultScale;
            OnExpandedFully();
        }

    }


    void Update()
    {

    }
}
