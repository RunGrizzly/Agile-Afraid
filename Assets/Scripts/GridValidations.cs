using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class GridValidations
{
    public static bool SinglePath(GridGenerator gridSnapshot)
    {
        foreach (LetterBlock letterBlock in gridSnapshot.letterBlocks)
        {
            if (letterBlock.lockState == LockState.unlocked)
            {
                return false;
            }
        }
        
        return true;
    }
    
    public static bool AllBlocksFilled(GridGenerator gridSnapshot)
    {
        foreach (LetterBlock letterBlock in gridSnapshot.letterBlocks)
        {
            if (letterBlock.lockState == LockState.unlocked)
            {
                return false;
            }
        }
        
        return true;
    }
    
    public static bool AllUniqueCharacters(GridGenerator gridSnapshot)
    {
        //Store all user inputted blocks from the level inputs
        List<char> inputChars =  BrainControl.Get().runManager.CurrentRun.ActiveLevel.inputs.SelectMany(x=>x.ValidatedCharacters).ToList();
        
        //Check that their respective characters are unique
        return inputChars.Distinct().Count() == inputChars.Count;
    }
    
    public static bool UsedAllTiles(GridGenerator gridSnapshot)
    {
        return BrainControl.Get().rack.letterTiles.Count == 0;
    }
    
    public static bool NoWordsRepeatCharacter(GridGenerator gridSnapshot)
    {
        var inputs = BrainControl.Get().runManager.CurrentRun.ActiveLevel.inputs;

        foreach (var input in inputs)
        {
            //Only check inputs that were validated
            if (input.isValidated)
            {
                //If the word has repeat characters
                if (input.ValidatedCharacters.Distinct().Count() != input.ValidatedCharacters.Count)
                {
                    //Fail
                    return false;
                }
            }
        }

        return true;
    }
}