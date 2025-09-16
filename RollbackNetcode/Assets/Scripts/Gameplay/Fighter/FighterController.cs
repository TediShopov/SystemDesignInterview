using FixMath.NET;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

//Data structure holding the inital parameters for performing fixed-point
//physics on the character
[System.Serializable]
public struct FighterPhysicsConfig
{
    //This are serializable field to make input intuitive
    [SerializeField] public float moveSpeed;

    [SerializeField] public float jumpPower;
    [SerializeField] public float gravity;
    public Fix64 ground;
    public FighterPhysicsConfig(float ms)
    {
        moveSpeed = 5;
        jumpPower = 10;
        gravity = 10;
        ground = (Fix64)(0.0);
    }
    public FighterPhysicsConfig(FighterPhysicsConfig Other)
    {
        this.moveSpeed = Other.moveSpeed;
        this.ground = Other.ground;
        this.jumpPower = Other.jumpPower;
        this.gravity = Other.gravity;
    }
}

[System.Serializable]
public struct FighterState
{
    public bool dying;
    public bool castingFireball;
    public bool isGrounded;
    public bool isCrouched;
    public bool isBlocking;
    public bool isEnemy;
    public bool isFlipped;

    public FixedVector2 Position;
    public Fix64 verticalVelocity;  // Velocity along the y-axis
    public FighterState(FixedVector2 startingPos, bool isE)
    {
        this.Position = startingPos;
        this.isEnemy = isE;
        this.isFlipped = false;
        dying = false;
        castingFireball = false;
        isGrounded = true;
        isCrouched = false;
        isBlocking = false;
        verticalVelocity = Fix64.Zero;  // Velocity along the y-axis
    }
    public FighterState(FighterState Other)
    {
        this.isCrouched = Other.isCrouched;
        this.isEnemy = Other.isEnemy;
        this.isGrounded = Other.isGrounded;
        this.dying = Other.dying;
        this.isFlipped = Other.isFlipped;
        this.castingFireball = Other.castingFireball;
        this.Position = Other.Position;
        this.verticalVelocity = Other.verticalVelocity;
        this.isBlocking = false;
    }
}

public class FighterController : MonoBehaviour
{
    //public FighterController enemy;

    [Header("STATE")]
    public FighterPhysicsConfig PhysicsConfig;

    public FighterState State;

    [Header("PROJECTILE RELATED")]
    public Projectile projectilePrefab;

    public Transform projectileFirePoint;

    public float waitAfterJump;
    public bool DebugInputBufferBeforeProcessing;
    public bool IsPredictionAllowed = true;
    public int TimePredicted = 0;

    public int ExecuteAtRelativeFrame = 0;

    public InputBuffer InputBuffer { get; set; }
    private int LastFrameProcessed;

    public bool ExecuteOnlyOnOverflow;
    private Animator animator;
    private AttackScript attack;
    private SpriteRenderer spriteRenderer;
    // Start is called before the first frame update

    public TMPro.TextMeshPro gameOverText;

    private void Awake()
    {
        LastFrameProcessed = 0;

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        attack = GetComponent<AttackScript>();
    }

