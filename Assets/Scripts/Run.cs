using System;
using System.Collections.Generic;
using System.Linq;
using CodingJar;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public enum TimePoolType
{
    Unknown,
    Squeaky,
    Core,
    Grey
}

[Serializable]
public class TimePip
{
    public TimePoolType TimePoolType = TimePoolType.Unknown;
    public float MaxValue = 5;
    public float CurrentValue = 5;
    public TimePipWidget Widget = null;

    public TimePip(TimePoolType timePoolType, float maxValue, float startValue)
    {
        Debug.LogFormat("Added a new time pip");
        TimePoolType = timePoolType;
        MaxValue = maxValue;
        CurrentValue = startValue;
    }
    
    public float AddTime(float value)
    {
        float space = MaxValue - CurrentValue;
        float added = Mathf.Min(space, value);
        CurrentValue += added;
        
        Widget.PipFace.fillAmount = CurrentValue / MaxValue;
        
        return value - added; // leftover
    }

    public float RemoveTime(float value)
    {
        Debug.LogFormat($"Removing time from {this} of type {TimePoolType}");
        float removed = Mathf.Min(CurrentValue, value);
        CurrentValue -= removed;

        Widget.PipFace.fillAmount = CurrentValue / MaxValue;
        
        return value - removed; // leftover to remove
    }
}


[Serializable]
public class TimePool
{
    [Readonly]
    public List<TimePip> TimePips = new List<TimePip>();
    
    public float CurrentValue
    {
        get
        {
            float current = 0;
            
            foreach (TimePip pip in TimePips)
            {
                current += pip.CurrentValue;
            }

            return current;
        }
    }
    
    public float MaxValue
    {
        get
        {
            float max = 0;
            
            foreach (TimePip pip in TimePips)
            {
                max += pip.MaxValue;
            }
            
            return max;
        }
    }
    
    public TimePool(List<TimePip> timePips)
    {
        TimePips = timePips;
    }

    public float AddWithOverflow(float addValue)
    {
        float remaining = addValue;

        foreach (var pip in TimePips)
        {
            if (remaining <= 0)
            {
                break;
            }
            
            remaining = pip.AddTime(remaining);
        }

        return remaining; // overflow left after filling all pips
    }

    public float RemoveWithOverflow(float elapseValue, bool killOnDeplete = false)
    {
        float remaining = elapseValue;
        
        foreach (var pip in TimePips.AsEnumerable().Reverse())
        {
            if (remaining <= 0)
            {
                break;
            }
          
            remaining = pip.RemoveTime(remaining);
            
            //If a pip depleted remove it
            if (killOnDeplete)
            {
                //If there is still grey remaining
                //Then the previous pip was depleted
                if (remaining > 0)
                {
                    GameObject.Destroy(pip.Widget.gameObject);
                    TimePips.Remove(pip);
                    BrainControl.Get().eventManager.e_pipRemoved.Invoke(pip);
                }
            }
        }

        return remaining; // amount we tried to remove but couldn’t
    }
}

public enum InterruptType { Pause, Win, Fail }

[Serializable]
public class Run
{
    public int ID = 0000;
    
    public bool IsPaused;
    
    GridGenerator gridGenerator;
    
    //public TileRack tileRack;
    
    public RunSettings RunSettings;
    
    public int Score;
    
    public List<string> recentWords = new List<string>();
    
    public float Elapsed;
    public float WorkingTime => SqueakyTime.CurrentValue + CoreTime.CurrentValue + GreyTime.CurrentValue;
    public float MaxTime => SqueakyTime.MaxValue + CoreTime.MaxValue + GreyTime.MaxValue;

    public TimePool SqueakyTime = null;
    public TimePool CoreTime = null;
    public TimePool GreyTime = null;
    public List<TimePip> AllPips => SqueakyTime.TimePips.Concat(CoreTime.TimePips).Concat(GreyTime.TimePips).ToList();
    public List<TimePip> WorkingPips => CoreTime.TimePips.Where(x => x.CurrentValue == x.MaxValue).ToList();
    
    public LevelSet ActiveLevelSet = null;
    public Level ActiveLevel = null;
    
    //Scoring actions
    public UnityAction<List<char>, int, bool, bool> newRackListener = null;
    public UnityAction<int, bool, bool> fillRackListener = null;
    public UnityAction<Letter, bool, bool> getTileListener = null;
    public UnityAction<bool, bool> getConsonantListener = null;
    public UnityAction<bool, bool> getVowelListener = null;
    public UnityAction<BlockInput> validateSuccessListener = null;
    public UnityAction validateFailListener = null;
    public UnityAction<Level> levelSuccessListener = null;

