using UnityEngine;

public class FlipPlayerScale : MonoBehaviour
{
    public SpriteRenderer PlayerSpriteRenderer;

    private Vector3 _defaultScale;

    private bool _flip;

    public bool MatchPlayerFlip => PlayerSpriteRenderer.flipX == _flip;
    // Start is called before the first frame update
    void Start()
    {
        _defaultScale = this.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {



        if (!MatchPlayerFlip)
        {
            this.transform.localScale = new Vector3(-this.transform.localScale.x, this.transform.localScale.y,
                this.transform.localScale.z);
            _flip = PlayerSpriteRenderer.flipX;
        }

    }
}
