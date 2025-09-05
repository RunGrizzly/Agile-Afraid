using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum Resolution {None, Win, Fail, Restarted, Aborted}

public class RunManager : MonoBehaviour
{
    public List<Run> Runs = new List<Run>();
    public Run CurrentRun => Runs[0];
    public Task RunTracker;

    public RunSettings ActiveRunSettings = null;
    
    //Game framework broadly looks like this
    //Game////////////////
    //////////////////////
    //                  //
    //  //Run/////////
    //////////////////////
    //  //              //
    //  //  //Level///////
    //  //  //////////////
    //  //  //          //
    //  //  //          //
    //  //  //          //
    //  //  //          //
    //  //  //          //
    //////////////////////
    //////////////////////
    ///
    ///     //Track run here
    private void OnEnable()
    {
        //A new input is called
        //This is prevalidated so thats good
        //////////////////////
        Brain.ins.eventManager.e_beginInput.AddListener((newLetterBlock) =>
        {
            Debug.LogFormat($"Began a new input");
            var newInput = new BlockInput(newLetterBlock, false, false);
            CurrentRun.ActiveLevel.inputs.Add(newInput);
            
            //Show the new possible lines
            //RevealPossibleLines(newInput.PossibleLines);
        });
        //////////////////////

        //The current input is updated
        //////////////////////
        Brain.ins.eventManager.e_updateInput.AddListener((newLetterBlock) =>
        {
            var liveInput = CurrentRun.ActiveLevel.InputInProgress();

            if (liveInput != null)
            {
                liveInput.AddToInput(newLetterBlock);         
            }
            //RevealPossibleLines(latestInput.PossibleLines);
        });
        //////////////////////

        //The current input is ended
        //////////////////////
        Brain.ins.eventManager.e_confirmInput.AddListener(() =>
        {
            Debug.LogFormat("Input ended");
            var liveInput = CurrentRun.ActiveLevel.InputInProgress();

            if (liveInput != null)
            {
                CurrentRun.ActiveLevel.InputInProgress().Compile();           
            }
        });
        
        Brain.ins.eventManager.e_cancelInput.AddListener((input) =>
        {
            Debug.LogFormat("Input cancelled");
            CurrentRun.ActiveLevel.RemoveInput(input);
        });
        //////////////////////
    }

    private void OnDisable()
    {
        // //Manager scope run controls
        // BrainControl.Get().eventManager.e_winRun.RemoveListener(()=>
        // {
        //     ResolveRun(RunResult.Win);
        // });
        // BrainControl.Get().eventManager.e_failRun.RemoveListener(()=>
        // {
        //     ResolveRun(RunResult.Lose);
        // });
        //
        // BrainControl.Get().eventManager.e_restartRun.RemoveListener(()=>
        // {
        //     ResolveRun(RunResult.Restarted);
        // });
        //
        //
        // BrainControl.Get().eventManager.e_quitToMenu.RemoveListener(()=>
        // {
        //     ResolveRun(RunResult.Quit);
        // });
    }
    
    public void RevealInputBlocks(BlockInput blockInput)
    {
        foreach (LetterBlock latestInputPlacedBlock in blockInput.PlacedBlocks)
        {
            //A framework for animating the color of a block temporarily
            //The grid should be able to request this effect (amongst others)
            Material material = latestInputPlacedBlock.MeshRenderer.material;
            Material animatedMaterial = new Material(material);
            latestInputPlacedBlock.MeshRenderer.material = animatedMaterial;

            LeanTween.value(0, 1, 1f).setEase(LeanTweenType.punch).setOnUpdate((val) =>
            {
                animatedMaterial.SetFloat("_normalEffect", Mathf.Lerp(material.GetFloat("_normalEffect"), 5, val));
            }).setOnComplete(() =>
            {
                latestInputPlacedBlock.MeshRenderer.material = material;
            });
        }
    }
    
