using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Properties))]
[RequireComponent(typeof(CoreControl))]

public class PlayerSoundGeneration : MonoBehaviour
{

    private CoreControl _core;
    private Properties _properties;

    private float _distanceTravelled = 0.0f;
    public float DistanceToSound = 2.0f;


    public float force = 2f;
    public float modifierTwo = 2f;


    private readonly float minDistance = 0.001f;
    private Vector2 _previousPosition;
    public Transform soundPoint;

    private Vector2 _currentPosition;
    // Start is called before the first frame update
    void Start()
    {
        _previousPosition = _currentPosition;
        this._core = this.gameObject.GetComponent<CoreControl>();
        this._properties = this.gameObject.GetComponent<Properties>();
    }
    private bool SprintHeld=false;
    public void refreshDistanceTillSound(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            Debug.Log("SprintHeld");
            SprintHeld = true;
        }

        if (context.canceled)
        {
            Debug.Log("NOT SprintHeld");
            SprintHeld = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

        _currentPosition = transform.position;

        float distance = Mathf.Abs(_currentPosition.x - _previousPosition.x);


        _distanceTravelled += distance;

        if (!SprintHeld)
        {
            _distanceTravelled = 0;
        }

        if (distance <= minDistance)
        {
            _distanceTravelled = DistanceToSound;
            return;
        }

        if (!_core.grounded)
        {
            _distanceTravelled = 0;

        }
        if (_distanceTravelled >= DistanceToSound)
        {
            GenerateSound();
            _distanceTravelled = 0;
        }
        _previousPosition = _currentPosition;

        Vector2 endPosition = _previousPosition;
        endPosition.x = _previousPosition.x + distance;



    }

    public float LastCollidedSoundProperty = 0;
    void GenerateSound()
    {
        SoundGenerator.Instance.SpawnSound(force, soundPoint.position, this._properties.SoundModifier, modifierTwo, this.gameObject);
    }


    public bool HasCollided = false;
    public float AngleOfHitAllowed = 30;

     bool IsHittingGround(Collision2D col)
    {
        var relPositionOfHit = col.GetContact(0).point - this._currentPosition;
        if (Vector3.Angle(relPositionOfHit.normalized, Vector3.down) < AngleOfHitAllowed)
        {
            return true;
        }
        return false;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        //SoundGenerator.GetSoundModifier(col.otherCollider.gameObject);
        //if (!HasCollided)
        //{
        //    SoundGenerator.Instance.SpawnSound(col);
        //    HasCollided = true;
        //}
        //if (!HasCollided && IsHittingGround(col))
        //{
        //    SoundGenerator.Instance.SpawnSound(col);
        //}

        if (col.contacts.Length<=2 && IsHittingGround(col) )
        {
            HasCollided = true;
            //LastCollidedSoundProperty = SoundGenerator.GetSoundModifier(collision.otherCollider.gameObject);
            SoundGenerator.Instance.SpawnSound(col);
        }
    }

    //void OnCollisionStay2D(Collision2D collision)
    //{
    //    if (!HasCollided && IsHittingGround(collision))
    //    {
    //        HasCollided = true;
    //        //LastCollidedSoundProperty = SoundGenerator.GetSoundModifier(collision.otherCollider.gameObject);
    //        SoundGenerator.Instance.SpawnSound(collision);
    //    }
    //}

    void OnCollisionExit2D(Collision2D col)
    {
        HasCollided = false;
    }



}
