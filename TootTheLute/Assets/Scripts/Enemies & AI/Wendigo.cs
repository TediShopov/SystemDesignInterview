using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR

using UnityEditor;

#endif

public enum WendigoState
{
    Exhausted = 0,
    RushAttacking = 1,
    Charge = 2,
    Roar = 3
}

[RequireComponent(typeof(MeleeAttack))]
public class Wendigo : Enemy
{
    [Header("Wendigo State Properties")]
    public WendigoState State;

    private WendigoState PrevState;
    public bool shouldSwitchAttackPattern = true;
    public int AttacksTillExhaustion;
    public int CurrentStateCount;
    public int PreviousDirection = 0;

    [Header("Exhausted State Properties")]
    public float ExhaustedDuration;

    [Header("Melee Attack Properties")]
    public float MeleeAttackWeight = 1;   // Weight of the attack to get picked

    public float MeleeAttackDuration;
    private MeleeAttack meleeAttack;

    [Header("Charge Attack Properties")]
    public float ChargeAttackWeight = 1;   // Weight of the attack to get picked

    public float ChargeAttackDuration;
    public float ChargeSpeedMultiplier = 5;
    public float ChargeForceMultiplier = 5;
    public Collider2D ChargeCollider;
    public AK.Wwise.Event OnChargeSound;

    [Header("Roar Attack Properties")]
    public float RoarAttackWeight = 1;   // Weight of the attack to get picked

    public float RoarAttackTime;

    public Tilemap TilemapCollider;
    public LayerMask RoomCollisionLayer;
    public AK.Wwise.Event OnRoarSound;

    public Vector2 chargeDirection = Vector2.zero;
    //private BoidBase BoidBase;
    // Start is called before the first frame update
    private void Start()
    {
        base.Start();
        shouldSwitchAttackPattern = true;
        this.State = WendigoState.RushAttacking;
    }
    private void Awake()
    {
        base.Awake();
        meleeAttack = GetComponent<MeleeAttack>();
    }

