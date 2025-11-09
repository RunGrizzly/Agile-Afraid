using UnityEngine;

[CreateAssetMenu(fileName = "MultiplyScoreModule", menuName = "Data/ModifierModules/NewMultiplyScoreModule", order = 1)]
public class ModifyScoreModifierModule : RunModifierModule
{
    [SerializeField]
    private int m_multiplyFactor = 1;
    
    public override void OnScore(ScoreData scoreData)
    {
        scoreData.Amount *= m_multiplyFactor;
    }
}