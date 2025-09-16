using UnityEngine;

public class SoundSpawner : MonoBehaviour
{
    [SerializeField] public GameObject SoundPrefab;
    [Range(0, 100)]
    [SerializeField] public float MinimumSoundDistance;
    [SerializeField] public float SoundDuration;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCollisionEnter2D(Collision2D collision2D)
    {
        SoundGenerator.Instance.SpawnSound(collision2D, this.gameObject);

    }
}
