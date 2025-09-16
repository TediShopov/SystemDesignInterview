using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProduceSoudnActivatable : Activatable
{
    public float ForceOfSound = 0.0f;
    public override void Activate()
    {
        SoundGenerator.Instance.SpawnSound(ForceOfSound, this.transform.position, 8,8, this.gameObject);
        Destroy(this.gameObject);
    }
}
