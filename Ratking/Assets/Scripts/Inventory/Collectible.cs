using UnityEngine;

[RequireComponent(typeof(Properties))]
[RequireComponent(typeof(SpriteBreaker))]
public class Collectible : MonoBehaviour
{
    [HideInInspector] public int UniquenessHash = 0;
    public int DamageThreshold = 1500;
    [HideInInspector] public Properties SoundProperties;
    [HideInInspector] public SpriteBreaker SpriteBreaker;
    [SerializeField] public bool IsEmittingSound=true;
    [SerializeField] public float CullSoundsBelowStrength;
    private void Start()
    {
        if (UniquenessHash == 0)
        {
            UniquenessHash = this.gameObject.GetHashCode();

        }

        SoundProperties = this.GetComponent<Properties>();
        this.SpriteBreaker = this.gameObject.GetComponent<SpriteBreaker>();
    }

    public override int GetHashCode()
    {
        return UniquenessHash;

    }



    void OnCollisionEnter2D(Collision2D collision)
    {
        if (Helpers.GetImpactStrength(collision) > DamageThreshold)
        {
            Debug.Log("Fragile Item broken");
            SpriteBreaker.Break();
            SoundProperties.SoundModifier *= 1.5f;
            SoundGenerator.Instance.SpawnSound(collision, this.gameObject);
            IsEmittingSound=false;
        }
        else if(IsEmittingSound && Helpers.GetImpactStrength(collision) > CullSoundsBelowStrength)
        {
            SoundGenerator.Instance.SpawnSound(collision, this.gameObject);
        }
    }



    [SerializeField] public InventoryItemData InventoryItemData;
}
