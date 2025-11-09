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

    public List<BlockInput> ScoredInputs
    {
        get
        {
            return inputs.Where(x => x.isValidated).ToList();
        }
    }
    
    public Resolution Resolution = Resolution.None;

    // //This can be used in conjuction with a scoring rubrik
    // public Rack Rack = null;
    
    //Initialiser for the new level
    public Level(LevelData data)
    {
        Data = data;
    }
    
    public IEnumerator Track()
    {
        //Tracking this level - kicks off a new grid generation
        int randomID = Random.Range(000, 999);
        
        //Call the generation method on the grid generator
        Task g = new Task(BrainControl.Get().Grid.Generate(Data.gridData));
        
        //Wait while the grid is still generating
        yield return new WaitWhile(() => g.Running);
        yield return new WaitForEndOfFrame();

        //Level load broadcasts
        BrainControl.Get().eventManager.e_updateUI.Invoke();
        BrainControl.Get().eventManager.e_levelLoaded.Invoke(this);
        
        //While we don't have a resolution yet
        while (Resolution == Resolution.None)
        {
            //Debug.LogFormat($"Level {randomID} is being tracked");
            yield return null;
        }
        
        //Debug.Log("Level tracking complete");
        BrainControl.Get().eventManager.e_levelSuccess.Invoke(this);
    }
    
    public BlockInput InputInProgress()
    {
        if (LatestInput() == null)
        {
            return null;
        }
        
        else
        {
            if (LatestInput().isValidated)
            {
                return null;
            }
            else
            {
                return LatestInput();
            }
        }
    }

    public void RemoveInput(BlockInput r)
    {
        inputs.Remove(r);
    }

    //get latest(current) input
    //I think this counts the intial seed input
    private BlockInput LatestInput()
    {
        return inputs.Count > 0? inputs[inputs.Count - 1]: null;
    }

    void SetStartInput(BlockInput input)
    {
        // startInput = input;
        BrainControl.Get().Grid.StartPosition = input.PlacedBlocks[0].transform.position;

        foreach (LetterBlock block in input.PlacedBlocks)
        {
            block.SetAsStart();
        }
    }

    void SetTargetInput(BlockInput input)
    {
        BrainControl.Get().Grid.TargetPosition = input.PlacedBlocks[0].transform.position;

        foreach (LetterBlock block in input.PlacedBlocks)
        {
            block.SetAsTarget();
        }
    }

    //This is called directly when the grid generator detects a completed level
    public void Complete()
    {
        Debug.Log("Level complete called");
        //This triggers the level passed trigger
        Resolution = Resolution.Win;
    }
    
    // public void ScoreInput(BlockInput scoredInput)
    // {
    //  ScoredInputs.Add(scoredInput);
    // }
}

