using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public enum LineOrientation { Horiz, Vert, Unknown }
public enum LineDirection { Forwards, Backwards, Unknown }


[Serializable]
public class BlockInput
{
    //Raw placed blocks
    public List<LetterBlock> PlacedBlocks = new List<LetterBlock>();
    
    //Granular access to validated info
    public List<BlockLine> ValidatedLines = new List<BlockLine>();
    public List<LetterBlock>  ValidatedBlocks => ValidatedLines.SelectMany(x => x.blocks).ToList();
    public List<char> ValidatedCharacters => ValidatedBlocks.Select(y => y.baseLetter.character).ToList();

    [ReadOnly]
    public List<string> ValidatedStrings = new List<string>();
    
    public bool isValidated = false;

    public BlockInput(List<LetterBlock> blocks, bool validate, bool score)
    {
        foreach (LetterBlock block in blocks)
        {
            AddToInput(block);
        }

        if (validate)
        {
            SetValidatedState(validate);
        }
    }
    public List<BlockLine> PossibleLines()
    {
        // if (blocks.Count >= 2)
        // {
        //     Debug.Log("Calculating input direction");
        //     return new List<BlockLine>() { DirectionFilter() };
        // }

        //Get the full cross by counting empties
        var cross = GridTools.GetCross(PlacedBlocks[PlacedBlocks.Count - 1],true);
        
        //Return the cross row and column
        return new List<BlockLine>() { cross[0], cross[1] };
    }

    //So this is failing
    public bool BlockIsPossible(LetterBlock checkBlock)
    {
        var possibleLines = PossibleLines();
        
        if (possibleLines.Count < 1)
        {
            Debug.LogFormat($"Block possible lines <1 - possible");
            return true;
        }
        
        if (possibleLines.SelectMany(x => x.blocks).ToList().Contains(checkBlock))
        {
            Debug.LogFormat($"Possible lines containes the check block");
            return true;
        }
        
        Debug.LogFormat($"Block not possible.");
        return false;
    }

    //Add a letter block to the input
    public void AddToInput(LetterBlock newLetterBlock)
    { 
        //Ongoing input orientation check
        Debug.Log("A new block was added to the current input");
        
        PlacedBlocks.Add(newLetterBlock);
        //characters.Add(newLetterBlock.baseLetter.character);
        
        //possibleLines = PossibleLines();
    }

    public void RemoveFromInput(LetterBlock r)
    {
        Debug.Log("Removing active block: " + r);
        
        PlacedBlocks.Remove(r);
        //characters.Remove(r.baseLetter.character);
        
        r.SetLockState(LockState.unlocked);
        r.Empty();
        
        if (PlacedBlocks.Count > 0)
        {
            //possibleLines = SetPossibleLines();
        }
        
        else
        {
            //possibleLines = new List<BlockLine>() { };
        }

        Debug.Log("Removal complete");
    }

    [Button]
    public void SetValidatedState(bool newValidatedState)
    {
        //We know the input is valid
        if (newValidatedState)
        {
            isValidated = true;
            
            foreach (LetterBlock block in PlacedBlocks)
            {
                //Don't lock
                block.SetLockState(LockState.locked);
                block.gameObject.layer = LayerMask.NameToLayer("Navigable");
            }

            //So
            //We want to check if this validate WOULD trigger a grid success
            
            //If so
            //Go ahead an call the grid done
            
            //If not
            //Just lock the input and continue
            
            //So the validation flow needs to be different
            
            //Input
            //Compiles
            //'Validates' that an acceptable word was made
            
            //Grid
            //When a word is validated
            //'Validates' the grid that a path has been made
            
            BrainControl.Get().eventManager.e_validateSuccess.Invoke(this);
            
            //So maybe this shouldn't happen automatically
            //Or it should centrally do the check 
            //BrainControl.Get().eventManager.e_navUpdate.Invoke();
        }

        else
        {
            isValidated = false;
            BrainControl.Get().eventManager.e_validateFail.Invoke();
        }
    }
    
    public void Compile()
    {
        List<BlockLine> compiledLines = new List<BlockLine>();
        
        //Go through blocks
        for (int i = 0; i < PlacedBlocks.Count; i++)
        {
            Debug.LogFormat($"Checking lines for block {PlacedBlocks[i].baseLetter.character}");
            
            var filledCross = GridTools.GetCross(PlacedBlocks[i]);
            
            //Compile blocklines of blocks that intersect the the block horizontally and vertically
            var newCompiledLines = new List<BlockLine>()
            {
                //Horizontally
                new BlockLine("horizLine",filledCross[0].blocks.OrderBy(x => x.gridRef.x).ToList()),
                
                //Vertically
                new BlockLine("vertLine", filledCross[1].blocks.OrderBy(x => x.gridRef.y).ToList()),
            };

            //If the letter does not make a long enough word horizontally - remove the check
            //We nullify so as to not modify the collection
            if (newCompiledLines[0].blocks.Count < 2)
            {
                Debug.LogWarningFormat($"Did not compile any valid words in the horizontal orientation");
                newCompiledLines[0] = null;
            }
            
            //If the letter does not make a long enough word vertically - remove the check
            if(newCompiledLines[1].blocks.Count < 2)
            {
                Debug.LogWarningFormat($"Did not compile any valid words in the vertical orientation");
                newCompiledLines[1] = null;
            }
            
            //Add non nullified lines
            compiledLines.AddRange(newCompiledLines.Where(x=>x!=null));
        }
       
        //We shouldn't autovalidate
        //We should return the compiled lines and validate externally
        bool isValid = true;

        foreach (BlockLine blockLine in compiledLines)
        {
            (string forwards, string backwards) input = GridTools.WordFromLine(blockLine);

            bool forwardValid = BrainControl.Get().runManager.CurrentRun.RunSettings.permittedWords.CheckValidWord(input.forwards);
            bool backwardValid = BrainControl.Get().runManager.CurrentRun.RunSettings.permittedWords.CheckValidWord(input.backwards);

            if (forwardValid && !ValidatedStrings.Contains(input.forwards))
            {
                Debug.LogFormat($"Blockline {blockLine.lineOrientation} validated forward: {input.forwards}");
                ValidatedLines.Add(blockLine);
                ValidatedStrings.Add(input.forwards);
            }
            
            if (backwardValid && !ValidatedStrings.Contains(input.backwards))
            {
                Debug.LogFormat($"Blockline {blockLine.lineOrientation} validated backwards: {input.backwards}");
                ValidatedLines.Add(blockLine);
                ValidatedStrings.Add(input.backwards);
            }
            
            if (!forwardValid && !backwardValid)
            {
                Debug.LogWarning($"Invalid word in both directions for line {blockLine.lineOrientation}: '{input.forwards}' and '{input.backwards}'");
                isValid = false;
                break;
            }
        }
        
        SetValidatedState(isValid);
    }



    // BlockLine DirectionFilter()
    // {
    //     //Else find out what direction we are going
    //     //Find out which coords are the same
    //     if (blocks[0].gridRef.x == blocks[1].gridRef.x)
    //     {
    //         inputOrientation = LineOrientation.Vert;
    //         return GetColumn(blocks[blocks.Count - 1]);
    //     }
    //     else
    //     {
    //         inputOrientation = LineOrientation.Horiz;
    //         return GetRow(blocks[blocks.Count - 1]);
    //     }
    // }

    // List<LetterBlock> ExistingInputs()
    // {
    //     return BrainControl.Get().runManager.currentRun.ActiveLevel.inputs.Where(x => x != this).SelectMany(x => x.blocks).ToList();
    // }


}