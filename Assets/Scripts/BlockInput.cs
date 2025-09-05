using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

public enum LineOrientation { Horiz, Vert, Unknown }
public enum LineDirection { Forwards, Backwards, Unknown }


[Serializable]
public class BlockInput
{
    //Raw placed blocks
    [ShowInInspector]
    public List<LetterBlock> PlacedBlocks { get; private set; } = new List<LetterBlock>();
    public List<BlockLine> ValidatedLines { get; private set; } = new List<BlockLine>();
    public List<LetterBlock>  ValidatedBlocks => ValidatedLines.SelectMany(x => x.blocks).ToList();
    public List<char> ValidatedCharacters => ValidatedBlocks.Select(y => y.BaseLetter.character).ToList();

    [ReadOnly]
    public List<string> ValidatedStrings = new List<string>();
    
    [ReadOnly]
    public List<BlockLine> PossibleLines = new List<BlockLine>();

    public bool isValidated = false;
    
    public BlockInput(LetterBlock initialBlock, bool validate, bool score)
    {
        //We initialise the input with the block
        AddToInput(initialBlock);
        
        var adjacentBlocks = GridTools.GetAdjacentBlocks(initialBlock);
        
        //We now have the input block
        //And its adjacencies
        if (validate)
        {
            SetValidatedState(validate);
        }
    }
    
    //Set possible lines with current input blocks
    private List<BlockLine> UpdatePossibleLines()
    {
        List<BlockLine> possibleLines = new List<BlockLine>();
        
        
        //Only count first two
        for (int i = 0; i < Mathf.Min(2,PlacedBlocks.Count); i++)
        {
            Debug.LogFormat($"Updating possible line with input block {i}");
            
            var cross = GridTools.GetCross(PlacedBlocks[i],true);
            //Remove anything that doesn't match
            if (i > 0)
            {
                Debug.LogFormat($"block {i} will confirm the line direction");
                //If both inputs share the horizontal line
                //Set it
                
                if( new HashSet<LetterBlock>(possibleLines[0].blocks).SetEquals(cross[0].blocks))
                {
                    Debug.LogFormat($"Second line is colinear with the horizontal line");
                    possibleLines = new List<BlockLine>()
                    {
                    possibleLines[0]
                    };
                }

                //If both inputs share the vertical line
                //Set it
                else if( new HashSet<LetterBlock>(possibleLines[1].blocks).SetEquals(cross[1].blocks))
                {
                    Debug.LogFormat($"Second line is colinear with the vertical line");
                    possibleLines = new List<BlockLine>()
                    {
                        possibleLines[1]
                    }; 
                }
            }
            else
            {
                possibleLines.Add(cross[0]);
                possibleLines.Add(cross[1]);
                Debug.LogFormat($"possibleLines 0 count = {possibleLines[0].blocks.Count}");
                Debug.LogFormat($"possibleLines 1 count = {possibleLines[1].blocks.Count}");
            }
        }

        return possibleLines;
    }
    
    public void RevealPossibleLines()
    {
        foreach (var blockLine in PossibleLines)
        {
            foreach (LetterBlock latestInputPlacedBlock in blockLine.blocks)
            {
                //A framework for animating the color of a block temporarily
                //The grid should be able to request this effect (amongst others)
                Material material = latestInputPlacedBlock.MeshRenderer.material;
                Material animatedMaterial = new Material(material);
                latestInputPlacedBlock.MeshRenderer.material = animatedMaterial;
    
                LeanTween.value(0, 1, 0.25f).setEase(LeanTweenType.easeOutExpo)
                .setOnUpdate((val) =>
                {
                    animatedMaterial.SetFloat("_normalEffect", Mathf.Lerp(material.GetFloat("_normalEffect"), 1.5f, val));
                })
                .setOnComplete(() =>
                {
                    LeanTween.value(0, 1, 0.25f).setEase(LeanTweenType.easeOutExpo)
                        .setOnUpdate((val) =>
                        {
                            animatedMaterial.SetFloat("_normalEffect", Mathf.Lerp( 1.5f, material.GetFloat("_normalEffect"), val));
                        })
                        .setOnComplete(() =>
                        {
                            latestInputPlacedBlock.MeshRenderer.material = material;
                        });
                });
            }
        }
    }
    
    //Add a letter block to the input
    public void AddToInput(LetterBlock newLetterBlock)
    { 
        PlacedBlocks.Add(newLetterBlock);
        PossibleLines = UpdatePossibleLines();
        RevealPossibleLines();
    }

    public void RemoveFromInput(LetterBlock letterBlock)
    {
        PlacedBlocks.Remove(letterBlock);
       
        BrainControl.Get().runManager.CurrentRun.RackData.Add(letterBlock.letter);
        letterBlock.SetLockState(LockState.unlocked);
        letterBlock.Empty();
        
        if (PlacedBlocks.Count < 1)
        {
            BrainControl.Get().eventManager.e_cancelInput.Invoke(this);
        }
        else
        {
            PossibleLines = UpdatePossibleLines();
            RevealPossibleLines();   
        }
    }

    [Button]
    public void SetValidatedState(bool newValidatedState)
    {
        //We know the input is valid
        if (newValidatedState)
        {
            isValidated = true;
            
            //We have no way to actually mark the correct line //This needs to change
            //So this validates the entire input block by block
            foreach (LetterBlock block in PlacedBlocks)
            {
                //Don't lock
                block.SetLockState(LockState.locked);
                block.gameObject.layer = LayerMask.NameToLayer("Navigable");

                block.letter.OnValidated();
            }

            //The input was validated - we know here that we can bestow bonuses etc
            BrainControl.Get().eventManager.e_validateSuccess.Invoke(this);
        }

        else
        {
            isValidated = false;
            BrainControl.Get().eventManager.e_validateFail.Invoke();
            BrainControl.Get().uiManager.PrintMessage("Invalid word");
        }
    }
    
    public void Compile()
    {
        List<BlockLine> compiledLines = new List<BlockLine>();
        
        //Go through blocks
        for (int i = 0; i < PlacedBlocks.Count; i++)
        {
            Debug.LogFormat($"Checking lines for block {PlacedBlocks[i].BaseLetter.character}");
            
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
                //Debug.LogFormat($"Blockline {blockLine.lineOrientation} validated forward: {input.forwards}");
                ValidatedLines.Add(blockLine);
                ValidatedStrings.Add(input.forwards);
            }
            
            if (backwardValid && !ValidatedStrings.Contains(input.backwards))
            {
                //Debug.LogFormat($"Blockline {blockLine.lineOrientation} validated backwards: {input.backwards}");
                ValidatedLines.Add(blockLine);
                ValidatedStrings.Add(input.backwards);
            }
            
            if (!forwardValid && !backwardValid)
            {
                //Debug.LogWarning($"Invalid word in both directions for line {blockLine.lineOrientation}: '{input.forwards}' and '{input.backwards}'");
                isValid = false;
                break;
            }
        }
        
        //Can we pass on context (what was actually validated)
        SetValidatedState(isValid);
    }
}