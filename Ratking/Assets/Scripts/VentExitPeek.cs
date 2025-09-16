using Cinemachine;
using System.Collections.Generic;
using FunkyCode;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class VentExitPeek : MonoBehaviour
{

    public LayerMask VentMask;
    public LayerMask GroundMask;
    public float VentByPassValue = 0.01f;
    public float LookAheadDistance = 1.0f;
    public CoreControl Core;
    public CinemachineVirtualCamera VCamera;
    public Transform CameraTarget;

    public BoxCollider2D VentTriggerBox;

    public Vector2 RelativeVentPosition;

    private Vector2 _defaultVentPosition = new Vector2(0, 0);
    public GameObject SpotLightObj;
    public GameObject OtherFogOfWar;
    private List<Vector2> DirectionToCheckForVent;
    // Start is called before the first frame update
    void Start()
    {
        this.VentTriggerBox = this.gameObject.GetComponent<BoxCollider2D>();
        DirectionToCheckForVent = new List<Vector2>()
        {
            Vector2.down,Vector2.up,Vector2.left,Vector2.right
        };
        RelativeVentPosition = _defaultVentPosition;
        SpotLightObj.GetComponent<Light2D>().size = this.LookAheadDistance;
    }


    private Vector3 _lastDebugPoint;
    // Update is called once per frame
    void Update()
    {



        Vector3 moveVector3 = Core.move.normalized;

        if (RelativeVentPosition != _defaultVentPosition && Core.move.normalized != _defaultVentPosition)
        {

            Vector3 relPos = this.RelativeVentPosition;
            Vector3 startPos = this.transform.position + (relPos * VentByPassValue);
            var castFromVentToOpenRoom = Physics2D.Raycast(startPos, moveVector3, LookAheadDistance, GroundMask);


            if (castFromVentToOpenRoom)
            {
                float halfDist = Vector2.Distance(startPos, castFromVentToOpenRoom.point) / 2.0f;
                var middlePoint = startPos + moveVector3 * halfDist;
                PeekLookAtLocation(startPos, middlePoint);

            }
            else
            {
                float halfDist = LookAheadDistance / 2.0f;
                PeekLookAtLocation(startPos, startPos + moveVector3 * halfDist);
            }
        }
        else
        {
            VCamera.gameObject.SetActive(false);
            SpotLightObj.SetActive(false);
            OtherFogOfWar.SetActive(true);
        }

    }

    public void PeekLookAtLocation(Vector3 startPos, Vector3 point)
    {

        OtherFogOfWar.SetActive(false);

        //Debug point setup
        _lastDebugPoint = point;

        //Set camera position at center
        VCamera.gameObject.SetActive(true);
        CameraTarget.position = point;

        //Spotlight pointing at position
        SpotLightObj.SetActive(true);


        SpotLightObj.transform.position = startPos;
        SpotLightObj.transform.rotation = Helpers.GetRotationLookAtPosition2D(startPos, point, new Vector3(0, 0, 0));

    }

    public Vector2 LocateRelativeVentPosition(Collider2D col)
    {
        foreach (var direVector2 in DirectionToCheckForVent)
        {
            var hit = Physics2D.Raycast(this.transform.position, direVector2, this.VentTriggerBox.size.x, VentMask);
            if (hit)
            {
                return direVector2.normalized;
            }
        }

        return _defaultVentPosition;
    }

    //public void DebugRaycastForVector()
    //{
    //    foreach (var direVector2 in DirectionToCheckForVent)
    //    {
    //        Vector2 startPoint = this.transform.position;
    //        Vector2 endPoint = startPoint + (direVector2 * this.VentTriggerBox.size.x);
    //        Gizmos.DrawLine(startPoint, endPoint);
    //    }

    //}


    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Vent"))
        {

            RelativeVentPosition = LocateRelativeVentPosition(col);
            if (RelativeVentPosition != _defaultVentPosition)
            {
                Debug.Log("Found Vent");
            }
        }

    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Vent"))
        {
            RelativeVentPosition = _defaultVentPosition;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (RelativeVentPosition != _defaultVentPosition)
        {
            Vector3 relPos = this.RelativeVentPosition;
            Gizmos.DrawWireSphere(this.transform.position + (relPos * this.VentTriggerBox.size.x / 2.0f), 0.1f);
            Gizmos.DrawLine(this.transform.position + (relPos * VentByPassValue), this.transform.position + (relPos * (VentByPassValue + LookAheadDistance)));
            Gizmos.DrawWireSphere(_lastDebugPoint, 0.2f);
        }
       // DebugRaycastForVector();

    }


}