    // Update is called once per frame
    public virtual void FixedUpdate()
    {
        lock (NetworkGamePacket.receiveLock)
        {
            if (ClientData.GameState != GameState.Runing || this.State.dying)
            {
                TransferInnerStateToAnimator(0);
                return;
            }

            ApplyGravity();

            //Teleport some space on the left (This is a TESTING functionlity)
            //This is used to test if game syncs are actually detected on the client
            if (Input.GetKey(KeyCode.L))
            {
                State.Position.x += (Fix64)(0.5);
            }
            //Teleport some space on the left (This is a TESTING functionlity)
            //This is used to test if game syncs are actually detected on the client
            if (Input.GetKey(KeyCode.Escape))
            {
                SceneManager.LoadScene(0);
            }

            OrientToEnemy(GetEnemy()?.transform);

            //if (FrameLimiter.Instance.FramesInPlay >= InputBuffer.DelayInput)
            // if (InputBuffer is null || InputBuffer.BufferedInput.Count <= 0)
            //     Debug.Log("Input Buffer Is Empty Or Uninitialized");

            if (IsPredictionAllowed && InputBuffer.BufferedInput.Count <= 0)
            {
                TimePredicted++;
                InputBuffer.Enqueue(GetPredictedInput(InputBuffer.LastFrame));
            }
            if (InputBuffer is null)
                Debug.LogError("Input Buffer is null");
            if (InputBuffer.Peek() is null)
                Debug.LogError("Input FRAME is null");
            //if (InputBuffer.Peek().FrameStamp >= 0 && FrameLimiter.Instance.FramesInPlay >= InputBuffer.Peek().FrameStamp)
            if (InputBuffer.Peek().FrameStamp >= 0 && FrameLimiter.Instance.FramesInPlay + ExecuteAtRelativeFrame == InputBuffer.Peek().FrameStamp)
            {
                ProcessInputBuffer(InputBuffer);
            }
            attack.OnUpdate();
            UpdatePosition();
        }
    }
    public void SetInnerStateTo(FighterController fc)
    {
        this.State = new FighterState(fc.State);
        this.PhysicsConfig = new FighterPhysicsConfig(fc.PhysicsConfig);
        this.IsPredictionAllowed = fc.IsPredictionAllowed;
    }
    private void OnDestroy()
    {
        if (ClientData.GameState == GameState.Finished)
        {
            if (this.State.isEnemy)
            {
                ClientData.GameOverMessage = "Game Over: You Win";
            }
            else
            {
                ClientData.GameOverMessage = "Game Over: You Lose";
            }
        }
    }

    public GameObject GetEnemy()
    {
        //If in rollback testing layer
        if (this.gameObject.layer == 9)
        {
            if (!State.isEnemy)
            {
                return StaticBuffers.Instance.EnemyRB;
            }
            else
            {
                return StaticBuffers.Instance.PlayerRB;
            }
        }
        else
        {
            if (!State.isEnemy)
            {
                return StaticBuffers.Instance.Enemy;
            }
            else
            {
                return StaticBuffers.Instance.Player;
            }
        }
    }

    private void OrientToEnemy(Transform enemyTransform)
    {
        bool isRight = (enemyTransform.position.x - this.transform.position.x) < 0;

        if (isRight)
        {
            State.isFlipped = true;
            if (this.transform.localScale.x > 0)
            {
                Vector3 theScale = transform.localScale;
                theScale.x *= -1;
                this.transform.localScale = theScale;
            }
            // spriteRenderer.flipX = true;
        }
        else
        {
            State.isFlipped = false;
            if (this.transform.localScale.x < 0)
            {
                Vector3 theScale = transform.localScale;
                theScale.x *= -1;
                this.transform.localScale = theScale;
            }
            //  spriteRenderer.flipX = false;
        }
    }

    public Vector3 GetDirToEnemy()
    {
        return Vector3.Normalize(GetEnemy().transform.position - this.transform.position);
    }
    private InputFrame GetPredictedInput(InputFrame lastReceived)
    {
        //Debug.LogError($"Predicted input from {lastReceived.FrameStamp}");
        if (lastReceived == null)
        {
            var emptyInputPred = new InputFrame();
            emptyInputPred.FrameStamp = FrameLimiter.Instance.FramesInPlay;
            emptyInputPred.IsPredicted = true;
            return emptyInputPred;
        }

        var inputPredicted = new InputFrame(lastReceived.Inputs,
                  FrameLimiter.Instance.FramesInPlay);
        inputPredicted.IsPredicted = true;
        return inputPredicted;
    }

    //    -----INPUT PROCESSING-----

    #region InputProcessing