    private void OnDestroy()
    {
        if (this.AttackTarget != null)
        {
            var p = this.AttackTarget.GetComponent<PlayerController2D>();
            if (p.IsGamePaused == false && p)
            {
                p.IsKilledWendigo = true;
                SceneManager.LoadScene(3);
            }
        }
    }
    private void FixedUpdate()
    {
        if (ChargeCollider != null)
        {
            if (this.State == WendigoState.Charge)
            {
                ChargeCollider.gameObject.SetActive(true);
                ChargeCollider.gameObject.GetComponent<AttackData>().ForcedKnockbackDirection = GetPushAwayFromChargeVector();
            }
            else
            {
                ChargeCollider.gameObject.SetActive(false);
            }
        }

        if (shouldSwitchAttackPattern == true)
        {
            //Exhaust afer X attacks
            if ((CurrentStateCount) % (AttacksTillExhaustion + 1) == 0)
            {
                State = WendigoState.Exhausted;
                //CurrentAttackCount = 0;
            }
            //Or Pick new attack at random
            else
            {
                // first value is zero so there is no chance of picking exhausted as state
                float[] weightsOfAttacks = { 0, MeleeAttackWeight, ChargeAttackWeight, RoarAttackWeight };
                int newStateIndex = GetRandomIndexWeighted(weightsOfAttacks);
                Debug.Log($"New State Index: {newStateIndex}");
                State = (WendigoState)newStateIndex;
            }
        }

        if (AttackTarget == null)
            return;
        if (boidBase == null) return;

        //Change state.
        //Fixed updates for each wendigo state
        if (State == WendigoState.Exhausted)
        {
            meleeAttack.enabled = false;
            UpdateAnimationSprite();

            if (shouldSwitchAttackPattern)
                StartStateTimer(ExhaustedDuration);
        }
        else if (State == WendigoState.RushAttacking)
        {
            UpdateAnimationSprite();
            ApplySteerMovement();
            meleeAttack.enabled = true;

            if (shouldSwitchAttackPattern)
                StartStateTimer(MeleeAttackDuration);
        }
        else if (State == WendigoState.Charge)
        {
            if (PrevState != State)
            {
                if (OnChargeSound != null)
                    this.OnChargeSound.Post(this.gameObject);
            }
            UpdateAnimationSprite();
            ApplyChargeMovement();
            meleeAttack.enabled = false;
            if (shouldSwitchAttackPattern)
            {
                chargeDirection = Vector2.zero;
                StartStateTimer(ChargeAttackDuration);
            }
        }
        else
        {
            if (PrevState != State)
            {
                Animator.SetTrigger("Howl");

                if (OnRoarSound != null)
                    this.OnRoarSound.Post(this.gameObject);
                //TODO InteractWithEnvironment
                var turrets = Room.GetOverlappingObjects<EnemyTurret>();
                foreach (var t in turrets)
                {
                    t.SpawnObjectInCircularPattern();
                }
            }
            if (shouldSwitchAttackPattern)
                StartStateTimer(RoarAttackTime);

            UpdateAnimationSprite();
            meleeAttack.enabled = false;
        }
        PrevState = State;

        //Already handled
        if (shouldSwitchAttackPattern)
        {
            CurrentStateCount++;
            shouldSwitchAttackPattern = false;
        }

        //base.FixedUpdate();
    }
    protected override void UpdateAnimationSprite()
    {
        if (State == WendigoState.Exhausted)
            Animator.SetBool("IsIdle", true);
        else
            Animator.SetBool("IsIdle", false);

        if (State == WendigoState.Charge)
            Animator.SetBool("IsCharging", true);
        else
            Animator.SetBool("IsCharging", false);
        Vector2 v = vel;

        //Debug.Log($"Animator: vel:{v}");
        if (v.magnitude < 0.2)
        {
        }
        else
        {
            float minAngle = 360 / 16;
            if (Vector2.Angle(v, Vector2.up) < minAngle)
                Animator.SetInteger("Slime9Dir", 1);

            if (Vector2.Angle(v, new Vector2(1, 1).normalized) < minAngle)
                Animator.SetInteger("Slime9Dir", 2);

            if (Vector2.Angle(v, Vector2.right) < minAngle)
                Animator.SetInteger("Slime9Dir", 3);

            if (Vector2.Angle(v, new Vector2(1, -1)) < minAngle)
                Animator.SetInteger("Slime9Dir", 4);

            if (Vector2.Angle(v, Vector2.down) < minAngle)
                Animator.SetInteger("Slime9Dir", 5);

            if (Vector2.Angle(v, new Vector2(-1, -1)) < minAngle)
                Animator.SetInteger("Slime9Dir", 6);

            if (Vector2.Angle(v, Vector2.left) < minAngle)
                Animator.SetInteger("Slime9Dir", 7);

            if (Vector2.Angle(v, new Vector2(-1, 1)) < minAngle)
                Animator.SetInteger("Slime9Dir", 8);
            PreviousDirection = Animator.GetInteger("Slime9Dir");
        }
    }

    private Vector2 GetRandom2DVector() => Random.onUnitSphere.normalized;

    public void StartStateTimer(float duration)
    {
        StartCoroutine(SetBoolTrueAfterTime(duration));
    }

    public Vector2 GetPushAwayFromChargeVector()
    {
        return Vector2.Perpendicular(chargeDirection);
    }

    public void OnDrawGizmosSelected()
    {
        if (AttackTarget != null)
        {
            Vector2 origin = AttackTarget.transform.position;
            Vector2 direction = Vector2.Perpendicular(chargeDirection);
            Gizmos.DrawRay(origin, direction * 3);
        }
    }

    private IEnumerator SetBoolTrueAfterTime(float seconds)
    {
        Debug.Log($"Wending Changin state after = {seconds}");
        yield return new WaitForSeconds(seconds);
        Debug.Log($"Wending Changin State Now");
        shouldSwitchAttackPattern = true;
    }

