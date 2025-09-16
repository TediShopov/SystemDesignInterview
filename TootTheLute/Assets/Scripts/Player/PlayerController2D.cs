using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PlayerController2D : MonoBehaviour
{

    private Vector2 moveInput; // Stores movement input
    private Rigidbody2D rb; // Reference to the Rigidbody2D component

    public OrbitalApproachingCircle OrbitalApproachingCircle;
    
    [Header("Structure")]
    public ParticleSystem   AttackPS;
    public ParticleSystem   SpecialAttackPS;
    public Animator         Animator;
    public BeatmapTrack     BeatmapTrack;
    public Animator         AttackAnimator;
    public GameObject       AttackCollider;
    public GameObject       MouseRotationObject;

    [Header("Debug")]
    public bool RequireInputToAttack = true;
    public bool RequireInputToDash = true;
    public bool IsKilledWendigo = false;

    [Header("Health")]
    public Health    HealthComponent;
    [SerializeField] public float DTakeDamage       = 25;
    [SerializeField] public float KnockBackDuration = 1;



    [Header("Movement")]
    [HideInInspector] private bool dashInput        = false;

    [SerializeField] private float moveSpeed        = 5f; // Configurable movement speed
    [SerializeField] private float maxSpeed         = 5f; 
    [SerializeField] private float acceleration     = 10f;
    [SerializeField] private float deceleration     = 8f;
    [SerializeField] private float inputDeadzone    = 0.1f;

    [Header("Dash")]
    [SerializeField] public float dashForce         = 10f;  // Strength of the dash
    [SerializeField] public float dashDuration      = 0.2f; // How long the dash lasts
    [SerializeField] public float dashCooldown      = 1f; // Time before another dash
    [SerializeField] public float DashInvulnarabilitySeconds = 0.5f;
    [SerializeField] private bool freeMovement      = true;



    public bool IsDead => HealthComponent.GetCurrentHealth() <= 0;  
    public Canvas PausedGameCanvas;
    public bool IsGamePaused = false;
    

    [Header("Audio")]

    // EVENTS
    public AK.Wwise.Event PlayerDashSound;
    public AK.Wwise.Event PlayerDeathSound;
    public AK.Wwise.Event PlayerHurtSound;
    public AK.Wwise.Event PlayerAttackOnBeatSound;
    public AK.Wwise.Event PlayerAttackMissedBeatSound;
    public AK.Wwise.Event PlayerFootstepSound;

    public float FootstepDistance = 0.5f;

    [Header("Attack")]
    public int SuccessfulBeatClicksInSuccession = 0;
    public int SpecialEvery = 4;

    //public event Action OnAttack; // Event triggered on left mouse button press
    private void OnDestroy()
    {
        if(IsKilledWendigo == false)
        {
            //Game is not paused. Assume played died
            if(IsGamePaused == false)
                SceneManager.LoadScene(2);

        }
    }
    public void DeathBegin(GameObject obj)
    {
        PlayerDeathSound.Post(this.gameObject);
        
        Animator.SetTrigger("Death");
        //this.gameObject.SetActive(false);

    }
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        HealthComponent = GetComponent<Health>();
        HealthComponent.OnDeath += DeathBegin;
        IsGamePaused = false;

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void OnPause(InputAction.CallbackContext context)
    {
        if (Time.timeScale == 0)
        {
            if (PausedGameCanvas)
                PausedGameCanvas.gameObject.SetActive(false);
            Time.timeScale = 1;
            IsGamePaused = false;

            BeatmapTrack.Beatmap.Conductor.Resume();
        }
        else
        {
            if (PausedGameCanvas)
                PausedGameCanvas.gameObject.SetActive(true);
            Time.timeScale = 0;
            IsGamePaused = true;
            BeatmapTrack.Beatmap.Conductor.Pause();

        }
    }
    public void OnResumeButtonClicked()
    {
            if (PausedGameCanvas)
                PausedGameCanvas.gameObject.SetActive(false);
            Time.timeScale = 1;
            IsGamePaused = false;
            BeatmapTrack.Beatmap.Conductor.Resume();
    }


    public bool IsInvincible = false;
    public void OnTriggerEnter2D(Collider2D collision)
    {

        if(collision.gameObject.layer == LayerMask.NameToLayer("Enemy")
            || collision.gameObject.layer == LayerMask.NameToLayer("EnemyAttack")
            )
        {
            var  ad =  collision.gameObject.GetComponent<AttackData>();
            if(HealthComponent.TakeDamage(ad))
            {
                PlayerHurtSound.Post(this.gameObject);
                Vector3 knockbackDir = (Vector2)this.transform.position - (Vector2)collision.gameObject.transform.position;

                if (ad.ForcedKnockbackDirection != Vector2.zero)
                {
                    knockbackDir = ad.ForcedKnockbackDirection.normalized;
                }
                this.ApplyImpulse(knockbackDir * ad.Knockback, true, KnockBackDuration);
            }
        }

    }
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed) // Fires event when left mouse button is pressed
        {

            if (Time.timeScale == 0) return;
            if (!RequireInputToDash || BeatmapTrack.InputBeat())
            {

                SuccessfulBeatClicksInSuccession++;
                if (SuccessfulBeatClicksInSuccession >= SpecialEvery)
                {
                    SuccessfulBeatClicksInSuccession = 0;
                    Debug.Log("Player Special Attack");
                }
                HealthComponent.SetInvulnarability(DashInvulnarabilitySeconds);

                Animator.SetTrigger("Dash");

                PlayerDashSound.Post(this.gameObject);

                StartCoroutine(Dash());
                
                //Update mouse pointer
                Debug.Log("Successful Dash");
            }
            else
            {
                Debug.Log("Unsuccessful Dash");
                SuccessfulBeatClicksInSuccession = 0;

            }


        }

    }

    public void OnFire(InputAction.CallbackContext context)
    {
        //OnPlayerAttack.Post(this.gameObject);
        if (context.performed) // Fires event when left mouse button is pressed
        {

            if (Time.timeScale == 0) return;

            if (!RequireInputToAttack || BeatmapTrack.InputBeat())
            {
                //Shared events between special and common attack

                //Play the sound
                PlayerAttackOnBeatSound.Post(this.gameObject);

                //Update mouse pointer
                var worldPoint = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                float zAngle = OrbitalApproachingCircle.GetRotationAngle(OrbitalApproachingCircle.Origin.transform.position, worldPoint);
                var prevEulerAngles = MouseRotationObject.transform.eulerAngles;
                MouseRotationObject.transform.eulerAngles = new Vector3(prevEulerAngles.x, prevEulerAngles.y, zAngle);



                SuccessfulBeatClicksInSuccession++;
                if (SuccessfulBeatClicksInSuccession >= SpecialEvery)
                {
                    SuccessfulBeatClicksInSuccession = 0;
                    AttackAnimator.SetTrigger("SpecialAttackAnimation");
                    Animator.SetTrigger("Attack");
                    if (SpecialAttackPS)
                        SpecialAttackPS.Play();
                    Debug.Log("Player Special Attack");
                }
                else
                {
                    AttackAnimator.SetTrigger("AttackAnimation");
                    Animator.SetTrigger("Attack");
                    if (AttackPS)
                        AttackPS.Play();

                }










                Debug.Log("Successful Attack");
            }
            else
            {
                SuccessfulBeatClicksInSuccession = 0;
                PlayerAttackMissedBeatSound.Post(this.gameObject);
                Debug.Log("Missed Attack");

            }


        }
    }

    Vector2 currentVelocity;

    float traversedDistanceSinceFootstep = 0;
    int previousAnimationIndex = 0;
    private void FixedUpdate()
    {
        if (freeMovement == false) { return; }
        // Read input
        Vector2 rawInput = moveInput;
        // Normalize input and calculate desired velocity
        Vector2 targetVelocity = rawInput.normalized * maxSpeed;



        currentVelocity = Vector3.MoveTowards(currentVelocity, targetVelocity,
         (targetVelocity.magnitude > 0 ? acceleration : deceleration) * Time.fixedDeltaTime);


        Vector2 movement = currentVelocity * Time.fixedDeltaTime;

        traversedDistanceSinceFootstep += movement.magnitude;
        if (traversedDistanceSinceFootstep >= FootstepDistance)
        {
            traversedDistanceSinceFootstep -= FootstepDistance;
            if (PlayerFootstepSound != null)
                PlayerFootstepSound.Post(this.gameObject);
        }


        //rb.AddForce(movement);
        rb.MovePosition(rb.position + movement);

        Vector2 direction = currentVelocity;

        if (direction.magnitude < 0.05)
        {
            Animator.SetBool("Idle", true);

        }
        else
        {
            Animator.SetBool("Idle", false);

            float minAngle = 360 / 16;
            if (Vector2.Angle(direction, Vector2.up) < minAngle)
                Animator.SetInteger("TRDLDir", 1);

            if (Vector2.Angle(direction, new Vector2(1, 1).normalized) < minAngle)
                Animator.SetInteger("TRDLDir", 2);

            if (Vector2.Angle(direction, Vector2.right) < minAngle)
                Animator.SetInteger("TRDLDir", 3);

            if (Vector2.Angle(direction, new Vector2(1, -1)) < minAngle)
                Animator.SetInteger("TRDLDir", 4);

            if (Vector2.Angle(direction, Vector2.down) < minAngle)
                Animator.SetInteger("TRDLDir", 5);


            if (Vector2.Angle(direction, new Vector2(-1, -1)) < minAngle)
                Animator.SetInteger("TRDLDir", 6);

            if (Vector2.Angle(direction, Vector2.left) < minAngle)
                Animator.SetInteger("TRDLDir", 7);

            if (Vector2.Angle(direction, new Vector2(-1, 1)) < minAngle)
                Animator.SetInteger("TRDLDir", 8);

        }


        //Update sprite
        //More vertical input
        //        if(Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
        //        {
        //            if(direction.y > 0)
        //            {
        //                Animator.SetInteger("TRDLDir", 0);
        //
        //            }
        //            else
        //            {
        //                Animator.SetInteger("TRDLDir", 2);
        //
        //            }
        //
        //
        //        }
        //        else
        //        {
        //            if(direction.x > 0)
        //            {
        //                Animator.SetInteger("TRDLDir", 1);
        //
        //            }
        //            else
        //            {
        //                Animator.SetInteger("TRDLDir", 3);
        //
        //            }
        //
        //        }


    }
    public void ApplyImpulse(Vector2 impulse, bool lockMovement = true,float d =1)
    {
        StartCoroutine(ApplyImpulseCoroutine(impulse, lockMovement,d));

    }

    private IEnumerator ApplyImpulseCoroutine(Vector2 impulse,bool lockMovement = true, float d=1)
    {
        if(lockMovement)
            freeMovement = false;

        rb.velocity = Vector2.zero; // Reset velocity before dashing
        //rb.AddForce(impulse,ForceMode2D.Impulse); // Apply dash force
        rb.AddForce(impulse,ForceMode2D.Impulse); // Apply dash force
        yield return new WaitForSeconds(d);

        if(lockMovement)
            freeMovement = true;
    }
    private IEnumerator Dash()
    {
        freeMovement = false;
        rb.velocity = Vector2.zero; // Reset velocity before dashing

        Vector2 dashDirection = moveInput != Vector2.zero ? moveInput : Vector2.right; // Default to right if no input
        rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse); // Apply dash force

        yield return new WaitForSeconds(dashDuration);

        //rb.velocity = Vector2.zero; // Stop movement after dash ends

        yield return new WaitForSeconds(dashCooldown);
        freeMovement = true;
    }
}