    private void ProcessInputBuffer(InputBuffer inputBuffer)
    {
        if (inputBuffer == null)
            return;

        if (DebugInputBufferBeforeProcessing)
            Debug.Log($"Current Frame: {FrameLimiter.Instance.FramesInPlay}. Buffer {inputBuffer.GetInputBufferString()}");

        if (inputBuffer.PressedKeys != null && inputBuffer.PressedKeys.Count != 0)
        {
            if (CheckFireball(inputBuffer.PressedKeys.ToArray()))
            {
                Debug.LogError("Fireball Input Detected");
                InputBuffer.PressedKeys.Clear();
                AttempCastFireball();
            }
        }
        //If combos/special attack is not found
        //Mode for the RB to simulate delay
        if (ExecuteOnlyOnOverflow)
        {
            if (inputBuffer.IsOverflow)
            {
                InputFrame input = inputBuffer.Dequeue();
                ProcessInputs(input);
                LastFrameProcessed = input.FrameStamp;
            }
        }
        else
        {
            InputFrame input;
            input = inputBuffer.Dequeue();
            // if (input.FrameStamp != FrameLimiter.Instance.FramesInPlay)
            //     Debug.Log($"Time stamp is not matching the frame {input.FrameStamp} {FrameLimiter.Instance.FramesInPlay}");
            ProcessInputs(input);
            LastFrameProcessed = input.FrameStamp;
        }
    }

    private bool CheckFireball(KeyCode[] inputElements)
    {
        KeyCode[] consecKey = new KeyCode[] { KeyCode.A, KeyCode.S, KeyCode.D };
        if (State.isFlipped)
        {
            consecKey = new KeyCode[] { KeyCode.D, KeyCode.S, KeyCode.A };
        }

        bool fireballDone = true;

        if (inputElements.Length < 3)
        {
            return false;
        }

        //EX Press keys Buffer D D A D A
        // [0-2]

        for (int index = 0; index <= inputElements.Length - 3; index++)
        {
            fireballDone = true;
            for (int i = 0; i <= 2; i++)
            {
                if (inputElements[index + i] != consecKey[i])
                {
                    fireballDone = false;
                    break;
                }
            }
            if (fireballDone)
            {
                return true;
            }
        }

        return false;
    }

    private void ProcessInputs(InputFrame inputs)
    {
        if (State.castingFireball)
        {
            return;
        }
        Vector2 horizontalMovement = new Vector2(0, 0);

        if (inputs.IsKey(KeyCode.K) && State.isGrounded)
        {
            State.isBlocking = true;
            TransferInnerStateToAnimator(0);
            return;
        }
        State.isBlocking = false;

        if (State.isGrounded && State.isBlocking == false)
        {
            attack.ProcessInput(inputs);
            if (attack.IsPerformingAttack())
            {
                TransferInnerStateToAnimator(horizontalMovement.x);
                return;
            }
        }

        //        if (attack.IsHurtBoxActivated) {
        //            return;
        //        }

        if (State.isGrounded)
        {
            SetCrouch(inputs.IsKey(KeyCode.S));
            if (inputs.IsKey(KeyCode.Space))
            {
                Jump();
            }
        }

        if (!State.isCrouched)
        {
            if (inputs.IsKey(KeyCode.D))
            {
                horizontalMovement.x = 1;
            }
            if (inputs.IsKey(KeyCode.A))
            {
                horizontalMovement.x = -1;
            }
        }

        State.Position.x += (Fix64)horizontalMovement.x * (Fix64)PhysicsConfig.moveSpeed * (Fix64)0.01f;

        if (State.isFlipped)
        {
            horizontalMovement.x = -horizontalMovement.x;
        }

        TransferInnerStateToAnimator(horizontalMovement.x);
    }

