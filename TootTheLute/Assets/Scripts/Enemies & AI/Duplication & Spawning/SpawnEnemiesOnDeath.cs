using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]

//Spawn more enemies when health reaches zero.
public class SpawnEnemiesOnDeath : MonoBehaviour
{


    private Health health;
    private Enemy enemyEmitter;
    public SlimeDroplet DropletPrefab;
    public List<GameObject> EnemiesToSpawnOnDeath;
    public LayerMask LayerMask;
    public float SpawnPointOffset = 2.0f;
    public float RandomXRange = 0.15f;
    public float RandomYRange = 0.15f;

    [Header("Sound")]
    public AK.Wwise.Event SplitSound;




    // Start is called before the first frame update
    void Start()
    {

        health = GetComponent<Health>();
        enemyEmitter = GetComponent<Enemy>();
        health.OnDeath += SpawnEnemiesInOppositeDirectionOfPlayer;

        
    }
    public void SpawnEnemiesInOppositeDirectionOfPlayer(GameObject obj)
    {
        if (SplitSound != null)
            SplitSound.Post(this.gameObject);
        //List<Vector2>  spawnPositions = GenerateFilteredPoints(EnemiesToSpawnOnDeath.Count);

        Vector2 baseDirection = this.enemyEmitter.transform.position - this.enemyEmitter.AttackTarget.transform.position;
        List<Vector2> directions = ConeUtils2D.EvenlySpacedDirectionsInCone2D(baseDirection, -15, 15, 2);
        for (int i = 0; i < directions.Count; i++)
        {
            var direction = directions[i];
            this.DropletPrefab.EnemyPrefab = (EnemiesToSpawnOnDeath[i]);
            this.DropletPrefab.InitialImpulse = direction.normalized * 75;
            this.DropletPrefab.Emitter = this.enemyEmitter;
            Instantiate(this.DropletPrefab, (this.transform.position), Quaternion.identity);
            this.enemyEmitter.Room.SpawnerInPlay.Add(this.GetHashCode());
        }

    }

    List<Vector2> GenerateFilteredPoints(int count)
    {
        List<Vector2> points = new List<Vector2>();

        //Get opposite vector of player
        Vector2 deterministicSpawnPosition =( this.transform.position - enemyEmitter.AttackTarget.transform.position).normalized;
        //Mulitple by offset
        deterministicSpawnPosition *= SpawnPointOffset;
        deterministicSpawnPosition = (Vector2)this.transform.position + deterministicSpawnPosition;
        Debug.Log($"Enemy Position:{this.transform.position}");

        Debug.Log($"Deterministic Spawn Position:{deterministicSpawnPosition}");


        for (int i = 0; i < count; i++)
        {
            Vector2 attemptedSpawnPoint = deterministicSpawnPosition;
            //Apply small randomess to the coordinate
            attemptedSpawnPoint += new Vector2(Random.Range(-RandomXRange, RandomXRange), Random.Range(-RandomYRange, RandomYRange));
            Debug.Log($"Attempted Spawn Position:{attemptedSpawnPoint}");

            //Raycast to the point to ensure it is in the bound of the level
            RaycastHit2D rayHit = Physics2D.Linecast(this.gameObject.transform.position, attemptedSpawnPoint,LayerMask.value);
            Vector2 actualSpawnPosition = attemptedSpawnPoint;
            if (rayHit)
                actualSpawnPosition = rayHit.point;
            Debug.Log($"Actual Spawn Position:{actualSpawnPosition}");
            points.Add(actualSpawnPosition);
        }
        return points;
    }

}

