using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum InputOrientation { Horiz, Vert, Unknown }
public enum InputDirection { Forwards, Backwards, Unknown }


[Serializable]
public class BlockInput
{
    public InputOrientation inputOrientation;
    List<BlockLine> possibleLines = new List<BlockLine>();
    public List<LetterBlock> blocks = new List<LetterBlock>();

    public List<BlockLine> compiledLines = new List<BlockLine>();

    public List<BlockLine> compiledWords = new List<BlockLine>();

    public List<char> characters = new List<char>();

    public bool isValidated;

    public BlockInput(List<LetterBlock> _blocks, bool validate, bool score)
    {
        foreach (LetterBlock block in _blocks)
        {
            AddToInput(block);
        }

        if (validate)
        {
            Validate(score);
            return;
        }
    }
    public BlockLine GetRow(LetterBlock block)
    {
        BlockLine row = new BlockLine("row");
        row.blocks.AddRange(BrainControl.Get().grid.letterBlocks.Cast<LetterBlock>().ToList().Where(x => x.gridRef.y == block.gridRef.y));
        //Remove any previously inputted
        row.blocks.RemoveAll(x => BrainControl.Get().sessionManager.currentSession.currentLevel.inputs.SelectMany(y => y.blocks).Contains(x));
        return row;
    }

    public BlockLine GetColumn(LetterBlock block)
    {
        BlockLine column = new BlockLine("column");
        column.blocks.AddRange(BrainControl.Get().grid.letterBlocks.Cast<LetterBlock>().ToList().Where(x => x.gridRef.x == block.gridRef.x));
        //Remove any previously inputted
        column.blocks.RemoveAll(x => BrainControl.Get().sessionManager.currentSession.currentLevel.inputs.SelectMany(y => y.blocks).Contains(x));
        return column;
    }

    public List<BlockLine> SetPossibleLines()
    {

        if (blocks.Count >= 2)
        {
            Debug.Log("Calculating input direction");
            return new List<BlockLine>() { DirectionFilter() };
        }

        return new List<BlockLine>() { GetRow(blocks[blocks.Count - 1]), GetColumn(blocks[blocks.Count - 1]) };

    }

    public bool BlockIsPossible(LetterBlock checkBlock)
    {
        if (possibleLines.Count < 1) return true;
        return (possibleLines.SelectMany(x => x.blocks).ToList().Contains(checkBlock));

    }

    //Add a letter block to the input
    public void AddToInput(LetterBlock a)
    {

        // //Ongoing input orientation check
        // inputOrientation = GridTools.GetLineOrientation(new BlockLine("input", blocks));


        Debug.Log("A new block was added to the current input");
        blocks.Add(a);
        characters.Add(a.baseLetter.character);
        possibleLines = SetPossibleLines();
    }

    public void RemoveFromInput(LetterBlock r)
    {
        Debug.Log("Removing active block: " + r);

        blocks.Remove(r);//Not called on second levels
        characters.Remove(r.baseLetter.character);
        r.SetLockState(LockState.unlocked);
        r.Empty();

        if (blocks.Count > 0) possibleLines = SetPossibleLines();
        else possibleLines = new List<BlockLine>() { };

        Debug.Log("Removal complete");
    }



    public void Compile()
    {
        //Got an orientation ( horizontal or vertical)
        inputOrientation = GridTools.GetLineOrientation(new BlockLine("input", blocks));

        //Go through blocks
        for (int i = 0; i < blocks.Count; i++)
        {
            //Compile blocklines of blocks that intersect the the block horizontally and vertically
            compiledLines = new List<BlockLine>()
            {
                //Horizontally
                new BlockLine("horizLine", GridTools.GetFilledCross(blocks[i])[0].blocks.OrderBy(x => x.gridRef.x).ToList()),
                //Vertically
                new BlockLine("vertLine", GridTools.GetFilledCross(blocks[i])[1].blocks.OrderBy(x => x.gridRef.y).ToList()),
            };

            //Horiz direction //If the input direction is horizontal only do this once
            ////////////////////////////
            if (compiledLines[0].blocks.Count > 1)
            {
                if (!(inputOrientation == InputOrientation.Horiz && i > 0))
                {
                    (string forwards, string backwards) horizset = GridTools.WordFromLine(compiledLines[0]);

                    //If this horizontal blocks are valid forwards
                    if (BrainControl.Get().sessionManager.sessionSettings.permittedWords.Validate(horizset.forwards)) compiledWords.Add(new BlockLine("forwards", compiledLines[0].blocks));

                    //If this horizontal blocks are valid backward
                    else if (BrainControl.Get().sessionManager.sessionSettings.permittedWords.Validate(horizset.backwards)) compiledWords.Add(new BlockLine("backwards", compiledLines[0].blocks.OrderByDescending(x => x.gridRef.x).ToList()));

                    else
                    {
                        BrainControl.Get().eventManager.e_validateFail.Invoke();
                        return;
                    }
                }
            }
            ////////////////////////////

            //Vert direction //If the input direction is vertucal only do this once
            ////////////////////////////
            if (compiledLines[1].blocks.Count > 1)
            {
                if (!(inputOrientation == InputOrientation.Vert && i > 0))
                {
                    (string forwards, string backwards) vertset = GridTools.WordFromLine(compiledLines[1]);

                    //If this horizontal blocks are valid forwards
                    if (BrainControl.Get().sessionManager.sessionSettings.permittedWords.Validate(vertset.forwards)) compiledWords.Add(new BlockLine("forwards", compiledLines[1].blocks));

                    //If this horizontal blocks are valid backward
                    else if (BrainControl.Get().sessionManager.sessionSettings.permittedWords.Validate(vertset.backwards)) compiledWords.Add(new BlockLine("backwards", compiledLines[1].blocks.OrderByDescending(x => x.gridRef.y).ToList()));

                    else
                    {
                        BrainControl.Get().eventManager.e_validateFail.Invoke();
                        return;
                    }
                }
            }
            ////////////////////////////
        }
        //Single letter inputs get here but are not registered properly
        Debug.Log("The input was validated");
        Validate(true);
    }

    public void Validate(bool score)
    {

        isValidated = true;
        foreach (LetterBlock block in blocks)
        {
            block.SetLockState(LockState.locked);
        }

        //Validate the input by locking it and triggering a validate success event
        //Send the input
        if (score) BrainControl.Get().eventManager.e_validateSuccess.Invoke(this);
        BrainControl.Get().eventManager.e_navUpdate.Invoke();
    }


    BlockLine DirectionFilter()
    {
        //Else find out what direction we are going
        //Find out which coords are the same
        if (blocks[0].gridRef.x == blocks[1].gridRef.x)
        {
            inputOrientation = InputOrientation.Vert;
            return GetColumn(blocks[blocks.Count - 1]);
        }
        else
        {
            inputOrientation = InputOrientation.Horiz;
            return GetRow(blocks[blocks.Count - 1]);
        }
    }

    List<LetterBlock> ExistingInputs()
    {
        return BrainControl.Get().sessionManager.currentSession.currentLevel.inputs.Where(x => x != this).SelectMany(x => x.blocks).ToList();
    }


}