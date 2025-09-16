using Cinemachine;
using UnityEngine;

public class FitCameraToCollider : MonoBehaviour
{
    private float _defaultCameraSize;
    public CinemachineVirtualCamera Camera;
    public Collider2D collider;
    // Start is called before the first frame update
    void Start()
    {
        this._defaultCameraSize = Camera.m_Lens.OrthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Camera.m_Lens.OrthographicSize = collider.bounds.size.x / 2.0f;
        }
        else if (Input.GetKeyUp(KeyCode.H))
        {
            Camera.m_Lens.OrthographicSize = _defaultCameraSize;
        }
    }
}
