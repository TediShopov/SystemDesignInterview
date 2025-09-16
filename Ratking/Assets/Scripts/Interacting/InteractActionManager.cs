using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractActionManager : MonoBehaviour
{
    private List<IInteractAction> Actions=new List<IInteractAction>();
    private IInteractAction _bestAction = null;

     void DoBestAction()
    {
        if (_bestAction!=null)
        {
            _bestAction.DoAction();
        }

        UpdateBestAction();
    }

    public void DoBestActionOnInput(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            DoBestAction();
        }

        UpdateBestAction();
    }

    void Start()
    {
        
    }

    void Update()
    {
        if (_bestAction!=null)
        {
            _bestAction.ActiveUpdate();
        }
    }

    void UpdateBestAction()
    {
       if (Actions.Count == 0)
        {
            return;
        }
        Actions.Sort();
        var previousBest= _bestAction;
        _bestAction = this.Actions.FirstOrDefault(x => x.CanPerform());
        if (_bestAction == null)
        {
            if (previousBest!=null)
            {
                previousBest.OnDeactivated();
            }
        }

        if (_bestAction!=null && previousBest != _bestAction)
        {
            if (previousBest!=null)
            {
                previousBest.OnDeactivated();
            }
            _bestAction.OnActivated();
        }
    }


    public void RegisterAction(IInteractAction action)
    {
        this.Actions.Add(action);
        action.OnChangedAvailability += UpdateBestAction;
        UpdateBestAction();
    }

    public void UnRegisterAction(IInteractAction action)
    {
        this.Actions.Remove(action);
        action.OnChangedAvailability -= UpdateBestAction;
        UpdateBestAction();
    }
}
