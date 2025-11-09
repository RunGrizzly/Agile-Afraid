using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class ScoreData
{
    public int Amount = 0;

    public ScoreData(int amount)
    {
        Amount = amount;
    }
}

public enum InterruptType { Pause, Win, Fail }

public enum TimePoolType
{
    Unknown,
    Squeaky,
    Core,
    Grey
}

[Serializable]
public class Run
{
    private int ID = 0000;
    
    public bool IsPaused;
    
    //Determines drop rates, available abilities, events etc
    //Can this be consolidated into settings?
    [ReadOnly]
    public Pet ActivePet = null;

    [ReadOnly]
    private List<Toy> m_toys = new List<Toy>();

    [ReadOnly]
    public RunSettings RunSettings;

    [FormerlySerializedAs("ActiveLevelSet")]
    [ReadOnly]
    public BossDungeon activeBossDungeon = null;

    public Level ActiveLevel = null;

    [ReadOnly]
    public Resolution Resolution = Resolution.None;

    //Score is decoupled from energy
    //But they are achieved from the same source (scoring words)
    public int Score;

    //So there should be multiple ways to earn K
    //But not intrinsically linked to score
    //Awarded K at end of run
    //Pets can also have K generation
    public int Energy;
    
    public List<string> recentWords = new List<string>();

    public RackData RackData = null;
    
    public float Elapsed;
    public float WorkingTime => SqueakyTime.CurrentValue + CoreTime.CurrentValue + GreyTime.CurrentValue;
    public float MaxTime => SqueakyTime.MaxValue + CoreTime.MaxValue + GreyTime.MaxValue;

    private TimePool SqueakyTime = null;
    private TimePool CoreTime = null;
    private TimePool GreyTime = null;
    public List<TimePip> AllPips => SqueakyTime.TimePips.Concat(CoreTime.TimePips).Concat(GreyTime.TimePips).ToList();
    public List<TimePip> WorkingPips => CoreTime.TimePips.Where(x => x.CurrentValue == x.MaxValue).ToList();
    
    //Scoring actions
    [HideInInspector]
    public UnityAction<bool, bool> emptyRackListener = null;
    [HideInInspector]
    public UnityAction<List<char>, int, bool, bool> newRackListener = null;
    [HideInInspector]
    public UnityAction<int, bool, bool> fillRackListener = null;
    [HideInInspector]
    public UnityAction<Letter, bool, bool> getTileListener = null;
    [HideInInspector]
    public UnityAction<bool, bool> getConsonantListener = null;
    [HideInInspector]
    public UnityAction<bool, bool> getVowelListener = null;
    [HideInInspector]
    public UnityAction<BlockInput> validateSuccessListener = null;
    [HideInInspector]
    public UnityAction validateFailListener = null;
    [HideInInspector]
    public UnityAction<Level> levelSuccessListener = null;
    
    public int ActiveLevelIndex
    {
        get
        {
            if (activeBossDungeon != null && ActiveLevel != null)
            {


                return activeBossDungeon.GetLevelIndex(ActiveLevel);
            }

            return 0;
        }
    }
    
    //Creates a new run and assigns run settings
    public Run(RunSettings runSettings, BossDungeon bossDungeon, Pet pet)
    {
        RunSettings = runSettings;
        activeBossDungeon = bossDungeon;
        SetPet(pet);
    }

    public Run()
    {
        RunSettings = null;
        activeBossDungeon = null;
        ActivePet = null;
    }

    public Run(RunSettings runSettings)
    {
        RunSettings = runSettings;
        activeBossDungeon = null;
        ActivePet = null;
    }

    public Run(BossDungeon bossDungeon)
    {
        RunSettings = null;
        activeBossDungeon = bossDungeon;
        ActivePet = null;
    }

    [Button]
    private void SetPet(Pet pet)
    {
        //pet.Passive.ScoreEvent.Invoke(new ScoreData(1));

        //Create copy
        ActivePet = ScriptableObject.Instantiate(pet);
        ActivePet.Activate();

    }

    private void AddToy(Toy toy)
    {
        //toy.Passive.ScoreEvent.Invoke(new ScoreData(1));
    }
    
     //Nothing in here should actually kick off any dynamic data
    //It should only set up run data objects
    public void Initialise()
    {
        ID = UnityEngine.Random.Range(0, 9999);
        
        //Initial level load
        //ActiveLevelSet = levelSet;
        ActiveLevel = new Level(activeBossDungeon.Levels[0]);
        
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

        //Can we initialise it with a capacity and seed chars?
        RackData = new RackData(activeBossDungeon.RackSize);
        RackData.Fill(activeBossDungeon.RackSeeds, activeBossDungeon.ScoringRubrik);
        
        //Set up appropriate event responses for the run
        VerifyResponses();
    }
    
