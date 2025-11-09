//Reusable as both a character passive and obtainable run mods
using UnityEngine;
using UnityEngine.Events;

public abstract class RunModification : ScriptableObject
{
    //Respondable run events
    public UnityEvent<ScoreData> ScoreEvent = new UnityEvent<ScoreData>();
    public UnityEvent<LetterBlock> BlockValidatedEvent = new UnityEvent<LetterBlock>();
    public UnityEvent<LetterTile> TileDiscardedEvent = new UnityEvent<LetterTile>();
}

public class ToyPassive : RunModification
{
  
}