    public int ActiveLevelIndex
    {
        get
        {
            if (ActiveLevelSet != null && ActiveLevel != null)
            {


                return ActiveLevelSet.GetLevelIndex(ActiveLevel);
            }

            return 0;
        }
    }
    
    //Creates a new run and assigns run settings
    public Run(RunSettings runSettings, LevelSet levelSet)
    {
      RunSettings = runSettings;
      ActiveLevelSet = levelSet;
    }
    
     //Nothing in here should actually kick off any dynamic data
    //It should only set up run data objects
    public void Initialise(LevelSet levelSet)
    {
        ID = UnityEngine.Random.Range(0, 9999);
        
        //Initial level load
        ActiveLevelSet = levelSet;
        ActiveLevel = new Level(ActiveLevelSet.Levels[0]);
        
        Elapsed = 0;
        
        SqueakyTime = new TimePool(new List<TimePip>());
        CoreTime = new TimePool(new List<TimePip>());
        GreyTime = new TimePool(new List<TimePip>());
        
        //Squeaky time is one pip of 5 seconds
        AddSqueakyPips(new List<TimePip>(1){new TimePip(TimePoolType.Squeaky, 5,5)});
        
        //Core time is made up denominations of pips
        List<TimePip> corePips = new List<TimePip>()
        {
            //8x1 //8
            new TimePip(TimePoolType.Core, 1f, 1f),
            new TimePip(TimePoolType.Core, 1f, 1f),
            new TimePip(TimePoolType.Core, 1f, 1f),
            new TimePip(TimePoolType.Core, 1f, 1f),
            new TimePip(TimePoolType.Core, 1f, 1f),
            new TimePip(TimePoolType.Core, 1f, 1f),
            new TimePip(TimePoolType.Core, 1f, 1f),
            new TimePip(TimePoolType.Core, 1f, 1f),
            
            //4x2 //8
            new TimePip(TimePoolType.Core, 2f,2f),
            new TimePip(TimePoolType.Core, 2f,2f),
            new TimePip(TimePoolType.Core, 2f,2f),
            new TimePip(TimePoolType.Core, 2f,2f),
            
            //2x4 //8,
            new TimePip(TimePoolType.Core, 4f, 4f),
            new TimePip(TimePoolType.Core, 4f, 4f),
            
            //1x8 //8
            new TimePip(TimePoolType.Core, 8f, 8f)
        };
        
        AddCorePips(corePips);
        
        //Grey time can be added and subtracted from - transient
        AddGreyPips(new List<TimePip>());
        
        //Set up appropriate event responses for the run
        VerifyResponses();
    }
    
