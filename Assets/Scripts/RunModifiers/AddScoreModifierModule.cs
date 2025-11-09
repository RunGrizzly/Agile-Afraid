using UnityEngine;

[CreateAssetMenu(fileName = "AddScoreModule", menuName = "Data/ModifierModules/NewAddScoreModule", order = 1)]
public class AddScoreSimpleModifierModule : RunModifierModule
{
    [SerializeField]
    private int m_addAmount = 1;
    
    public override void OnScore(ScoreData scoreData)
    {
        Debug.LogFormat($"Added {m_addAmount} to {scoreData.Amount}");
        scoreData.Amount += m_addAmount;
        Debug.LogFormat($"Score amount is now {scoreData.Amount}");
    }
}