    public void RevealPossibleLines(List<BlockLine> possibleLines)
    {
        foreach (var blockLine in possibleLines)
        {
            foreach (LetterBlock latestInputPlacedBlock in blockLine.blocks)
            {
                //A framework for animating the color of a block temporarily
                //The grid should be able to request this effect (amongst others)
                Material material = latestInputPlacedBlock.MeshRenderer.material;
                Material animatedMaterial = new Material(material);
                latestInputPlacedBlock.MeshRenderer.material = animatedMaterial;

                LeanTween.value(0, 1, 1f).setEase(LeanTweenType.punch).setOnUpdate((val) => { animatedMaterial.SetFloat("_normalEffect", Mathf.Lerp(material.GetFloat("_normalEffect"), 5, val)); }).setOnComplete(() => { latestInputPlacedBlock.MeshRenderer.material = material; });
            }
        }
    }

    //Prepare a new run with run settings and a level set
    public void LoadNewRun()
    {
        
    }


    public void SetRunSettings(RunSettings newSettings)
    {
        ActiveRunSettings = newSettings;
    }
    
    //Kick off the currently loaded run
    public void StartNewRun(LevelSet levelSet)
    {
        //Creates a new run from run settings
        Runs.Insert(0, new Run(ActiveRunSettings, levelSet));
        RunTracker = new Task(TrackRun(CurrentRun));
    }
    
    
    //This fails on restart from daily challenge
    public void StartNewRun(Run sourceRun)
    {
        //Creates a new run from run settings
        Runs.Insert(0, new Run(sourceRun.RunSettings, sourceRun.ActiveLevelSet));
        RunTracker = new Task(TrackRun(CurrentRun));
    }
    
    
    //This breaks specifically when in a pause state
    //Win/Lose is fine
    //But only on mobile ...
    public void ResolveRun(Resolution result)
    {
        //When the run is resolved
        //We need to make sure to kill the current level
        //And kill the current run tracker
        Debug.LogFormat($"Resolving run with resolution {result}");
        
        if (CurrentRun != null)
        {
            //Input validation
            BrainControl.Get().eventManager.e_validateSuccess.RemoveListener(CurrentRun.validateSuccessListener);
            BrainControl.Get().eventManager.e_validateFail.RemoveListener(CurrentRun.validateFailListener);
            BrainControl.Get().eventManager.e_levelSuccess.RemoveListener(CurrentRun.levelSuccessListener);
        }

        CurrentRun.Resolution = result;
        
        switch (result)
        {
         case Resolution.Win:
             //CurrentRun = null;
             BrainControl.Get().eventManager.e_winRun.Invoke(CurrentRun);
             break;
         
         case Resolution.Fail:
             //CurrentRun = null;
             BrainControl.Get().eventManager.e_failRun.Invoke(CurrentRun);
             break;
         
         case Resolution.Restarted:
             //Current run is the last run added (0) (so the one just played)
             // CurrentRun.ActiveLevel.Resolution = Resolution.Restarted;
             StartNewRun(CurrentRun);
             break;
         
         case Resolution.Aborted:
             //This will kill the active level
             // CurrentRun.ActiveLevel.Resolution = Resolution.Aborted;
             // BrainControl.Get().eventManager.e_quitToMenu.Invoke();
             break;
        }
    }
    
