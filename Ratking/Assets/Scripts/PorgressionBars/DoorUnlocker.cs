using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CircleCollider2D))]
public class DoorUnlocker : MonoBehaviour, IInteractAction
{

    private CircleCollider2D InteractableRange;

    private Door DoorInRange; 
    [SerializeField] public GameObject PromptOnActive;

    // Start is called before the first frame update
    void Start()
    {
        InteractableRange = this.gameObject.GetComponent<CircleCollider2D>();
        this.Priority = 50;
        LevelData.InteractActionManager.RegisterAction(this);
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Door"))
        {
            var doorComponent = other.gameObject.GetComponent<Door>();
            DoorInRange = doorComponent;
            if (OnChangedAvailability!=null)
                OnChangedAvailability.Invoke();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Door"))
        {
            DoorInRange = null;
            if (OnChangedAvailability != null)
                OnChangedAvailability.Invoke();
        }
    }


    public int Priority { get; private set; }
    public bool CanPerform()
    {
        //Player can always try to unlock the door.
        return DoorInRange != null && !DoorInRange.IsUnlocked /*&& DoorInRange.CanUnlock()*/ ;
    }

    public void OnActivated()
    {
        Vector3 downPoint = this.DoorInRange.GetComponent<Collider2D>().ClosestPoint(Vector2.down * 1000);
        LevelData.ButtonPromptManager.SpawnOrUpdate(KeyCode.E, downPoint, PromptOnActive);
    }

    public void OnDeactivated()
    {
        LevelData.ButtonPromptManager.Remove(KeyCode.E);
    }

    public void ActiveUpdate()
    {
        return;
    }

    public void PlacePrompt()
    {
        Vector2 downPoint = this.DoorInRange.GetComponent<Collider2D>().ClosestPoint(Vector2.down * 1000);
        LevelData.ButtonPromptManager.SpawnOrUpdate(KeyCode.E, downPoint, PromptOnActive);
    }

    public void DoAction()
    {
        if (this.DoorInRange != null)
        {
            this.DoorInRange.TryUnlock();
        }
        if (OnChangedAvailability != null)
            OnChangedAvailability.Invoke();

    }

    public event IInteractAction.ChangedAvailability OnChangedAvailability;

    public int CompareTo(IInteractAction other)
    {
        return Priority.CompareTo(other.Priority);
    }
}
