using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

//A new instance of a level - driven by a level data
[Serializable]
public class Level
{
    //Source
    public LevelData Data;
    
    //Transient
    public List<BlockInput> inputs = new List<BlockInput>();
    public List<BlockInput> ScoredInputs => inputs.Where(x => x.isValidated).ToList();
    public BlockInput startInput;
    public BlockInput targetInput;

    public int Score = 0;

    public bool passed = false;

    //Initialiser for the new level
    public Level(LevelData data)
    {
        Data = data;
    }

    //Should this just be accessible via brain?
    //Do we really need to pass the level?
    // private void Complete(Level level)
    // {
    //     //This triggers the level passed trigger
    //     passed = true;
    // }

    public IEnumerator Track()
    {
        //Tracking this level - kicks off a new grid generation
        int randomID = Random.Range(000, 999);
        
        //Call the generation method on the grid generator
        Task g = new Task(BrainControl.Get().grid.Generate(Data.gridData));
        
        //Wait while the grid is still generating
        yield return new WaitWhile(() => g.Running);
        yield return new WaitForEndOfFrame();

        //Level load broadcasts
        BrainControl.Get().eventManager.e_updateUI.Invoke();
        BrainControl.Get().eventManager.e_levelLoaded.Invoke(this);
        
        while (!passed)
        {
            //Debug.LogFormat($"Level {randomID} is being tracked");
            yield return null;
        }
        
        //Debug.Log("Level tracking complete");
        BrainControl.Get().eventManager.e_levelSuccess.Invoke(this);
    }
    
    public Boolean InputInProgress()
    {
        bool p = (inputs.Count > 0) ? !LatestInput().isValidated : false;
        //Debug.Log("Input in progress?: " + p);
        return p;
    }

    public void RemoveInput(BlockInput r)
    {
        inputs.Remove(r);
    }

    //get latest(current) input
    public BlockInput LatestInput()
    {
        return inputs[inputs.Count - 1];
    }

    void SetStartInput(BlockInput input)
    {
        startInput = input;
        BrainControl.Get().grid.startPosition = startInput.PlacedBlocks[0].transform.position;

        foreach (LetterBlock block in startInput.PlacedBlocks)
        {
            block.SetAsStart();
        }
    }

    void SetTargetInput(BlockInput input)
    {
        targetInput = input;
        BrainControl.Get().grid.targetPosition = targetInput.PlacedBlocks[0].transform.position;

        foreach (LetterBlock block in targetInput.PlacedBlocks)
        {
            block.SetAsTarget();
        }
    }

    //This is called directly when the grid generator detects a completed level
    public void Complete()
    {
        Debug.Log("Level complete called");
        
        Score = 0;
        
        int letterScore = 0;
        
        //Calculate score
        foreach (BlockInput scoredInput in ScoredInputs)
        {
            foreach (LetterBlock letterBlock in scoredInput.PlacedBlocks)
            {
                letterScore += letterBlock.baseLetter.score;
            }
        }

      
        //Flat completion
        Score += 2;      
        
        //Add letter score
        Score += letterScore;
        
        //Add word amount
        Score += inputs.Count;
        
        //This triggers the level passed trigger
        passed = true;
    }
    
    public void ScoreInput(BlockInput scoredInput)
    {
     ScoredInputs.Add(scoredInput);
    }
}

