using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CopySpriteOnStart : MonoBehaviour
{

    public GameObject PlayerGameObject;
    private SpriteRenderer _playerSpriteRendererRef;

    private SpriteRenderer _spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerGameObject != null)
        {
            _spriteRenderer = this.GetComponent<SpriteRenderer>();

            _playerSpriteRendererRef = PlayerGameObject.GetComponentInChildren<SpriteRenderer>();
            this.transform.localScale = _playerSpriteRendererRef.gameObject.transform.lossyScale;
            this._spriteRenderer.sprite = _playerSpriteRendererRef.sprite;

        }

    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerGameObject != null)
        {
            _spriteRenderer = this.GetComponent<SpriteRenderer>();
            this._spriteRenderer.sprite = _playerSpriteRendererRef.sprite;
            Debug.Log($"Sprite is : {_spriteRenderer.sprite.name}");
        }
    }
}