    //Room normals
    public List<Vector2> IsoNormal = new List<Vector2>
    {
       new Vector2(1,1), // Top-Right
       new Vector2(1,-1), // Bottom-Right
       new Vector2(-1,-1), // Bottom-Left
       new Vector2(-1,1) // Top-Left
    };

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int thisLayer = collision.gameObject.layer;
        int thisLayerMask = 1 << thisLayer;
        if ((RoomCollisionLayer.value & thisLayerMask) != 0)
        {
            //chargeDirection = GetAverageContactNormal(collision);
            Vector2 isoNormal = Vector2.zero;
            //            float angleThreshold = 360 / 8;
            //
            //            if (Vector2.Angle(chargeDirection.normalized, IsoNormal[0].normalized) <= angleThreshold)
            //            {
            //                Debug.Log($"Reflection Surface Top-Right");
            //                isoNormal = IsoNormal[0];
            //            }
            //            else if (Vector2.Angle(chargeDirection.normalized, IsoNormal[1].normalized) <= angleThreshold)
            //            {
            //                Debug.Log($"Reflection Surface Bottom-Right");
            //                isoNormal = IsoNormal[1];
            //            }
            //            else if (Vector2.Angle(chargeDirection.normalized, IsoNormal[2].normalized) <= angleThreshold)
            //            {
            //
            //                Debug.Log($"Reflection Surface Bottom-Left");
            //                isoNormal = IsoNormal[2];
            //            }
            //            else
            //            {
            //                Debug.Log($"Reflection Surface Top-Left");
            //                isoNormal = IsoNormal[3];
            //            }

            // Get first contact point
            //---ANOTHER IDEA IS CHECK THE COLLIDER TILES --
            //            ContactPoint2D contact = collision.GetContact(0);
            //            Vector2 collisionPoint = contact.point;
            //
            //            // Extend the point slightly in the opposite direction of the normal
            //            //Vector2 checkPoint = collisionPoint - (contact.normal * 0.3f);
            //            Vector2 checkPoint = collisionPoint + vel.normalized*0.1f;
            //
            //            // Convert world position to tilemap cell position
            //            Vector3Int cellPosition = TilemapCollider.WorldToCell(checkPoint);
            //            // Get the tile at that position
            //            TileBase tile = TilemapCollider.GetTile(cellPosition);
            //
            //            if (tile != null)
            //            {
            //                Debug.Log($"Tile: {tile.name}");
            //            }
            //            else
            //            {
            //                Debug.Log($"Tile: Null");
            //            }

            //For now it is based on the actual normal of the 2D Physics simulation
            //Even though that would not look accurate as we are trying to depict a 3D world
            isoNormal = GetAverageContactNormal(collision);
            isoNormal.x = -isoNormal.x;
            isoNormal.y = -isoNormal.y;

            chargeDirection = Vector2.Reflect(chargeDirection.normalized, isoNormal.normalized);
        }
    }

    public static int GetRandomIndexWeighted(float[] weights)
    {
        if (weights == null || weights.Length == 0)
            throw new System.ArgumentException("Weights array cannot be null or empty.");

        float totalWeight = 0f;
        foreach (float weight in weights)
        {
            if (weight < 0f)
                throw new System.ArgumentException("Weights cannot be negative.");
            totalWeight += weight;
        }

        if (totalWeight == 0f)
            throw new System.ArgumentException("Total weight must be greater than zero.");

        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        for (int i = 0; i < weights.Length; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue < cumulativeWeight)
                return i;
        }

        return weights.Length - 1; // Fallback (shouldn't be reached)
    }

    private Vector2 GetAverageContactNormal(Collision2D collision)
    {
        Vector2 sumNormal = Vector2.zero;
        int contactCount = collision.contactCount;

        for (int i = 0; i < contactCount; i++)
        {
            sumNormal += collision.GetContact(i).normal;
        }

        return contactCount > 0 ? sumNormal / contactCount : Vector2.zero;
    }
    protected void ApplyChargeMovement()
    {
        if (chargeDirection == Vector2.zero)
        {
            chargeDirection = AttackTarget.transform.position - this.transform.position;
            //chargeDirection = Random.onUnitSphere;
            chargeDirection.Normalize();
        }

        if (boidBase == null) return;

        Vector2 steeringForce = chargeDirection * boidBase.MaxForce * ChargeForceMultiplier;
        Vector2 acceleration = Vector2.ClampMagnitude(steeringForce, boidBase.MaxForce * ChargeForceMultiplier);
        vel = Vector2.ClampMagnitude(vel + acceleration, boidBase.MaxSpeed * ChargeSpeedMultiplier);
        Vector2 newPosition = rigidbody2D.position + vel * Time.fixedDeltaTime;
        rigidbody2D.MovePosition(newPosition);
    }
}