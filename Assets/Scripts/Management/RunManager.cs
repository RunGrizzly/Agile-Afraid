using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum RunResult {Win, Fail, Restarted, Quit}

public class RunManager : MonoBehaviour
{
    public List<Run> Runs = new List<Run>();
    public Run CurrentRun => Runs[0];
    public Task RunTracker;

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
        // //Manager scope run controls
        // BrainControl.Get().eventManager.e_winRun.AddListener(()=>
        //     {
        //         ResolveRun(RunResult.Win);
        //     });
        // BrainControl.Get().eventManager.e_failRun.AddListener(()=>
        // {
        //     ResolveRun(RunResult.Lose);
        // });
        //
        // BrainControl.Get().eventManager.e_restartRun.AddListener(()=>
        // {
        //     ResolveRun(RunResult.Restarted);
        // });
        //
        //
        // BrainControl.Get().eventManager.e_quitToMenu.AddListener(()=>
        // {
        //     ResolveRun(RunResult.Quit);
        // });
        
        //Why is this here?
        Brain.ins.eventManager.e_clearBlock.AddListener((c) =>
        {
            CurrentRun.ActiveLevel.LatestInput().RemoveFromInput(c);
            //If this would kill the input
            if (CurrentRun.ActiveLevel.LatestInput().PlacedBlocks.Count == 0)
            {
                CurrentRun.ActiveLevel.RemoveInput(CurrentRun.ActiveLevel.LatestInput());
            }
        });

        //A new input is called
        //Why is any of this here?
        //////////////////////
        Brain.ins.eventManager.e_beginInput.AddListener((a) =>
        {
            // //Check for any initial adjacency
            CurrentRun.ActiveLevel.inputs.Add(new BlockInput(new List<LetterBlock>() { /*BrainControl.Get().grid.CheckAdjacency(a),*/ a }, false, false));
        });
        //////////////////////

        //The current input is updated
        //////////////////////
        Brain.ins.eventManager.e_updateInput.AddListener((u) =>
        {
            CurrentRun.ActiveLevel.LatestInput().AddToInput(u);
        });
        //////////////////////

        //The current input is ended
        //////////////////////
        Brain.ins.eventManager.e_endInput.AddListener(() =>
        {
            Debug.LogFormat("Input ended");
            CurrentRun.ActiveLevel.LatestInput().Compile();
        });
        //////////////////////
        ///
        
        
        
        //We actually want to be the source of this event - it should pass an already loaded run
        //Brain.ins.eventManager.e_newRun.AddListener(StartNewRun);
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

    //Prepare a new run with run settings and a level set
    public void LoadNewRun()
    {
        
    }
    
    
    //Kick off the currently loaded run
    public void StartNewRun(RunSettings runSettings, LevelSet levelSet)
    {
        //Creates a new run from run settings
        Runs.Insert(0, new Run(runSettings, levelSet));
        RunTracker = new Task(TrackRun(CurrentRun));
    }
    
    public void StartNewRun(Run sourceRun)
    {
        //Creates a new run from run settings
        Runs.Insert(0, new Run(sourceRun.RunSettings, sourceRun.ActiveLevelSet));
        RunTracker = new Task(TrackRun(CurrentRun));
    }
    
    public void ResolveRun(RunResult result)
    {
        if (CurrentRun != null)
        {
            //Can we move this to the actual run?
            // BrainControl.Get().eventManager.e_newRackRequest.RemoveListener(CurrentRun.newRackListener);
            // BrainControl.Get().eventManager.e_fillRackRequest.RemoveListener(CurrentRun.fillRackListener);
            // BrainControl.Get().eventManager.e_getTile.RemoveListener(CurrentRun.getTileListener);
            // BrainControl.Get().eventManager.e_getConsonantRequest.RemoveListener(CurrentRun.getConsonantListener);
            // BrainControl.Get().eventManager.e_getVowelRequest.RemoveListener(CurrentRun.getVowelListener);

            //Input validation
            BrainControl.Get().eventManager.e_validateSuccess.RemoveListener(CurrentRun.validateSuccessListener);
            BrainControl.Get().eventManager.e_validateFail.RemoveListener(CurrentRun.validateFailListener);

            BrainControl.Get().eventManager.e_levelSuccess.RemoveListener(CurrentRun.levelSuccessListener);
        }

        // CurrentRun.End();

        switch (result)
        {
         case RunResult.Win:
             //CurrentRun = null;
             BrainControl.Get().eventManager.e_winRun.Invoke();
             break;
         
         case RunResult.Fail:
             //CurrentRun = null;
             BrainControl.Get().eventManager.e_failRun.Invoke();
             break;
         
         case RunResult.Restarted:
             //Current run is the last run added (0) (so the one just played)
             StartNewRun(CurrentRun);
             //BrainControl.Get().eventManager.e_restartRun.Invoke();
             break;
         
         case RunResult.Quit:
             //CurrentRun = null;
             BrainControl.Get().eventManager.e_quitToMenu.Invoke();
             break;
        }
    }

