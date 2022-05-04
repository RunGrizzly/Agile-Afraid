using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Level
{
    public LevelData data;

    //This will be controlled by the grid
    //public LetterBlock activeBlock;
    public List<BlockInput> inputs = new List<BlockInput>();

    public BlockInput startInput;
    public BlockInput targetInput;

    //Initialiser for the new level
    public Level(LevelData _data)
    {
        data = _data;
    }

    public Boolean InputInProgress()
    {
        bool p = (inputs.Count > 0) ? !LatestInput().isValidated : false;
        Debug.Log("Input in progress?: " + p);
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
        BrainControl.Get().grid.startPosition = startInput.blocks[0].transform.position;

        foreach (LetterBlock block in startInput.blocks)
        {
            block.SetAsStart();
        }
    }

    void SetTargetInput(BlockInput input)
    {
        targetInput = input;
        BrainControl.Get().grid.targetPosition = targetInput.blocks[0].transform.position;

        foreach (LetterBlock block in targetInput.blocks)
        {
            block.SetAsTarget();
        }
    }

    public IEnumerator TrackLevel()
    {
        //Session setup
        Debug.Log("A new level was initialised");

        //Call the generation method on the grid generator
        Task g = new Task(BrainControl.Get().grid.Generate(data.gridDims));

        //FillRack at level load
        BrainControl.Get().eventManager.e_fillRack.Invoke((data.rackSize / 2) + 1, false);

        //Wait while the grid is still generating
        yield return new WaitWhile(() => g.Running);
        yield return new WaitForEndOfFrame();

        BrainControl.Get().eventManager.e_updateUI.Invoke();

        //Place word seeds
        foreach (WordRequest request in data.seeds)
        {
            if (!GridTools.WordIntoLine(request, true)) GridTools.WordIntoLine(new WordRequest(request.word, PlacementType.Random), true);
        }

        SetStartInput(inputs[0]);
        SetTargetInput(inputs[inputs.Count - 1]);

        BrainControl.Get().eventManager.e_levelLoaded.Invoke(this);

        while (Brain.ins.sessionManager.currentSession != null && BrainControl.Get().sessionManager.currentSession.currentLevel == this)
        {
            Debug.Log("A level is running");
            yield return null;
        }
        Debug.Log("Session ended");
        //Level cleanup
    }
}

