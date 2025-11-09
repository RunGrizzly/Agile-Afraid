using UnityEngine;

[CreateAssetMenu(fileName = "ChangeBlockLetter", menuName = "Data/Abilities/ExecuteOnSelected/New ChangeBlockLetter", order = 1)]
public class ChangeBlockLetter : ExecuteOnSelected
{
    [SerializeField]
    private Letter m_newLetter = new Letter('a', 1);
    
    public override void OnExecute()
    {
        Debug.LogFormat($"Executed {this}");
        Debug.LogFormat($"Execute target is {m_targetBlock}");
        m_targetBlock.SetLetter(m_newLetter);
    }
}