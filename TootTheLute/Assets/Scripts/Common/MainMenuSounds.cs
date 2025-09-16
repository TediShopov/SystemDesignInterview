using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/MainMenuSonds", order = 1)]
public class MainMenuSounds : ScriptableObject
{
    public AK.Wwise.Event OnMenuClick;
    public AK.Wwise.Event OnMenuPlay;
    public AK.Wwise.Event OnMenuBack;
    public AK.Wwise.Event SoundtrackPlay;
    public AK.Wwise.Event SoundtrackStop;
}
