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

    public List<char> characters = new List<char>();

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

    public bool isValidated;

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
            return new List<BlockLine>() { CalculateDirection() };
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


        bool horizIsGood = false;
        bool vertIsGood = false;

        //Go through each input block and figure out if we have made a horizontal or a vertical word
        foreach (LetterBlock block in blocks)
        {

            compiledLines = new List<BlockLine>();

            BlockLine horiz = new BlockLine("horizLine", GridTools.GetFilledCross(block)[0].blocks);

            BlockLine vert = new BlockLine("vert line", GridTools.GetFilledCross(block)[1].blocks);

            //Put each block list in order
            horiz.blocks = horiz.blocks.OrderBy(x => x.gridRef.x).ToList();
            vert.blocks = vert.blocks.OrderBy(x => x.gridRef.y).ToList();


            compiledLines.Add(horiz);
            compiledLines.Add(vert);
            // }


            string word = "";
            string reverseword = "";


            //Figure out horiz word and check
            if (horiz.blocks.Count > 1)
            {
                Debug.Log("Assembling horizontal forwards");
                //Forwards
                for (int i = 0; i < horiz.blocks.Count; i++)
                {
                    word += horiz.blocks[i].baseLetter.character;
                }

                if (BrainControl.Get().sessionManager.sessionSettings.permittedWords.Validate(word))
                {
                    Debug.Log("A word was validated: " + word);
                    horizIsGood = true;
                }
                else
                {
                    Debug.Log("A word was not validated: " + word);
                    horizIsGood = false;
                }

                if (!horizIsGood)
                {
                    Debug.Log("Assembling horizontal backwards");
                    //Backwards
                    for (int i = horiz.blocks.Count - 1; i > -1; i--)
                    {
                        reverseword += horiz.blocks[i].baseLetter.character;
                    }

                    if (BrainControl.Get().sessionManager.sessionSettings.permittedWords.Validate(reverseword))
                    {
                        Debug.Log("A word was validated: " + reverseword);
                        horizIsGood = true;
                    }
                    else
                    {
                        Debug.Log("A word was not validated: " + reverseword);
                        horizIsGood = false;
                    }
                }
            }
            else horizIsGood = true;





            //Figure out vert word and check
            if (vert.blocks.Count > 1)
            {

                word = "";
                reverseword = "";

                Debug.Log("Assembling vertical forwards");
                //Forwards
                for (int i = 0; i < vert.blocks.Count; i++)
                {
                    word += vert.blocks[i].baseLetter.character;
                }
                if (BrainControl.Get().sessionManager.sessionSettings.permittedWords.Validate(word))
                {
                    Debug.Log("A word was validated: " + word);
                    vertIsGood = true;
                }
                else
                {
                    Debug.Log("A word was not validated: " + word);
                    vertIsGood = false;
                }

                if (!vertIsGood)
                {
                    Debug.Log("Assembling vertical backwards");
                    //Backwards
                    for (int i = vert.blocks.Count - 1; i > -1; i--)
                    {
                        reverseword += vert.blocks[i].baseLetter.character;
                    }
                    if (BrainControl.Get().sessionManager.sessionSettings.permittedWords.Validate(reverseword))
                    {
                        Debug.Log("A word was validated: " + reverseword);
                        vertIsGood = true;
                    }
                    else
                    {
                        Debug.Log("A word was not validate: " + reverseword);
                        vertIsGood = false;
                    }
                }
            }
            else vertIsGood = true;

        }

        if (horizIsGood && vertIsGood) Validate(true);
        else BrainControl.Get().eventManager.e_validateFail.Invoke(10);

    }

    public void Validate(bool score)
    {

        isValidated = true;
        foreach (LetterBlock block in blocks)
        {
            block.SetLockState(LockState.locked);
        }
        if (score) BrainControl.Get().eventManager.e_validateSuccess.Invoke(20);
        BrainControl.Get().eventManager.e_navUpdate.Invoke();
    }


    BlockLine CalculateDirection()
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