using UnityEngine;

[CreateAssetMenu(fileName = "ModifyLetterScore", menuName = "Data/Abilities/ExecuteOnSelected/New ModifyLetterScore", order = 1)]
public class ModifyLetterScore : ExecuteOnSelected
{
    [SerializeField]
    private int m_scoreAdd = 0;

    [SerializeField]
    private int m_scoreMult = 1;

    public override void OnExecute()
    {
        if (m_targetBlock == null)
        {
            Debug.LogWarning($"Tried to exexcute {this.name} without an actively selected block.");
            return;
        }

        Letter targetLetter = m_targetBlock.letter;
        int newScore = 0;

        newScore *= m_scoreMult;
        newScore += m_scoreAdd;

        m_targetBlock.SetLetter(new Letter(targetLetter.character, newScore));
    }
}