    private void VerifyResponses()
    {
        //Scoring actions
        if (newRackListener == null)
        {
            newRackListener = (seedChars, fillTo, costsScore, costsPips) =>
            {
                bool validated = true;

                //If the active level has a live input don't allow new tiles
                if (ActiveLevel.InputInProgress())
                {
                    Debug.LogWarningFormat($"Cannot generate new tiles when there is a live input");
                    validated = false;
                }
                
                //Did the request come with a score cost?
                else if (costsScore)
                {
                    int scoreCost = RunSettings.ActiveScoringRubrik.newRackScoreCost;
                    
                    //Do we have enough score to cover the cost?
                    if (Score >= RunSettings.ActiveScoringRubrik.newRackScoreCost)
                    {
                        //Take the score away
                        ModifyScore(-scoreCost);
                    }
                    else
                    {
                        validated = false;
                    }
                }

                //If we haven't already failed validation and the request wants to remove pips
                if (validated && costsPips)
                {
                    int pipCost = RunSettings.ActiveScoringRubrik.newRackPipCost;
                    
                    //If we have enough pips to cover the pip cost
                    if (WorkingPips.Count >= pipCost)
                    {
                        //Remove the pips
                        RemovePips(pipCost);
                    }
                    else
                    {
                        validated = false;
                    }
                }

                //If we are still in a valid state - execute the request
                if (validated)
                {
                    BrainControl.Get().eventManager.e_newRackRequest.Invoke(seedChars, fillTo, costsScore, costsPips);
                }
            };
        }
        
        if (fillRackListener == null)
        {
            fillRackListener = (fillTo, costsScore, costsPips) =>
            {
                bool validated = true;
                
                //If the active level has a live input don't allow new tiles
                if (ActiveLevel.LatestInput().PlacedBlocks.Count > 0)
                {
                    Debug.LogWarningFormat($"Cannot generate new tiles when there is a live input");
                    validated = false;
                }

               else if (costsScore)
                {
                    int scoreCost = RunSettings.ActiveScoringRubrik.fillRackScoreCost;
                    
                    if (Score >= RunSettings.ActiveScoringRubrik.fillRackScoreCost)
                    {
                        ModifyScore(-scoreCost);
                    }
                    else
                    {
                        validated = false;
                    }
                }

                if (validated && costsPips)
                {
                    int pipCost = RunSettings.ActiveScoringRubrik.fillRackPipCost;
                    
                    if (WorkingPips.Count >= pipCost)
                    {
                        RemovePips(pipCost);
                    }
                    else
                    {
                        validated = false;
                    }
                }

                //If we are still in a valid state - execute the request
                if (validated)
                {
                    BrainControl.Get().eventManager.e_fillRackRequest.Invoke(fillTo, costsScore, costsPips);
                }
            };
        }

        if (getTileListener == null)
        {
            getTileListener = (baseLetter, costsScore, costsPips) =>
            {
                bool validated = true;
                
                //If the active level has a live input don't allow new tiles
                if (ActiveLevel.LatestInput().PlacedBlocks.Count > 0)
                {
                    Debug.LogWarningFormat($"Cannot generate new tiles when there is a live input");
                    validated = false;
                }

                else if (costsScore)
                {
                    int scoreCost = RunSettings.ActiveScoringRubrik.specificTileScoreCost;
                    
                    if (Score >= scoreCost)
                    {
                        ModifyScore(-scoreCost);
                    }
                    else
                    {
                        validated = false;
                    }
                }

                if (validated && costsPips)
                {
                    int pipCost = RunSettings.ActiveScoringRubrik.specificTilePipCost;
                    
                    if (WorkingPips.Count >= pipCost)
                    {
                        RemovePips(pipCost);
                    }
                    else
                    {
                        validated = false;
                    }
                }

                if (validated)
                {
                    BrainControl.Get().eventManager.e_getTile.Invoke(baseLetter, costsScore, costsPips);
                }
            };
        }

        if (getConsonantListener == null)
        {
            getConsonantListener = ( costsScore, costsPips) =>
            {
                bool validated = true;
                
                //If the active level has a live input don't allow new tiles
                if (ActiveLevel.LatestInput().PlacedBlocks.Count > 0)
                {
                    Debug.LogWarningFormat($"Cannot generate new tiles when there is a live input");
                    validated = false;
                }

                else if (costsScore)
                {
                    int scoreCost = RunSettings.ActiveScoringRubrik.consonantScoreCost;
                    
                    if (Score >= scoreCost)
                    {
                        ModifyScore(-scoreCost);
                    }
                    else
                    {
                        validated = false;
                    }
                }

                if (validated && costsPips)
                {
                    int pipCost = RunSettings.ActiveScoringRubrik.consonantPipCost;
                    
                    if (WorkingPips.Count >= pipCost)
                    {
                        RemovePips(pipCost);
                    }
                    else
                    {
                        validated = false;
                    }
                }

                if (validated)
                {
                    BrainControl.Get().eventManager.e_getConsonantRequest.Invoke(costsScore, costsPips);
                }
            };
        }

        if (getVowelListener == null)
        {
            getVowelListener = ( costsScore,costsPips) =>
            {
                bool validated = true;
                
                //If the active level has a live input don't allow new tiles
                if (ActiveLevel.LatestInput().PlacedBlocks.Count > 0)
                {
                    Debug.LogWarningFormat($"Cannot generate new tiles when there is a live input");
                    validated = false;
                }

                else if (costsScore)
                {
                    int scoreCost = RunSettings.ActiveScoringRubrik.vowelScoreCost;
                    
                    if (Score >= scoreCost)
                    {
                        ModifyScore(-scoreCost);
                    }
                    else
                    {
                        validated = false;
                    }
                }

                if (validated && costsPips)
                {
                    int pipCost = RunSettings.ActiveScoringRubrik.vowelPipCost;
                    
                    if (WorkingPips.Count >= pipCost)
                    {
                        RemovePips(pipCost);
                    }
                    else
                    {
                        validated = false;
                    }
                }

                if (validated)
                {
                    BrainControl.Get().eventManager.e_getVowelRequest.Invoke(costsScore, costsPips);
                }
            };
        }
        
        if (validateSuccessListener == null)
        {
            validateSuccessListener = (blockInput) =>
            {
                int score = 0;
                
                foreach (BlockLine blockLine in blockInput.ValidatedLines)
                {
                    string validated = GridTools.WordFromLine(blockLine).forwards;
                
                    // Debug.Log("Compiled word set: " + GridTools.WordFromLine(b));
                
                    //Figure out how much the compiled block line is worth
                    int scoreAdd = RunSettings.ActiveScoringRubrik.ScoreFromBlocks(blockLine);
                    
                    Debug.Log("Compiled word set is worth: " + scoreAdd);
                    
                    score += scoreAdd;
                   
                    recentWords.Add(validated);
                    
                    //Truncate our list
                    if (recentWords.Count >= RunSettings.recentWordBias)
                    {
                        recentWords.RemoveAt(recentWords.Count - 1);
                    }
                }
                
                ModifyScore(score);
            };
        }
        
        if (validateFailListener == null)
        {
            validateFailListener = () =>
            {
                Debug.Log("Validation failed");
                ModifyScore(-RunSettings.ActiveScoringRubrik.validateFailPenalty);
            };
        }
        
        if (levelSuccessListener == null)
        {
            levelSuccessListener = (Level level) =>
            {
                ModifyScore(level.Score);
            };
        }
    }
    
