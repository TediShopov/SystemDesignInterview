using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(BoxCollider2D))]
public class Door : MonoBehaviour
{
    private BoxCollider2D Collider;
    public DoorLock[] DoorLocks;
    DoorLock[] startLocks;

    public GameObject closedDoors;
    public GameObject openDoors;
    void Start()
    {
        Collider = this.gameObject.GetComponent<BoxCollider2D>();
        DoorLocks = this.transform.GetComponentsInChildren<DoorLock>();
        startLocks = DoorLocks;
        setDoorsClosed();

    }

    private void OnEnable()
    {

       ExtractionMethods.onUpdateZone += close;

    }
    private void OnDisable()
    {

        ExtractionMethods.onUpdateZone -= close;

    }

    public void TryUnlock()
    {
        foreach (var doorLock in DoorLocks)
        {
            doorLock.TryUnlock();
        }

        DoorLocks = this.DoorLocks.Where(x => x.IsUnlocked == false).ToArray();

        if (DoorLocks.Length == 0)
        {
            OnAllLocksRemoved();
        }
    }

    public bool IsUnlocked => DoorLocks.Length == 0;

    public bool CanUnlock()
    {
        foreach (var doorLock in DoorLocks)
        {
            if (!doorLock.CanUnlock())
            {
                return false;
            }
            
        }
        return true;
    }

    void open()
    {
        openDoors.SetActive(true);
        closedDoors.SetActive(false);
        Collider.enabled = false;
        
    }

    void setDoorsClosed()
    {
        openDoors.SetActive(false);
        closedDoors.SetActive(true);
        Collider.enabled = true;
    }

    void close(object sender, EventArgs e)
    {
        setDoorsClosed();
        DoorLocks = startLocks;

        foreach (DoorLock go in DoorLocks)
        {
            go.OnReset();
        }
    }
    
   

    void OnAllLocksRemoved()
    {
        //Destroy(this.gameObject, 1.0f);
        open();
    }

}
