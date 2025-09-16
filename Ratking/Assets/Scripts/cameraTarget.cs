using UnityEngine;

public class cameraTarget : MonoBehaviour
{
    public Vector2 offSet;
    public Transform Player;
    public Transform Target;
    [HideInInspector] public float LerpValue = 1;
    public float LerpSpeed = 0.2f;
    Vector2 prevPos;

    // Update is called once per frame
    void Update()
    {
        transform.position = (Vector2)Player.position + offSet;
        if (Target.position != transform.position)
        {

            if (LerpValue <= 1)
            {
                if (LerpValue == 0)
                    prevPos = Target.localPosition;

                LerpValue = Mathf.MoveTowards(LerpValue, 1, LerpSpeed * Time.deltaTime);
                Target.localPosition = Vector2.Lerp(prevPos, Vector2.zero, LerpValue);

                if (LerpValue == 1)
                    LerpValue += 1;
            }
        }
    }
}
