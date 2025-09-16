using System;
using UnityEngine;

public class SoundGenerator : MonoBehaviour
{

    public static SoundGenerator Instance = null;
    // Start is called before the first frame update
    [Range(0, 1000)]
    [SerializeField] public float DefaultSoundModifier = 0.1f;

    public SoundExpansionAnimation SoundPrefab;

    [Range(0.1f, 200)]
    public float BigestSoundRadiusAllowed;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;


        this.transform.localScale = new Vector3(0, 0, 0);

    }


    public static float GetSoundModifier(GameObject obj)
    {
        if (obj == null)
        {
            return 1;
        }

        var properties = obj.GetComponent<Properties>();
        if (properties != null)
        {
            return properties.SoundModifier;
        }



        properties = obj.GetComponentInParent<Properties>();
        if (properties != null)
        {
            return properties.SoundModifier;
        }
        return 1;
    }

    public float CalculateSoundDistance(Collision2D collision2D)
    {

        var colidingObjectOne = collision2D.collider.gameObject;
        var colidingObjectTwo = collision2D.otherCollider.gameObject;

        var soundModifierOne = GetSoundModifier(colidingObjectOne);
        var soundModifierTwo = GetSoundModifier(colidingObjectTwo);

        return CalculateSoundDistance(Helpers.GetImpactStrength(collision2D), soundModifierOne, soundModifierTwo);
    }

    public float CalculateSoundDistance(float force, float modifier1, float modifier2)
    {
        //Set position of sound 
        //this.gameObject.transform.position = contactPoint;

        //Set radius of sound
        float averageModifier = (modifier1 + modifier2) / 2.0f;
        float radius = force * averageModifier * DefaultSoundModifier;
        float finalRadius = Math.Min(radius, BigestSoundRadiusAllowed);


        return finalRadius;


        return radius;

    }

    public void SpawnSound(Collision2D collision2D, GameObject caster = null)
    {
        Vector3 soundPosition = collision2D.GetContact(0).point;
        var sound = Instantiate(SoundPrefab, soundPosition, Quaternion.identity);
        sound.SetupSoundObject(this.CalculateSoundDistance(collision2D), caster);
    }

    public void SpawnSound(float force, Vector2 contactPoint, float modifier1, float modifier2, GameObject caster = null)
    {
        Vector3 soundPosition = contactPoint;
        var sound = Instantiate(SoundPrefab, soundPosition, Quaternion.identity);
        sound.SetupSoundObject(this.CalculateSoundDistance(force, modifier1, modifier2), caster);
    }

    //void OnDrawGizmos()
    //{
    //    Gizmos.DrawWireSphere(this.transform.position, BigestSoundRadiusAllowed);
    //}
}
