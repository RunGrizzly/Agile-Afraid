using UnityEngine;

public abstract class ExecuteOnSelected : Ability
{
    //Select and change requires a selected block
    // [ReadOnly]
    // [ShowInInspector]
    public LetterBlock m_targetBlock = null;

    public override void Activate()
    {
        BrainControl.Get().eventManager.e_blockSelected.AddListener(SetTarget);
    }

    public override void Deactivate()
    {
        BrainControl.Get().eventManager.e_blockSelected.RemoveListener(SetTarget);
    }
    
    protected virtual void SetTarget(LetterBlock newTarget)
    {
        Debug.LogFormat(newTarget.gameObject,$"Set new target block to {newTarget}");
        m_targetBlock = newTarget;
        Debug.LogFormat(m_targetBlock,$"New target block set: {m_targetBlock}");
    }
}