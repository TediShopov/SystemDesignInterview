using UnityEngine;

public abstract class BeatObjectBase : MonoBehaviour
{
    private float lastBeat; // Serve as reference point
    public Conductor Conductor { get; private set; }
    //Per how many beats would the interaction happen
    public int BeatInteraction = 1;
    private int beatsSinceInteraction = 0;
    public void GetConductorFromScene()
    {
        this.Conductor = (Conductor)FindObjectOfType<Conductor>();
    }
    public void BeatTrackerUpdate()
    {
        if (this.Conductor == null) return;

        if (this.Conductor.SongPosition <= 0)
            return;

        if(this.Conductor.SongPosition > lastBeat + Conductor.Crochet)
        {
            beatsSinceInteraction++;
            if(beatsSinceInteraction >= BeatInteraction)
            {
                Debug.Log($"Crouchet: {Conductor.Crochet}");
                InteractOnBeat();
                //Rest the beats since interactions
                beatsSinceInteraction=0;
            }
            
            lastBeat += Conductor.Crochet;
        }

    }
    public abstract void InteractOnBeat();
    
}
