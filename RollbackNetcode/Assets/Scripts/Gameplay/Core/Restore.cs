using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

public class Restore : MonoBehaviour
{
    public GameObject ProjectilePrefab;
    public GameObject FighterPrefab;
    public GameObject GameContainer;
    public GameObject RBState;
    public InputFrame RollbackInput { get; set; }
    public FighterController EnemyFighterController { get; set; }

    //Debug properties for testing the RollBack
    public bool AutoRollbackEveryFrame = false;
    public bool PrintInputBeforeAndAfter = true;

    public int PauseInterval = 5;
    public int MaxAllowedRollbackFrames;
    public int FramesToWaitForNextRollback = 5;
    public int RollbackAllowed { get; private set; }

    private int _passedPausedFrames = 0;

    public void Awake()
    {
        Physics2D.simulationMode = SimulationMode2D.Script;
    }


    private void Start()
    {
        var playerController = StaticBuffers.Instance.Player.GetComponent<FighterController>();
        RollbackAllowed = 7;
        EnemyFighterController = StaticBuffers.Instance.Enemy.GetComponent<FighterController>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (NetworkGamePacket.RollbackFrames < 0 || Input.GetKey(KeyCode.B) || AutoRollbackEveryFrame)
        {
            //Rollback is po
            bool rollbackIsPossible = Math.Abs(NetworkGamePacket.RollbackFrames) <= MaxAllowedRollbackFrames;
            Rollback(MaxAllowedRollbackFrames);
            NetworkGamePacket.RollbackFrames = 0;
        }
        //Pause input
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (ClientData.GameState == GameState.Paused)
                ClientData.GameState = GameState.Runing;
            else if (ClientData.GameState == GameState.Runing)
                ClientData.GameState = GameState.Paused;
        }
    }

    public void Rollback(int frames)
    {
        lock (NetworkGamePacket.receiveLock)
        {

            if (PrintInputBeforeAndAfter)
                StaticBuffers.Instance.PlayerBuffer.GetInputBufferString();

            //Replaces the acutal fighter entities with their copies from the
            //rollback layer
            ReplaceFighterObjectWith(ref StaticBuffers.Instance.Player, StaticBuffers.Instance.PlayerRB);
            ReplaceFighterObjectWith(ref StaticBuffers.Instance.Enemy, StaticBuffers.Instance.EnemyRB);

            //Reassign all static buffers and variables to point to the new player and enemy
            StaticBuffers.Instance.RenewBuffers();

            EnemyFighterController = StaticBuffers.Instance.Enemy.GetComponent<FighterController>();

            //Shorthands from input buffers
            InputBuffer player = StaticBuffers.Instance.Player.GetComponent<FighterController>().InputBuffer;
            InputBuffer playerRB = StaticBuffers.Instance.PlayerRB.GetComponent<FighterController>().InputBuffer;
            InputBuffer enemy = StaticBuffers.Instance.Enemy.GetComponent<FighterController>().InputBuffer;


            //The concatenated buffers to rollback with.
            //Exepected 10 input frames (3 from static delay, 7 from rollback)
            player = BufferToRollbackWith(playerRB, player);
            InputBuffer enemyRollback = BufferToRollbackWith(StaticBuffers.Instance.EnemyRBBuffer, enemy);

            if (PrintInputBeforeAndAfter)
            {
                Debug.Log($"BEFORE RESTORE: \n");
                DebugPringAllBuffersState(player, enemyRollback);
            }

            SetSimulationState(RBState.GetComponent<StateProjectileManager>(), false);
            ResimulateProjectiles(GameContainer.GetComponent<StateProjectileManager>(),
                RBState.GetComponent<StateProjectileManager>());

            //Resimulate the first 7 frames of the concatenated buffers
            for (int i = 0; i < frames; i++)
            {
                StaticBuffers.Instance.Player.GetComponent<FighterController>().Resimulate(player, 1);
                player.OnUpdate();
                StaticBuffers.Instance.Player.GetComponent<AnimatorUpdater>().ManualUpdateFrame();

                StaticBuffers.Instance.Enemy.GetComponent<FighterController>().Resimulate(enemyRollback, 1);
                enemyRollback.OnUpdate();
                StaticBuffers.Instance.Enemy.GetComponent<AnimatorUpdater>().ManualUpdateFrame();

                Physics2D.Simulate(Time.fixedDeltaTime);
            }
            SetSimulationState(RBState.GetComponent<StateProjectileManager>(), true);
            //Should be left with 3 input frames as on a regular frame


            if (PrintInputBeforeAndAfter)
            {
                Debug.Log($"AFTER RESTORE: \n");
                DebugPringAllBuffersState(player, enemyRollback);
            }
        }
    }

    private void ReplaceFighterObjectWith(ref GameObject toReplace, GameObject from)
    {
        Transform RBTransform = from.GetComponent<Transform>();

        GameObject newObject;
        newObject = Instantiate(FighterPrefab, GameContainer.transform);

        //Debug.LogError("Instantiated  NEW Player Fighter");
        try
        {
            //Rollback the positiion of the actual player to RB dummy
            newObject.transform.position = new Vector3(RBTransform.position.x, RBTransform.position.y, 0);
            newObject.transform.rotation = RBTransform.rotation;
            newObject.transform.parent = RBTransform.parent;

            newObject.transform.localScale = RBTransform.localScale;

            //Changed RestoRE !!!!
            newObject.GetComponent<FighterController>().
                SetInnerStateTo(from.GetComponent<FighterRBControlller>());

            newObject.GetComponent<FighterBufferMono>()
                .InputBuffer.SetTo(toReplace.GetComponent<FighterBufferMono>().InputBuffer);

            Animator newObjectAnimator = newObject.GetComponent<Animator>();
            Animator fromAnimator = from.GetComponent<Animator>();

            bool crouchFrom = fromAnimator.GetBool("Crouch");
            bool crouchNew = newObjectAnimator.GetBool("Crouch");
            //TODO add blocking state to restore
            ReplaceAnimationClip(newObjectAnimator, fromAnimator);
            ReplaceAnimatorParameters(newObjectAnimator, fromAnimator);

            HealthScript healthScript = newObject.GetComponent<HealthScript>();
            healthScript.SetValues(from.GetComponent<HealthScript>());

            AttackScript attackScript = newObject.GetComponent<AttackScript>();
            if (from.GetComponent<AttackScript>().IsHurtBoxActivated == true)
            {
                int a = 3;
            }
            attackScript.SetTo(from.GetComponent<AttackScript>());

            bool isEnemy = newObject.GetComponent<FighterController>().State.isEnemy;

            if (!isEnemy)
            {
                newObject.GetComponent<FighterBufferMono>().CollectInputFromKeyboard = true;
            }
            else
            {
                newObject.GetComponent<SpriteRenderer>().color = Color.red;
            }
            Destroy(toReplace);
            toReplace.SetActive(false);
            toReplace = newObject;
        }
        catch (System.Exception)
        {
            Destroy(toReplace);

            throw;
        }
    }
    private void ReplaceAnimationClip(Animator toReplace, Animator from)
    {
        var animState = from.GetCurrentAnimatorStateInfo(0);

        toReplace.Play(animState.fullPathHash, 0, animState.normalizedTime);
    }

    private void ReplaceAnimatorParameters(Animator toReplace, Animator from)
    {
        foreach (var param in from.parameters)
        {
            switch (param.type)
            {
                case AnimatorControllerParameterType.Float:
                    toReplace.SetFloat(param.name, from.GetFloat(param.name));

                    break;

                case AnimatorControllerParameterType.Int:
                    toReplace.SetInteger(param.name, from.GetInteger(param.name));
                    break;

                case AnimatorControllerParameterType.Bool:
                    toReplace.SetBool(param.name, from.GetBool(param.name));
                    break;

                case AnimatorControllerParameterType.Trigger:
                    //    toReplace.SetTrigger(param.name);
                    break;

                default:
                    break;
            }
        }
    }

    private void SetSimulationState(StateProjectileManager RBProjectileManager, bool state)
    {
        foreach (var proj in RBProjectileManager.ProjectilesInState)
        {
            proj.GetComponent<Rigidbody2D>().simulated = state;
        }
    }
    private void ResimulateProjectiles(StateProjectileManager gameState,
        StateProjectileManager RBProjectileManager)
    {
        foreach (var proj in gameState.ProjectilesInState)
        {
            DestroyImmediate(proj);
        }

        foreach (var proj in RBProjectileManager.ProjectilesInState)
        {
            GameObject projectileCopy = Instantiate(ProjectilePrefab, proj.transform.position,
                proj.transform.rotation);
            projectileCopy.GetComponent<Projectile>().AddToManager(gameState.gameObject);
            projectileCopy.GetComponent<Rigidbody2D>().velocity =
                proj.GetComponent<Rigidbody2D>().velocity;
        }
    }


    //Concatenate the fighter and rollback fighter buffers
    private InputBuffer BufferToRollbackWith(InputBuffer RBBuffer, InputBuffer normalBuffer)
    {
        InputBuffer rollbackBuffer = new InputBuffer();
        rollbackBuffer.SetTo(RBBuffer);

        foreach (var inputFrame in normalBuffer.BufferedInput)
        {
            rollbackBuffer.BufferedInput.Enqueue(inputFrame);
        }
        return rollbackBuffer;
    }

    private static void DebugPringAllBuffersState(InputBuffer player, InputBuffer bufferToRollbackWith)
    {
        Debug.Log("PlayerBuffer\n");
        Debug.Log(StaticBuffers.Instance.PlayerBuffer.GetInputBufferString());
        Debug.Log("EnemyBuffer\n");
        Debug.Log(StaticBuffers.Instance.EnemyBuffer.GetInputBufferString());
        Debug.Log("EnemyRBBuffer\n");
        Debug.Log(StaticBuffers.Instance.EnemyRBBuffer.GetInputBufferString());
        Debug.Log("ROLLBACK PLAYER\n");
        Debug.Log(player.GetInputBufferString());
        Debug.Log("ROLLBACK ENEMY\n");
        Debug.Log(bufferToRollbackWith.GetInputBufferString());
    }

    private static bool HasUniquesStamps(InputBuffer buff)
    {
        Queue<InputFrame> enemyBufferdInput = new InputBuffer(buff).BufferedInput;
        HashSet<int> occuringFrames = new HashSet<int>();

        for (int i = 0; i < enemyBufferdInput.Count; i++)
        {
            int currentFrame = enemyBufferdInput.Dequeue().FrameStamp;
            if (occuringFrames.Contains(currentFrame) == false)
                occuringFrames.Add(currentFrame);
            else
                return false;
        }
        return true;
    }
}