    //Track the run - listen to events and execute method on its behalf
    //This happens whenever the player select a run type from the main menu
    //Subscriptions should un register when the run ends
    public IEnumerator TrackRun(Run run)
    {
        Debug.LogFormat($"Started tracking a new run");
        
        //Initialise the current run
        run.Initialise();
        
         //We can wait while the current run is validated
         while (run.RunSettings == null)
         {
             Debug.LogWarning($"Current run does not have a valid run settings yet.");
             yield return null;
         }
         
         
         while (BrainControl.Get().uiManager.TimePipHolder== null)
         {
             Debug.Log("Time pip holder is null");
             yield return null;
         }
         
        while (
            run.newRackListener == null ||
            run.fillRackListener == null ||
            run.getTileListener == null ||
            run.getConsonantListener == null ||
            run.getVowelListener == null ||
            run.validateSuccessListener == null ||
            run.validateFailListener == null
        )
        {
            Debug.LogWarning($"Current run has not initialised its listeners");
            yield return null;
        }
        
        #region Run Started Subscriptions
         BrainControl.Get().eventManager.e_validateSuccess.AddListener(run.validateSuccessListener);
         BrainControl.Get().eventManager.e_validateFail.AddListener(run.validateFailListener);
         BrainControl.Get().eventManager.e_levelSuccess.AddListener(run.levelSuccessListener);   
        #endregion
        
        // //Fill the rack to prepare the run
        // BrainControl.Get().eventManager.e_newRackRequest.Invoke
        // (
        //     run.ActiveLevel.Data.RackSeeds,
        //     run.ActiveLevel.Data.rackSize, 
        //     false, 
        //     false
        // );
        
        //Directly fill the runs rack and let it broadcast
        // CurrentRun.RackData.Fill( run.ActiveLevel.Data.RackSeeds, CurrentRun.ActiveLevelSet.ScoringRubrik);
        
        
        //Run manager has prepared the current run
        BrainControl.Get().eventManager.e_newRun.Invoke(run);

        
            //So we can await each level here
            //We need to be able to STOP this process
            for(int i = 0; i < run.ActiveLevelSet.Levels.Count; i++)
            {
                Debug.LogFormat($"New level index: {i}");
                
                var level = run.ActiveLevelSet.Levels[i];
                run.ActiveLevel = new Level(level);
                
                //This invokes the level initialiser
                var levelTracker = run.ActiveLevel.Track();

                //So we dont actually track the run - we just wait for the levels to resolve
                while (levelTracker.MoveNext())
                {
                    //If at any point the run has a resolution
                    if (run.Resolution != Resolution.None)
                    {
                        //Stop this coroutine
                        yield break;
                    }
                    
                    #region Dev Shortcuts

                    //Level skip
                    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.Alpha0))
                    {
                        Debug.Log("DEVELOPER CONTROL: Skip Level");
                        CurrentRun.ActiveLevel.Complete();
                        yield break;
                    }

                    //Pause
                    if (Input.GetKeyUp(KeyCode.Escape))
                    {
                        Debug.Log("Escape key pressed");
                        if (!CurrentRun.IsPaused)
                        {
                            CurrentRun.SetPaused(true);
                        }
                        else
                        {
                            CurrentRun.SetPaused(false);
                        }
                    }

                    //Empty rack
                    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.Alpha9))
                    {
                        Debug.Log("DEVELOPER CONTROL: Empty Rack");
                        CurrentRun.RackData.Empty();
                    }

                    #endregion
                    
                    if (run.ActiveLevelSet.IsTimed)
                    {
                        CurrentRun.RemoveWorkingTime(Time.deltaTime * (run.IsPaused ? 0 : 1));
                    }
                    
                    if (run.WorkingTime <= 0)
                    {
                        Debug.LogWarning("Run ran out of time and ended");
                        
                        //Resolving the run would end this coroutine naturally
                        ResolveRun(Resolution.Fail);
                    }
                    
                    //Should I send the run to confirm if its still valid?
                    Brain.ins.eventManager.e_updateUI.Invoke();
                    
                    //Debug.Log("Run is still being tracked");
                    run.Elapsed += Time.deltaTime * (run.IsPaused ? 0 : 1);
                    
                    yield return levelTracker.Current;
                }
                
                //This level ended its tracking phase
                //It has resolved
                Debug.LogFormat($"Level resolution: {run.ActiveLevel.Resolution}");
            }
            
            //We completed the iteration of levels
            //This will end the run
            ResolveRun(Resolution.Win);
            
            //Keep the coroutine in the station until it exits naturally
            yield return new WaitUntil( ()=>run.Resolution != Resolution.None);
    }
}