    public void ResimulateInput(InputBuffer inputBuffer)
    {
        if (inputBuffer == null)
            return;

        if (DebugInputBufferBeforeProcessing)
            Debug.Log($"Current Frame: {FrameLimiter.Instance.FramesInPlay}. Buffer {inputBuffer.GetInputBufferString()}");

        if (inputBuffer.PressedKeys != null && inputBuffer.PressedKeys.Count != 0)
        {
            if (CheckFireball(inputBuffer.PressedKeys.ToArray()))
            {
                Debug.LogError("Fireball Input Detected");
                InputBuffer.PressedKeys.Clear();
                AttempCastFireball();
            }
        }
        InputFrame input;
        input = inputBuffer.Dequeue();
        ProcessInputs(input);
    }

    public void Resimulate(InputBuffer buffer, int frames)
    {
        for (int i = 0; i < frames; i++)
        {
            ApplyGravity();
            OrientToEnemy(GetEnemy()?.transform);

            if (buffer.BufferedInput.Count > 0)
            {
                ResimulateInput(buffer);
            }
            attack.OnUpdate();
            UpdatePosition();
        }
    }

    #endregion InputProcessing

    //    -----GRAVITY AND PHYSICS-----

    #region GravityAndPhysicsRelated

    private void ApplyGravity()
    {
        // Apply gravity only if the entity is in the air
        if (!State.isGrounded)
        {
            State.verticalVelocity += (Fix64)PhysicsConfig.gravity * (Fix64)(Time.fixedDeltaTime);  // Gravity scaled by FixedUpdate timestep
            State.Position.y += State.verticalVelocity * (Fix64)(Time.fixedDeltaTime);  // Update position with velocity

            // Check if the entity has reached or fallen below ground level
            if (State.Position.y <= PhysicsConfig.ground)
            {
                State.Position.y = PhysicsConfig.ground;
                State.verticalVelocity = Fix64.Zero;  // Reset velocity upon landing
                State.isGrounded = true;  // Mark entity as grounded
                animator.SetBool("IsGrounded", State.isGrounded);
                animator.SetBool("Jumped", false);
            }
        }
    }
    private void Jump()
    {
        State.verticalVelocity = (Fix64)PhysicsConfig.jumpPower;  // Apply jump force
        State.isGrounded = false;
        animator.SetBool("Jumped", true);
    }

    #endregion GravityAndPhysicsRelated

    //    -----TRANSFERING TO RENDER STATE-----

    #region TransferingGameSateToRenderState

    private void UpdatePosition()
    {
        // Convert fixed-point values back to float for Unity's transform
        transform.position = new Vector3((float)State.Position.x, (float)State.Position.y, 0);
    }
    private void TransferInnerStateToAnimator(float horizontalInput)
    {
        //State
        animator.SetBool("IsGrounded", State.isGrounded);
        animator.SetBool("CastingFireball", State.castingFireball);
        animator.SetBool("IsBlocking", State.isBlocking);
        animator.SetBool("Crouch", State.isCrouched);

        animator.SetFloat("XSpeed", horizontalInput);
        animator.SetBool("Dying", State.dying);
        if (State.dying)
        {
            animator.SetBool("IsDead", true);
            animator.SetTrigger("Dying");
        }
    }

    #endregion TransferingGameSateToRenderState

    //Triggers to affect the animation state
    public void AttempCastFireball()
    {
        animator.SetTrigger("CastFireball");
    }
    public void SetCastingFireball(bool b)
    {
        State.castingFireball = b;
        animator.SetBool("CastingFireball", b);
        if (State.isCrouched)
        {
            SetCrouch(false);
        }
    }

    public void SetDamaged(bool isLow)
    {
        if (isLow)
        {
            animator.SetTrigger("LowDamage");
        }
        else
        {
            animator.SetTrigger("HighDamage");
        }
    }
    public void SetDying(bool b)
    {
        this.State.dying = b;
    }
    public void SetCrouch(bool b)
    {
        State.isCrouched = b;
    }
}