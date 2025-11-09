using UnityEngine;

public abstract class Ability : ScriptableObject
{
    //The required xp/score/coin/mana to execute
    protected int m_executeCost = 0;
    public abstract void OnExecute();
    // public abstract void OnExecute(LetterBlock letterBlock);
    // public abstract void OnExecute(LetterTile letterTile);
    // public abstract void OnExecute(Letter letter);

    public abstract void Activate();
    public abstract void Deactivate();
}