    // public void OnQuitToMenu()
    // {
    //     CurrentRun = null;
    // }

    // public void RestartCurrentRun()
    // {
    //     var currentSettings = CurrentRun.RunSettings;
    //     var currentLevelSet = CurrentRun.ActiveLevelSet;
    //     
    //     CurrentRun = new Run(currentSettings);
    //     
    //     //Kill old task?
    //     
    //     
    //     RunTracker = new Task(TrackRun(currentLevelSet));
    // }
    
    //Track the run - listen to events and execute method on its behalf
    //This happens whenever the player select a run type from the main menu
    //Subscriptions should un register when the run ends
    public IEnumerator TrackRun(Run run)
    {
        Debug.LogFormat($"Started tracking a new run");
        
        //Initialise the current run
        CurrentRun.Initialise(run.ActiveLevelSet);
        
         //We can wait while the current run is validated
         while (CurrentRun.RunSettings == null)
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
            CurrentRun.newRackListener == null ||
            CurrentRun.fillRackListener == null ||
            CurrentRun.getTileListener == null ||
            CurrentRun.getConsonantListener == null ||
            CurrentRun.getVowelListener == null ||
            CurrentRun.validateSuccessListener == null ||
            CurrentRun.validateFailListener == null
        )
        {
            Debug.LogWarning($"Current run has not initialised its listeners");
            yield return null;
        }
        
        #region Run Started Subscriptions
         // BrainControl.Get().eventManager.e_newRackRequest.AddListener(CurrentRun.newRackListener);
         // BrainControl.Get().eventManager.e_fillRackRequest.AddListener(CurrentRun.fillRackListener);
         // BrainControl.Get().eventManager.e_getTile.AddListener(CurrentRun.getTileListener);
         // BrainControl.Get().eventManager.e_getConsonantRequest.AddListener(CurrentRun.getConsonantListener);
         // BrainControl.Get().eventManager.e_getVowelRequest.AddListener(CurrentRun.getVowelListener);
         
         BrainControl.Get().eventManager.e_validateSuccess.AddListener(CurrentRun.validateSuccessListener);
         BrainControl.Get().eventManager.e_validateFail.AddListener(CurrentRun.validateFailListener);
         
         //When the level indicates success
         BrainControl.Get().eventManager.e_levelSuccess.AddListener(CurrentRun.levelSuccessListener);   
        #endregion
         
        //Fill the rack to prepare the run
        BrainControl.Get().eventManager.e_newRackRequest.Invoke
        (
            CurrentRun.ActiveLevel.Data.RackSeeds,
            CurrentRun.ActiveLevel.Data.rackSize, 
            false, 
            false
        );
        
        //Run manager has prepared the current run
        BrainControl.Get().eventManager.e_newRun.Invoke(CurrentRun);
        
            //So we can await each level here
            foreach (var level in run.ActiveLevelSet.Levels)
            {
                CurrentRun.ActiveLevel = new Level(level);
                var levelTracker = CurrentRun.ActiveLevel.Track();

                //While this runtracker is valid - track it
                //This will pop when it changes
                while (levelTracker.MoveNext())
                {
                    #region Dev Shortcuts

                    //Level skip
                    if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.Alpha0))
                    {
                        Debug.Log("DEVELOPER CONTROL: Skip Level");
                        CurrentRun.ActiveLevel.Complete();
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
                        BrainControl.Get().rack.EmptyRack();
                    }

                    #endregion

                    //Do I really want to be updating the UI constantly?
                    Brain.ins.eventManager.e_updateUI.Invoke();

                    if (CurrentRun.WorkingTime <= 0)
                    {
                        Debug.LogWarning("Run ran out of time and ended");
                        ResolveRun(RunResult.Fail);
                        yield break;
                    }
                    
                    if (CurrentRun.ActiveLevel.Data.IsTimed)
                    {
                        CurrentRun.RemoveWorkingTime(Time.deltaTime * (run.IsPaused? 0: 1));
                    }

                    //Debug.Log("Run is still being tracked");
                    CurrentRun.Elapsed += Time.deltaTime * (run.IsPaused? 0: 1);

                    yield return levelTracker.Current;
                }
            }

        #region Run Ended Subscription Cleanup
       ResolveRun(RunResult.Win);
        #endregion
    }
}
