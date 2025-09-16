using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class DoorLock : MonoBehaviour
{
    [SerializeField] public bool IsDanger = false;
    [HideInInspector] public bool IsUnlocked = false;
    [SerializeField] public int ValueNeeded = 0;
    [SerializeField] public DoorLockAnimationData AnimationData;

    public Sprite LockedSprite;
    public Sprite UnlockedSprite;

    private SpriteRenderer _renderer;
    private KeyBasedProgression _progression;


    public bool CanUnlock()
    {
        if (IsDanger)
            return this._progression.KeysEarned < ValueNeeded;
        else
            return this._progression.KeysEarned >= ValueNeeded;
    } 

    void Start()
    {
        this._progression = LevelData.GetProgressBar(IsDanger);
        this._renderer=this.gameObject.GetComponent<SpriteRenderer>();
        this._renderer.sprite = LockedSprite;
    }

    public void TryUnlock()
    {
        if (CanUnlock())
        {
            this.IsUnlocked = true;
            OnUnlock();
        }
        else
        {
            OnFailedUnlock();

        }

    }

    public void OnUnlock()
    {
        //this.transform.SetParent(null);
        this._renderer.sprite = this.UnlockedSprite;
        AnimationData.OnSucceededAnimation(this.gameObject);
        StartCoroutine(DelayForAnimation(AnimationData.OnSucceedDuration));
    }

    IEnumerator DelayForAnimation(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }

    public void OnFailedUnlock()
    {
        AnimationData.OnFailedAnimation(this.gameObject);
    }

    public void OnReset()
    {
        gameObject.SetActive(true);
        this._renderer.sprite = this.LockedSprite;
        AnimationData.OnFailedAnimation(this.gameObject);
    }
}
