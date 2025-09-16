using UnityEngine;

[RequireComponent(typeof(Properties))]
public class CollectibleGhost : MonoBehaviour
{
    public int UniquenessHash = 0;
    public int DamageThreshold = 1500;
    [SerializeField] public Properties SoundProperties;
    [SerializeField] public Thrower ThrowerScript;
    private void Awake()
    {
        if (UniquenessHash == 0)
        {
            UniquenessHash = this.gameObject.GetHashCode();

        }

        this.SoundProperties = this.gameObject.GetComponent<Properties>();
    }

    public override int GetHashCode()
    {
        return UniquenessHash;

    }



    void OnCollisionEnter2D(Collision2D collision)
    {

        PredictedSoundInfo soundInfo = new PredictedSoundInfo();
        if (Helpers.GetImpactStrength(collision) > DamageThreshold)
        {
            // Debug.Log("Fragile Item broken");
            Destroy(this.gameObject);
            SoundProperties.SoundModifier *= 1.5f;
            soundInfo.IsBreaking = true;
        }

        Vector3 scaleOfSoundArea = new Vector3(1, 1, 1) * SoundGenerator.Instance.CalculateSoundDistance(collision);
        scaleOfSoundArea.z = 1;
        soundInfo.WorldPosition = this.transform.position;
        soundInfo.LocalScale = scaleOfSoundArea;
        ThrowerScript.PredictedSoundPositions.Add(soundInfo);
    }



    [SerializeField] public InventoryItemData InventoryItemData;
}