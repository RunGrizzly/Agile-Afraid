using UnityEngine;

public abstract class RunModifierModule : ScriptableObject
{
    public abstract void OnScore(ScoreData scoreData);
}