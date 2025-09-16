using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface  IInteractAction : IComparable<IInteractAction>
{
    public int Priority { get; }
    public  bool CanPerform();
    public  void OnActivated();
    public void OnDeactivated();
    public void ActiveUpdate();
     
    public  void DoAction();

    public int CompareTo(IInteractAction other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return Priority.CompareTo(other.Priority);
    }

    public delegate void ChangedAvailability();
    public event ChangedAvailability OnChangedAvailability;

}
