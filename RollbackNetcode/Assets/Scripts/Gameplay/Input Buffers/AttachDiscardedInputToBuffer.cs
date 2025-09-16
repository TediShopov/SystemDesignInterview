using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachDiscardedInputToBuffer : MonoBehaviour
{

    private FighterController Fighter;

    // Start is called before the first frame update
    void Start()
    {
        Fighter = this.GetComponent<FighterController>();
        if (!Fighter.State.isEnemy)
        {
            // Fighter.OnInputProcessed +=
            //(InputFrame f) => { StaticBuffers.Instance.PlayerRB.GetComponent<FighterController>()
            //    .InputBuffer.Enqueue(f); };

            Fighter.InputBuffer.OnInputFrameDiscarded +=
         (InputFrame f) =>
         {
             StaticBuffers.Instance.PlayerRB.GetComponent<FighterController>()
.InputBuffer.Enqueue(f);
         };
        }
        else
        {
            //  Fighter.OnInputProcessed +=
            //(InputFrame f) => {
            //    StaticBuffers.Instance.EnemyRB.GetComponent<FighterController>()
            //  .InputBuffer.Enqueue(f);
            //};

            Fighter.InputBuffer.OnInputFrameDiscarded +=
        (InputFrame f) =>
        {
            StaticBuffers.Instance.EnemyRB.GetComponent<FighterController>()
.InputBuffer.Enqueue(f);
        };
        }

    }
}