    private void VerifyResponses()
    {
        //Scoring actions
        if (emptyRackListener == null)
        {
            emptyRackListener = (costsK, costsPips) => {
                bool validated = true;

                //If the active level has a live input don't allow new tiles
                if (ActiveLevel.inputs.Count > 0 && ActiveLevel.InputInProgress() != null)
                {
                    Debug.LogWarningFormat($"Cannot empty rack when there is a live input");
                    validated = false;
                }

                //Did the request come with a score cost?
                else if (costsK)
                {
                    int kCost = activeBossDungeon.ScoringRubrik.emptyRackKCost;

                    //Do we have enough score to cover the cost?
                    if (Energy >= kCost)
                    {
                        //Take the score away
                        //ModifyScore(-kCost);
                        Energy -= kCost;
                    }
                    else
                    {
                        validated = false;
                    }
                }

                //If we haven't already failed validation and the request wants to remove pips
                if (validated && costsPips)
                {
                    int pipCost = activeBossDungeon.ScoringRubrik.emptyRackPipCost;

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
                    //Listener >> Gate >> Event    
                    BrainControl.Get().runManager.CurrentRun.RackData.Empty();
                }
            };
        }
        
        
        if (newRackListener == null)
        {
            newRackListener = (seedChars, fillTo, costsK, costsPips) =>
            {
                bool validated = true;

                //If the active level has a live input don't allow new tiles
                if (ActiveLevel.inputs.Count > 0 && ActiveLevel.InputInProgress() != null )
                {
                    Debug.LogWarningFormat($"Cannot generate new tiles when there is a live input");
                    validated = false;
                }
                
                //Did the request come with a score cost?
                else if (costsK)
                {
                    int kCost = activeBossDungeon.ScoringRubrik.newRackKCost;
                    
                    //Do we have enough score to cover the cost?
                    if (Energy >= kCost)
                    {
                        //Take the score away
                        //ModifyScore(-kCost);
                        Energy -= kCost;
                    }
                    else
                    {
                        validated = false;
                    }
                }

                //If we haven't already failed validation and the request wants to remove pips
                if (validated && costsPips)
                {
                    int pipCost = activeBossDungeon.ScoringRubrik.newRackPipCost;
                    
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
                    //Listener >> Gate >> Event    
                    RackData.GetNew(activeBossDungeon.ScoringRubrik);
                }
            };
        }
        
        if (fillRackListener == null)
        {
            fillRackListener = (fillTo, costsK, costsPips) =>
            {
                bool validated = true;
                
                //If the active level has a live input don't allow new tiles
                if (ActiveLevel.inputs.Count > 0 && ActiveLevel.InputInProgress() != null)
                {
                    Debug.LogWarningFormat($"Cannot generate new tiles when there is a live input");
                    validated = false;
                    return;
                }

                if (RackData.AtCapacity())
                {
                    validated = false;
                    return;
                }

                else if (costsK)
                {
                    int kCost = activeBossDungeon.ScoringRubrik.fillRackKCost;

                    if (Energy >= kCost)
                    {
                        //ModifyScore(-kCost);
                        Energy -= kCost;
                    }
                    else
                    {
                        validated = false;
                    }
                }

                if (validated && costsPips)
                {
                    int pipCost = activeBossDungeon.ScoringRubrik.fillRackPipCost;
                    
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
                    RackData.Fill(null, activeBossDungeon.ScoringRubrik);
                }
            };
        }

        if (getTileListener == null)
        {
            getTileListener = (baseLetter, costsK, costsPips) =>
            {
                bool validated = true;
                
                //If the active level has a live input don't allow new tiles
                if (ActiveLevel.inputs.Count > 0 && ActiveLevel.InputInProgress() != null)
                {
                    Debug.LogWarningFormat($"Cannot generate new tiles when there is a live input");
                    validated = false;
                }

                else if (costsK)
                {
                    int kCost = activeBossDungeon.ScoringRubrik.specificTileKCost;

                    if (Energy >= kCost)
                    {
                        //ModifyScore(-kCost);
                        Energy -= kCost;
                    }
                    else
                    {
                        validated = false;
                    }
                }

                if (validated && costsPips)
                {
                    int pipCost = activeBossDungeon.ScoringRubrik.specificTilePipCost;
                    
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
                    BrainControl.Get().eventManager.e_getTile.Invoke(baseLetter, costsK, costsPips);
                }
            };
        }

        if (getConsonantListener == null)
        {
            getConsonantListener = (costsK, costsPips) =>
            {
                bool validated = true;

                if (RackData.AtCapacity())
                {
                    validated = false;
                }

                else
                {
                    //If the active level has a live input don't allow new tiles
                    if (ActiveLevel.inputs.Count > 0 && ActiveLevel.InputInProgress() != null)
                    {
                        Debug.LogWarningFormat($"Cannot generate new tiles when there is a live input");
                        validated = false;
                    }

                    else if (costsK)
                    {
                        int kCost = activeBossDungeon.ScoringRubrik.consonantKCost;

                        if (Energy >= kCost)
                        {
                            //ModifyScore(-kCost);
                            Energy -= kCost;
                        }
                        else
                        {
                            validated = false;
                        }
                    }

                    if (validated && costsPips)
                    {
                        int pipCost = activeBossDungeon.ScoringRubrik.consonantPipCost;

                        if (WorkingPips.Count >= pipCost)
                        {
                            RemovePips(pipCost);
                        }
                        else
                        {
                            validated = false;
                        }
                    }
                }
                if (validated)
                {
                    RackData.Add(activeBossDungeon.ScoringRubrik.Consonant());
                }
            };
        }

        if (getVowelListener == null)
        {
            getVowelListener = (costsK, costsPips) =>
            {
                bool validated = true;

                if (RackData.AtCapacity())
                {
                    validated = false;
                }

                else
                {
                    //If the active level has a live input don't allow new tiles
                    if (ActiveLevel.inputs.Count > 0 && ActiveLevel.InputInProgress() != null)
                    {
                        Debug.LogWarningFormat($"Cannot generate new tiles when there is a live input");
                        validated = false;
                    }

                    else if (costsK)
                    {
                        int kCost = activeBossDungeon.ScoringRubrik.vowelKCost;

                        if (Energy >= kCost)
                        {
                            //ModifyScore(-kCost);
                            Energy -= kCost;
                        }
                        else
                        {
                            validated = false;
                        }
                    }

                    if (validated && costsPips)
                    {
                        int pipCost = activeBossDungeon.ScoringRubrik.vowelPipCost;

                        if (WorkingPips.Count >= pipCost)
                        {
                            RemovePips(pipCost);
                        }
                        else
                        {
                            validated = false;
                        }
                    }
                }

                if (validated)
                {
                    RackData.Add(activeBossDungeon.ScoringRubrik.Vowel());
                    // BrainControl.Get().eventManager.e_getVowelRequest.Invoke(costsScore, costsPips);
                }
            };
        }

        //At this point we know we have a legit block input with a scoreable word

        //So we award a score based on the letters
        //And we award spendable K to spend on //abilities //new letters
        //Should K be based on base score - or modified score?
        if (validateSuccessListener == null)
        {
            validateSuccessListener = (blockInput) =>
            {
                ScoreData newScoreData = new ScoreData(0);
                
                foreach (BlockLine blockLine in blockInput.ValidatedLines)
                {
                    string validated = GridTools.WordFromLine(blockLine).forwards;
                
                    // Debug.Log("Compiled word set: " + GridTools.WordFromLine(b));
                
                    //Figure out how much the compiled block line is worth
                    int scoreAdd = activeBossDungeon.ScoringRubrik.ScoreFromBlocks(blockLine);
                    
                    //Debug.Log("Compiled word set is worth: " + scoreAdd);

                    newScoreData.Amount += scoreAdd;
                    
                    recentWords.Add(validated);
                    
                    //Truncate our list
                    if (recentWords.Count >= RunSettings.recentWordBias)
                    {
                        recentWords.RemoveAt(recentWords.Count - 1);
                    }
                }

                //Modify the score in order
                //Passive
                //Each collected toy
                if (ActivePet != null)
                {
                    //Passive Effects
                    ActivePet.Passive.ScoreEvent.Invoke(newScoreData);
                }

                //Toy Effects
                foreach (Toy toy in m_toys)
                {
                    toy.Passive.ScoreEvent.Invoke(newScoreData);
                }

                //Add the score data to the current score
                ModifyScore(newScoreData.Amount);

                //As a test we add score divided by two as energy
                Energy += newScoreData.Amount / 2;

                BrainControl.Get().Grid.CheckForCompletion(blockInput);
            };
        }
        
        if (validateFailListener == null)
        {
            validateFailListener = () =>
            {
                Debug.Log("Validation failed");
                ModifyScore(-activeBossDungeon.ScoringRubrik.validateFailPenalty);
            };
        }
        
        if (levelSuccessListener == null)
        {
            levelSuccessListener = (Level level) =>
            {
                //Add score depending on words
                ModifyScore(activeBossDungeon.ScoringRubrik.LevelSuccessAward);
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
    }
    
    //Add or remove a score value from the run
    private void ModifyScore(int modification)
    {
        Debug.Log($"Modifying the score by {modification}");
        
        //Modify the score
        Score = (int)Mathf.Clamp((float)Score + (float)modification, 0, Mathf.Infinity);
        
        AddWorkingTime(modification/4);
        
        //Reflect it in the UI
        BrainControl.Get().eventManager.e_updateUI.Invoke();
    }
}

