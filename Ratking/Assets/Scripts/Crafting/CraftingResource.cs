using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CraftingReosurceType 
{ Iron, String, Food}

[RequireComponent(typeof(Collectible))]
public class CraftingResource : MonoBehaviour
{
    [SerializeField] public CraftingReosurceType Type;
    [SerializeField] public int Amount;

}
