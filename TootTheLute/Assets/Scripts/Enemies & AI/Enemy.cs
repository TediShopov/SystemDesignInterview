using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoidBase))]
public class Enemy : MonoBehaviour
{
    private bool wasAttackedPreviousFrame = false;
    private bool freeMovement = true;
    private Vector2 lastAttackDirection = Vector2.zero;
    private Health health;

    protected Collider2D collider2D;
    protected Rigidbody2D rigidbody2D;
    protected BoidBase boidBase;
    protected Vector2 vel;

    public float InitialInvulnerabilitySeconds = 0.5f;
    public Room Room;
    public GameObject AttackTarget;
    public ParticleSystem ParticleSystem;

    public SpriteRenderer shadow2D;
    public Animator Animator;

    //For Scheduling enemy behaviours on the beat
    public AIConductor AIConductor;

    public Coroutine Immobilized;

    [Header("Sound")]
    public AK.Wwise.Event OnDeathSound;
    public AK.Wwise.Event OnHurtSound;

    public Vector2 GetDirectionToTarget() => (AttackTarget.transform.position - transform.position).normalized;

    public void Awake()
    {
        //Find reference to a player controller in the scene
        // and set it as attack target
        AttackTarget = Object.FindObjectOfType<PlayerController2D>().gameObject;
        AIConductor = Object.FindObjectOfType<AIConductor>();
        boidBase = this.GetComponent<BoidBase>();
        freeMovement = true;
    }
    // Start is called before the first frame update
    public void Start()
    {
        health = GetComponent<Health>();
        collider2D = GetComponent<Collider2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        Animator = GetComponent<Animator>();
        health.OnDeath += OnDeath;
    }
    public void OnEnable()
    {
        StartCoroutine(StartInvulnerabilityAfterOneFrame());
    }

    private void FixedUpdate()
    {
        if (AttackTarget == null) return;
        if (boidBase == null) return;

        if (wasAttackedPreviousFrame)
        {
            this.ApplyImpulse(lastAttackDirection * health.LastAttack.Knockback, true, 0.1f);
            this.health.SetInvulnarability(0.3f);
            //MoveTowardsDirection(-health.LastAttack.Knockback);
            wasAttackedPreviousFrame = false;
        }
        else
        {
            if (freeMovement)
            {
                ApplySteerMovement();
            }
        }
        UpdateAnimationSprite();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Attack"))
        {
            var ad = collision.gameObject.GetComponent<AttackData>();
            if (health.TakeDamage(ad))
            {
                if (OnHurtSound != null)
                    OnHurtSound.Post(this.gameObject);
                this.Animator.SetTrigger("Damaged");
                if (this.ParticleSystem != null)
                {
                    var apos = (Vector2)this.AttackTarget.gameObject.transform.position;

                    lastAttackDirection = ((Vector2)this.collider2D.ClosestPoint(apos) - apos).normalized;

                    float angle = Mathf.Atan2(lastAttackDirection.y, lastAttackDirection.x) * Mathf.Rad2Deg;
                    this.ParticleSystem.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                    this.ParticleSystem.Play();
                }
                wasAttackedPreviousFrame = true;
            }
        }
    }


    //Disable enemy fully after animation
    public void OnDeath(GameObject obj)
    {
        if (OnDeathSound != null)
            OnDeathSound.Post(this.gameObject);
        this.enabled = false;
        this.boidBase.enabled = false;
        this.collider2D.enabled = false;
        this.shadow2D.enabled = false;
        this.Animator.SetTrigger("Death");
        this.Animator.SetBool("IsDead", true);
    }
    private IEnumerator StartInvulnerabilityAfterOneFrame()
    {
        yield return null; // Wait one frame to ensure Unity finishes setup
        health.SetInvulnarability(InitialInvulnerabilitySeconds);
    }



    public IEnumerator ImmobilizeFor(float secs)
    {
        freeMovement = false;
        yield return new WaitForSeconds(secs);
        freeMovement = true;
    }

    public bool IsMovementAllowed => freeMovement;

    protected virtual void UpdateAnimationSprite()
    {
        Vector2 v = vel;

        //Debug.Log($"Animator: vel:{v}");
        if (v.magnitude < 0.2)
            Animator.SetInteger("Slime9Dir", 0);
        else
        {
            float minAngle = 360 / 16;
            if (Vector2.Angle(v, Vector2.up) < minAngle)
                Animator.SetInteger("Slime9Dir", 1);
            else if (Vector2.Angle(v, new Vector2(1, 1).normalized) < minAngle)
                Animator.SetInteger("Slime9Dir", 2);
            else if (Vector2.Angle(v, Vector2.right) < minAngle)
                Animator.SetInteger("Slime9Dir", 3);
            else if (Vector2.Angle(v, new Vector2(1, -1)) < minAngle)
                Animator.SetInteger("Slime9Dir", 4);
            else if (Vector2.Angle(v, Vector2.down) < minAngle)
                Animator.SetInteger("Slime9Dir", 5);
            else if (Vector2.Angle(v, new Vector2(-1, -1)) < minAngle)
                Animator.SetInteger("Slime9Dir", 6);
            else if (Vector2.Angle(v, Vector2.left) < minAngle)
                Animator.SetInteger("Slime9Dir", 7);
            else if (Vector2.Angle(v, new Vector2(-1, 1)) < minAngle)
                Animator.SetInteger("Slime9Dir", 8);
        }
    }

    protected void ApplySteerMovement()
    {
        rigidbody2D.velocity = Vector3.zero;
        if (boidBase == null) return;
        Vector2 steeringForce = boidBase.CalculateSteering(this.AttackTarget.transform.position);
        Vector2 acceleration = Vector2.ClampMagnitude(steeringForce, boidBase.MaxForce);
        vel = Vector2.ClampMagnitude(vel + acceleration, boidBase.MaxSpeed);
        boidBase.Velocity = vel;
        Vector2 newPosition = rigidbody2D.position + vel * Time.fixedDeltaTime;
        rigidbody2D.MovePosition(newPosition);
    }

    private void ApplyImpulse(Vector2 impulse, bool lockMovement = true, float d = 1)
    {
        StartCoroutine(ApplyImpulseCoroutine(impulse, lockMovement, d));
    }

    private IEnumerator ApplyImpulseCoroutine(Vector2 impulse, bool lockMovement = true, float d = 1)
    {
        if (lockMovement)
            freeMovement = false;

        this.rigidbody2D.velocity = Vector2.zero;
        this.rigidbody2D.AddForce(impulse, ForceMode2D.Impulse);
        yield return new WaitForSeconds(d);

        if (lockMovement)
            freeMovement = true;
    }

    protected void MoveTowardsDirection(float moveSpeed)
    {
        Vector2 moveDirection = AttackTarget.transform.position - rigidbody2D.transform.position;
        moveDirection.Normalize();
        Vector2 movement = moveDirection * moveSpeed * Time.fixedDeltaTime;
        rigidbody2D.MovePosition(rigidbody2D.position + movement);
    }
}