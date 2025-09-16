using UnityEngine;

public class KeyBasedProgression : MonoBehaviour
{

    public delegate void ProgressionScoreChanged(int newValue);
    public ProgressionScoreChanged OnProgressionScoreChanged;

    private int _currentScore;
    public int CurrentScore
    {
        get { return _currentScore; }
        set
        {
            //Adding can never exceed max score
            if (value >= MaxScore)
            {
                _currentScore = MaxScore;
            }
            //Score can be reduced to only to the score of the last earned key
            else if (value <= KeysEarned * KeyScoreValue)
            {
                _currentScore = KeysEarned * KeyScoreValue;
            }
            else
            {
                _currentScore = value;
            }



            //Earn new keys based on score 
            KeysEarned = _currentScore / KeyScoreValue;


            //Fire an event that the value has changed
            if (OnProgressionScoreChanged != null)
            {
                OnProgressionScoreChanged(_currentScore);
            }
        }
    }

    public int ProgressionToNextKey()
    {
        return _currentScore % KeyScoreValue;
    }



    /*[HideInInspector]*/
    public int KeysEarned;
    [SerializeField][Range(1, 10)] public int MaxKeyCount;
    [SerializeField][Range(1, 1000)] public int KeyScoreValue;

    public int MaxScore => MaxKeyCount * KeyScoreValue;

    public void AddScore(int scoreToAdd)
    {
        CurrentScore += scoreToAdd;
    }


    public int DebugChangeValue = 15;


    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    CurrentScore -= DebugChangeValue;
        //}
        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    CurrentScore += DebugChangeValue;
        //}
    }

}