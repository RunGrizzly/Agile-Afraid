using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Canvas MainMenuCanvas = null;
    public Canvas GameCanvas = null;

    public CanvasGroup TimerGroup = null;
    public CanvasGroup TileRackGroup = null;
    public CanvasGroup TileRequestGroup = null;
    public CanvasGroup LevelRequirementsGroup = null;
    public CanvasGroup LevelTrackGroup = null;
    public CanvasGroup RecentWordGroup = null;
    
    //Holders, panels, canvasses
    ////////////////////
    // public Transform gameCanvas;
    public Transform RecentWordHolder;
    public Transform LevelPipHolder;
    // public Transform levelTrack;
    public Image TimeBar;
    public Transform TimePipHolder;
    ////////////////////

    //Text Box elements
    ////////////////////
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI messageBox;
    ////////////////////

    //UI Element spawning
    ////////////////////
    
    public InputModificationWidget inputControlTemplate;
    private InputModificationWidget inputControlInstance;
    
    [SerializeField]
    private InterruptPanel m_interruptPanelTemplate;
    
    private InterruptPanel interruptPanelInstance;
    
    public GameObject recentWordTemplate;
    public GameObject levelPipTemplate;

    public TimePipWidget TimePipWidgetTemplate = null;

    //Should I be spinning out a "widget" encapsulation? - probably
    public Image LevelRequirementGrubTemplate = null;
    ////////////////////

    public SerializableDictionary<LevelRequirements, Image> LevelRequirementGrubs = new SerializableDictionary<LevelRequirements, Image>();

    private int m_printMessageTween = -99;
    private int m_clearMessageTween = -99;
    
    [SerializeField]
    private Transform m_tileHolder;
    private LetterTile m_tileTemplate;
    private SerializableDictionary<Letter, LetterTile> LetterTiles = new SerializableDictionary<Letter, LetterTile>();

    // public void OnLetterAdded(Letter newLetter)
    // {
    //     //Add a new blank tile 
    //     //Should the actual instantiantation take place on the UI?
    //     LetterTile newTile = GameObject.Instantiate(m_tileTemplate, Vector3.zero, Quaternion.identity);
    //     newTile.transform.SetParent(m_tileHolder);
    //     newTile.transform.SetAsLastSibling();
    //     newTile.transform.localScale = Vector3.one;
    //     newTile.transform.localEulerAngles = Vector3.zero;
    //     newTile.transform.localPosition = Vector3.zero;
    //     
    //     //Here we build on the tile itself
    //     newTile.Build(newLetter);     
    //     
    //     LetterTiles.Add(newLetter,newTile);
    // }
    //
    // public void OnLetterRemoved(Letter letter)
    // {
    //     LetterTile targetLetter = null;
    //     
    //     if (LetterTiles.TryGetValue(letter, out targetLetter) && targetLetter != null)
    //     {
    //         LetterTiles.Remove(letter);
    //         Destroy(targetLetter.gameObject);
    //     }
    // }
    
    void SetLevelText(int level)
    {
        levelText.text = (level + 1).ToString();
    }

    //This is cancer
    void UpdateUI()
    {
        //Max time
        var currentRun = Brain.ins.runManager.CurrentRun;

        if (currentRun == null)
        {
            return;
        }
        
        scoreText.text = (Brain.ins.runManager.CurrentRun.Score).ToString();
        SetLevelText(Brain.ins.runManager.CurrentRun.ActiveLevelIndex);
        
        //This is apparently being called on a run that hasn't initialised them yet
        TimeBar.fillAmount = currentRun.WorkingTime / currentRun.MaxTime;
    }

    void Start()
    {
        // BrainControl.Get().eventManager.e_addedToRack.AddListener(OnLetterAdded);
        //
        // BrainControl.Get().eventManager.e_removedFromRack.AddListener(OnLetterRemoved);
        
        BrainControl.Get().eventManager.e_updateUI.AddListener(UpdateUI);

        BrainControl.Get().eventManager.e_blockSelected.AddListener(ShowInputWidget);
        
        BrainControl.Get().eventManager.e_beginInput.AddListener(ShowInputWidget);
        
        //This flow aint great
        BrainControl.Get().eventManager.e_validateSuccess.AddListener((s) =>
        {
            foreach (var t in s.ValidatedStrings)
            {
                PrintRecentWord(t + " " + BrainControl.Get().runManager.CurrentRun.ActiveLevelSet.ScoringRubrik.ScoreFromBlocks(s.ValidatedLines[0]));
            }
        });
        //
        // BrainControl.Get().eventManager.e_endInput.AddListener(() =>
        // {
        //     ClearInputWidget();
        // });
        //
        // BrainControl.Get().eventManager.e_clearBlock.AddListener((c) =>
        // {
        //     ClearInputWidget();
        // });

        //LEVEL scope UI
        BrainControl.Get().eventManager.e_levelLoaded.AddListener((level) =>
        {
            InitialiseLevelUI(level);
        });
        
        // BrainControl.Get().eventManager.e_restartLevel.AddListener(() =>
        // {
        //   InitialiseLevelUI(BrainControl.Get().runManager.currentRun.ActiveLevel);
        // });
        
        BrainControl.Get().eventManager.e_levelSuccess.AddListener((level) =>
        {
            Debug.Log("PATH COMPLETE: UI Mangager");
            Debug.Log("Completed level was: " + level);
            if (inputControlInstance != null)
            {
                Destroy(inputControlInstance.gameObject);
            }
            
            LevelPipHolder.transform.GetChild(BrainControl.Get().runManager.CurrentRun.ActiveLevelIndex).GetComponent<Image>().color = Color.cyan;
            ClearLevelUI();
            // /UpdateUI();
        });

        ///////////////////////
        
        //RUN scope UI
        BrainControl.Get().eventManager.e_newRun.AddListener((Run run) =>
        {
            if (interruptPanelInstance != null)
            {
                interruptPanelInstance.KillPanel();
            }
            
            InitialiseRunUI(run);
        });
        
        // BrainControl.Get().eventManager.e_restartRun.AddListener(() =>
        // {
        //     if (interruptPanelInstance != null)
        //     {
        //         interruptPanelInstance.KillPanel();
        //     }
        //     
        //     InitialiseRunUI(BrainControl.Get().runManager.CurrentRun);
        // });
        
        
        BrainControl.Get().eventManager.e_pauseRun.AddListener(() =>
        {
            if (interruptPanelInstance != null)
            {
                interruptPanelInstance.KillPanel();
            }

            interruptPanelInstance = Instantiate(m_interruptPanelTemplate, Vector3.zero, Quaternion.identity);
            interruptPanelInstance.transform.SetParent(GameCanvas.transform, false);
            interruptPanelInstance.transform.localPosition = Vector3.zero;
            interruptPanelInstance.transform.localPosition = Vector3.one;
            interruptPanelInstance.BuildPause();
        });
        
        BrainControl.Get().eventManager.e_unpauseRun.AddListener(() =>
        {
            if (interruptPanelInstance != null)
            {
                interruptPanelInstance.KillPanel();
            }
        });
        
        BrainControl.Get().eventManager.e_failRun.AddListener((run) =>
        {
            if (inputControlInstance != null)
            {
                Destroy(inputControlInstance.gameObject);
            }
            
            if (interruptPanelInstance != null)
            {
                interruptPanelInstance.KillPanel();
            }

            interruptPanelInstance = Instantiate(m_interruptPanelTemplate, Vector3.zero, Quaternion.identity);
            interruptPanelInstance.transform.SetParent(GameCanvas.transform, false);
            interruptPanelInstance.transform.localPosition = Vector3.zero;
            interruptPanelInstance.transform.localPosition = Vector3.one;
            interruptPanelInstance.BuildFail(run);
        });

        BrainControl.Get().eventManager.e_winRun.AddListener((run) =>
        {
            if (inputControlInstance != null)
            {
                Destroy(inputControlInstance.gameObject);
            }
          
            if (interruptPanelInstance != null)
            {
                interruptPanelInstance.KillPanel();
            }

            interruptPanelInstance = Instantiate(m_interruptPanelTemplate, Vector3.zero, Quaternion.identity);
            interruptPanelInstance.transform.SetParent(GameCanvas.transform, false);
            interruptPanelInstance.transform.localPosition = Vector3.zero;
            interruptPanelInstance.transform.localPosition = Vector3.one;
            interruptPanelInstance.BuildWin(run);
        });
        ///////////////////////
        

        BrainControl.Get().eventManager.e_quitToMenu.AddListener(() =>
        {
            if (interruptPanelInstance != null)
            {
                interruptPanelInstance.KillPanel();
            }
            ClearRunUI();
            ClearInputWidget();
        });
        
        BrainControl.Get().eventManager.e_pipAdded.AddListener((TimePip newPip) =>
        {
            Debug.Log("Triggered new UI pip");
            TimePipWidget newPipWidget = Instantiate(TimePipWidgetTemplate, TimePipHolder);
            // newPipWidget.transform.SetAsLastSibling();
            newPipWidget.transform.localScale = new Vector3( newPip.MaxValue,1,1);
        
            newPip.Widget = newPipWidget;
        
            switch (newPip.TimePoolType)
            {
                case TimePoolType.Squeaky:
                    newPipWidget.PipFace.color = Color.red;
                    break;
                
                case TimePoolType.Core:
                    newPipWidget.PipFace.color = Color.green;
                    break;
                
                case TimePoolType.Grey:
                    newPipWidget.PipFace.color = Color.gray;
                    break;
            }
        });
    }

    public void FixedUpdate()
    {
        //If the man canvas exists and doesn't have a camera
        if (GameCanvas != null && GameCanvas.worldCamera == null)
        {
            GameCanvas.worldCamera = Camera.main;
            GameCanvas.planeDistance = 3;
        }
        
        if (MainMenuCanvas != null && MainMenuCanvas.worldCamera == null)
        {
            MainMenuCanvas.worldCamera = Camera.main;
            MainMenuCanvas.planeDistance = 3;
        }
    }

    private void ClearRunUI()
    {
        InitialiseRecentWords();
        
        //Clear old run timer elements elements
        foreach (Transform child in TimePipHolder.transform)
        {
            Destroy(child.gameObject);
        }
        
        //Clear level requirements grubs
        ClearLevelRequirementsGrubs();
        
        //Clear level pip
        foreach (Transform child in LevelPipHolder)
        {
            Destroy(child.gameObject);
        }
        
        ClearInputWidget();
    }

    private void InitialiseRunUI(Run run)
    {
        //Initialising automatically does a clear
        ClearRunUI();
        
        //Layout pip track for the levels in this run
        foreach (LevelData level in run.ActiveLevelSet.Levels)
        {
            GameObject newPip = Instantiate(levelPipTemplate, Vector3.zero, Quaternion.identity);
            newPip.transform.SetParent(LevelPipHolder);
            newPip.transform.localPosition = Vector3.zero;
            newPip.transform.localEulerAngles = Vector3.zero;
            newPip.transform.localScale = Vector3.one;
        }

        //We should really just pass the run
        foreach (var newPip in run.AllPips)
        {
            Debug.Log("Triggered new UI pip");
            TimePipWidget newPipWidget = Instantiate(TimePipWidgetTemplate, TimePipHolder);
            // newPipWidget.transform.SetAsLastSibling();
            newPipWidget.transform.localScale = new Vector3( newPip.MaxValue,1,1);

            newPip.Widget = newPipWidget;

            switch (newPip.TimePoolType)
            {
                case TimePoolType.Squeaky:
                    newPipWidget.PipFace.color = Color.red;
                    break;
                
                case TimePoolType.Core:
                    newPipWidget.PipFace.color = Color.green;
                    break;
                
                case TimePoolType.Grey:
                    newPipWidget.PipFace.color = Color.gray;
                    break;
            }    
        }
        
        //The timer group alpha is based on if the level is timed or not
        TimerGroup.alpha = run.ActiveLevelSet.IsTimed ? 1 : 0;
    }
    
    private void ClearLevelUI()
    { 
        //We want to show recent words over the course of a full run
      //InitialiseRecentWords();

      ClearLevelRequirementsGrubs();
      ClearInputWidget();
    }

    private void ClearLevelRequirementsGrubs()
    {
        Debug.LogFormat($"Clearing level requirements grubs");
        foreach (var entry in LevelRequirementGrubs)
        {
            Debug.LogFormat($"Trying to destroy {entry.Key} grub");
            Destroy(entry.Value.gameObject);
        }

        LevelRequirementGrubs = new SerializableDictionary<LevelRequirements, Image>();
    }
    
    private void InitialiseLevelUI(Level level)
    {
        ClearLevelUI();
        
        LevelPipHolder.GetChild(BrainControl.Get().runManager.CurrentRun.ActiveLevelIndex).GetComponent<Image>().color = Color.green;
        
        foreach (LevelRequirements flag in Enum.GetValues(typeof(LevelRequirements)))
        {
            //Don't count none
            if (flag == LevelRequirements.None)
            {
                continue;
            }

            if (level.Data.LevelRequirements.HasFlag(flag))
            {
                var newLevelRequirementGrub = Instantiate(LevelRequirementGrubTemplate);
                newLevelRequirementGrub.transform.SetParent(LevelRequirementsGroup.transform);

                newLevelRequirementGrub.transform.localPosition = Vector3.zero;
                newLevelRequirementGrub.transform.localRotation  = quaternion.identity;
                newLevelRequirementGrub.transform.localScale = Vector3.one;
                
                newLevelRequirementGrub.GetComponentInChildren<TextMeshProUGUI>().text = flag.ToString();
                
                //Populate the list
                LevelRequirementGrubs.Add(flag,newLevelRequirementGrub);
                Debug.LogFormat($"Added {flag} grub to the UI manager");
            }
        }

        //This should always fail but it initialises the challenge grubs
        //But its circular - it wall call back to this
        BrainControl.Get().Grid.ValidateChallenges();
        
        //Only show level requirements if there are some
        LevelRequirementsGroup.alpha = level.Data.LevelRequirements == LevelRequirements.None ? 0 : 1;
        
        //Tile request buttons disabled if the level does not allow them
        TileRequestGroup.alpha = level.Data.AllowTileRequests ? 1 : 0;
        TileRequestGroup.interactable = level.Data.AllowTileRequests;
    }
    
    private void ClearInputWidget()
    {
        if (inputControlInstance != null)
        {
            Destroy(inputControlInstance.gameObject);
        }
    }

    private void ShowInputWidget(LetterBlock block)
    {
        //Debug.LogFormat($"Showing input control widget");
        
        if (block.lockState == LockState.locked)
        {
            Debug.LogWarningFormat(block.gameObject, $"Input block is locked");
            return;
        }

        // if (block.fillState == FillState.empty)
        // {
        //     Debug.LogWarningFormat(block.gameObject, $"Input block is empty");
        //     return;
        // }
        
        if (inputControlInstance == null)
        {
            inputControlInstance = Instantiate(inputControlTemplate, block.transform.position, Quaternion.identity);
            inputControlInstance.AssignTarget(block);
            //Debug.LogFormat(inputControlInstance,$"New control widget was spawned");
        }
        
        //Ensure it faces up
        //inputControlInstance.transform.localEulerAngles = new Vector3(90, 0, 0);
        //Set Position
        // LeanTween.move(inputControlInstance, new Vector3(block.transform.position.x, 0.75f, block.transform.position.z), 0.35f).setEase(LeanTweenType.easeOutExpo);
    }

    private void InitialiseRecentWords()
    {
        // if (inputControlInstance != null)
        // {
        //     Destroy(inputControlInstance);
        // }
        
        //Reset the word lists
        foreach (Transform child in RecentWordHolder)
        {
            Destroy((child.gameObject));
        }
    }
    
    public void PrintMessage(string message)
    {
        LeanTween.cancel(m_printMessageTween);
        LeanTween.cancel(m_clearMessageTween);
        
        messageBox.text = message;
        
        var animatedTransform =   messageBox.GetComponent<RectTransform>();
        
        
        m_printMessageTween = LeanTween.value(messageBox.gameObject, -50f, 0, 0.35f).setEase(LeanTweenType.easeSpring).setOnUpdate((v) =>
        {
            animatedTransform.anchoredPosition = new Vector3(0, v, 0);
        }).id;
        
        m_clearMessageTween =LeanTween.delayedCall(0.4f, () =>
        {
            messageBox.text = "";
        }).id;
    }

    void PrintRecentWord(string recent)
    {
        if (RecentWordHolder.childCount >= BrainControl.Get().runManager.CurrentRun.RunSettings.recentWordBias)
        {
            Destroy(RecentWordHolder.GetChild(RecentWordHolder.childCount - 1).gameObject);
        }

        TextMeshProUGUI newRecent = Instantiate(recentWordTemplate, Vector3.zero, Quaternion.identity).GetComponentInChildren<TextMeshProUGUI>();

        newRecent.transform.SetParent(RecentWordHolder);
        newRecent.transform.SetAsFirstSibling();

        newRecent.text = recent;


        newRecent.transform.localPosition = Vector3.zero;
        newRecent.transform.localEulerAngles = Vector3.zero;
        newRecent.transform.localScale = Vector3.one;
    }


}