    public void SetPaused(bool state)
    {
        IsPaused = state;

        if (IsPaused == true)
        {
            BrainControl.Get().eventManager.e_pauseRun.Invoke();
        }
        else
        {

            BrainControl.Get().eventManager.e_unpauseRun.Invoke();
        }
    }

    //Return a number that represents the amount of times the given string appears in the recent words buffer
    public int RecencyScore(string check)
    {
        return recentWords.Where(x => x == check).ToList().Count;
    }

    public void AddSqueakyPips(List<TimePip> newPips)
    {
        SqueakyTime.TimePips.AddRange(newPips);
    }
    
    public void AddCorePips(List<TimePip> newPips)
    {
        CoreTime.TimePips.AddRange(newPips);
    }

    public void AddGreyPips(List<TimePip> newPips)
    {
        foreach (var greyPip in newPips)
        {
            GreyTime.TimePips.Add(greyPip);
         
            //Let the app know a new pip was created
            Brain.ins.eventManager.e_pipAdded.Invoke(greyPip);
        }
    }
    
    [Button]
    public void AddWorkingTime(float timeAdd)
    {
        // so there is an order to this
        //We fill up from timeAdd
        //And add subsequent remaining time
        //Until the remainder becomes grey time
        float remainderA = SqueakyTime.AddWithOverflow(timeAdd);
        float remainderB = CoreTime.AddWithOverflow(remainderA);
        
        //The remainder becomes a grey pip
        AddGreyPips(new List<TimePip>(){new TimePip(TimePoolType.Grey, remainderB,remainderB)});
        
        // float remainderC = GreyTime.AddWithOverflow(remainderB);
    }
    
    public void RemoveWorkingTime(float timeElapse)
    {
        // so there is an order to this
        //We fill up from timeAdd
        //And add subsequent remaining time
        //Until the remainder becomes grey time
        float remainderA = GreyTime.RemoveWithOverflow(timeElapse,true);
        float remainderB = CoreTime.RemoveWithOverflow(remainderA);
        float remainderC = SqueakyTime.RemoveWithOverflow(remainderB);
    }
    public void RemovePips(int pipsToRemove)
    {
        //int pipsRemoved = 0;
        List<TimePip> consumablePips = new List<TimePip>();
        consumablePips.AddRange(GreyTime.TimePips.Where(x => x.CurrentValue > 0));
        consumablePips.AddRange(CoreTime.TimePips.Where(x => x.CurrentValue > 0));

        if (consumablePips.Count >= pipsToRemove)
        {
            var workingSet = consumablePips.AsEnumerable().Reverse().ToList();
            
            for (int i = 0; i < pipsToRemove; i++)
            {
                workingSet[i].RemoveTime(workingSet[i].CurrentValue);   
            }
        }
        else
        {
            Debug.LogWarningFormat($"Tried to remove {pipsToRemove} but there are only {consumablePips.Count} consumable pips available."); 
        }

        // return pipsRemoved; // How many we actually removed
    }
    // public void End()
    // {
    //     //This SHOULD tell the run tracker to stop
    //     //And then it will unsubscribe
    //     
    //     //Is there a way to duck out without nullifying?
    //     BrainControl.Get().runManager.RunTracker = null;
    // }

    //Add or remove a score value from the run
    private void ModifyScore(int modification)
    {
        Debug.Log($"Modifying the score by {modification}");
        
        //Modify the score
        Score = (int)Mathf.Clamp((float)Score + (float)modification, 0, Mathf.Infinity);
        
        AddWorkingTime(modification/4);
        
        //Reflect it in the UI
        BrainControl.Get().eventManager.e_updateUI.Invoke();
            
        if (Score <= 0)
        {
            Brain.ins.eventManager.e_failRun.Invoke();
        }
    }
}

