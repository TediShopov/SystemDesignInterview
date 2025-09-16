using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum BeatObjectState
{
    Approaching,
    Successful,
    Unsuccessful,

}

[DisallowMultipleComponent]
[RequireComponent(typeof(RawImage))]
public class BeatObject : MonoBehaviour
{
    public Beat Beat;
    public float TimingOnTrack;
    public BeatmapTrack BeatTrackOn;
    public float ForceOnSuccessful;
    public float ForceOffsetFromCenter;

    #region Action Scheduling
    //Delegate for beat scheduled actions 
    public delegate void ExecuteOnBeatAction(BeatObject beat);
    //Does not matter if it is successful or unsucessful
    public event ExecuteOnBeatAction OnBeatPassed;

    #endregion



    public bool IsFirstApproaching => BeatTrackOn.BeatObjects[0] == this;

    public BeatObjectState State = BeatObjectState.Approaching;
    public BeatObjectState PreviousState = BeatObjectState.Approaching;
    public float GetBeatLength() => Beat.Length * BeatTrackOn.Beatmap.Conductor.Crochet;
    private RawImage rawImage;
    // Start is called before the first frame update
    void Start()
    {
        rawImage = this.GetComponent<RawImage>();
    }

    // Update is called once per frame
    void Update()
    {
        // If state changed
        if (PreviousState != this.State)
        {
            //Invoke any scheduled actions
            OnBeatPassed?.Invoke(this);

            if (this.State == BeatObjectState.Successful)
                BeatSuccessfulBehavior();
            if (this.State == BeatObjectState.Unsuccessful)
                BeatUnsuccessfulBehavior();
        }

        if (this.State == BeatObjectState.Approaching && this.IsFirstApproaching)
            rawImage.color = Color.yellow;

        PreviousState = this.State;
    }

    private void BeatUnsuccessfulBehavior()
    {
        rawImage.color = new Color(0, 0, 0, 0);
    }

    private void BeatSuccessfulBehavior()
    {
        rawImage.color = Color.green;
        rawImage.maskable = false;

        var rb = this.GetComponent<Rigidbody2D>();
        rb.simulated = true;
        //rb.AddForce(Vector2.up * ForceOnSuccessful,ForceMode2D.Impulse);

        Vector2 downRight = new Vector2(1.0f, -1);
        downRight.x += Random.Range(-0.5f, 0.0f);
        downRight.Normalize();
        Vector2 forceDirection = new Vector2(-1.0f, 1.0f);
        forceDirection.x += Random.Range(0.0f, 0.2f);
        forceDirection.y += Random.Range(-0.2f, 0.0f);
        forceDirection.Normalize();
        StartCoroutine(ApplyForceNextFrame(rb, forceDirection * ForceOnSuccessful, downRight * ForceOffsetFromCenter));
    }

    private IEnumerator ApplyForceNextFrame(Rigidbody2D rb, Vector2 force, Vector2 position)
    {
        yield return new WaitForFixedUpdate(); // Wait for physics update
        rb.AddForceAtPosition(force, position, ForceMode2D.Impulse);
    }
}
