using UnityEngine;

public class PlayerStates : MonoBehaviour
{
    public enum States
    {
        Idle,
        Crawl,
        Walk,
        Run,
        Jump,
        Landed
    }

    public bool IsGrounded { get; private set; }
    private Rigidbody2D _rb;

    public CoreControl Player;
    private AirPhysics PlayerAirState;
    public static States PlayerState;

    //Player is stationary variables
    private Vector2 PlayerPosition;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        PlayerAirState = GetComponent<AirPhysics>();
        Player = GetComponent<CoreControl>();
        PlayerPosition = _rb.position;

        IsGrounded = true;

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 newPlayerPosition = _rb.position;
        if (Helpers.CompareVectors(PlayerPosition, newPlayerPosition, 0.0001f))
        {
            PlayerState = States.Idle;
        }
        else
        {
            if (Player.grounded)
            {
                IsGrounded = true;
                switch (Player.coreState)
                {
                    case CoreControl.speedState.Jog:
                        PlayerState = States.Walk;

                        break;
                    case CoreControl.speedState.Sprint:
                        PlayerState = States.Run;
                        break;
                }

                if (PlayerAirState.airState == AirPhysics.jumpState.Landed)
                {
                    IsGrounded = false;

                    PlayerState = States.Landed;
                }
            }
            else
            {
                IsGrounded = false;
                PlayerState = States.Jump;
            }
        }
        PlayerPosition = _rb.position;
    }



}

