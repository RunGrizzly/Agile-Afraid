using System;
using System.Collections;
using System.Collections.Generic;
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
    
    //Holders, panels, canvasses
    ////////////////////
    public Transform gameCanvas;
    public Transform recentHolder;
    public Transform levelTrack;
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
    
    public GameObject inputControlTemplate;
    GameObject inputControlInstance;
    
    [SerializeField]
    private InterruptPanel m_interruptPanelTemplate;
    
    private InterruptPanel interruptPanelInstance;
    
    public GameObject recentWordTemplate;
    public GameObject levelPipTemplate;

    public TimePipWidget TimePipWidgetTemplate = null;

    //Should I be spinning out a "widget" encapsulation? - probably
    public Image LevelRequirementGrubTemplate = null;
    ////////////////////

    public Dictionary<LevelRequirements, Image> LevelRequirementGrubs = new Dictionary<LevelRequirements, Image>();
    
    void SetLevelText(int level)
    {
        levelText.text = (level + 1).ToString();
    }

    //This is cancer
    void UpdateUI()
    {
        scoreText.text = (Brain.ins.runManager.CurrentRun.Score).ToString();
        SetLevelText(Brain.ins.runManager.CurrentRun.ActiveLevelIndex);
        
        // //Max time
        var currentRun = Brain.ins.runManager.CurrentRun;
        
        TimeBar.fillAmount = currentRun.WorkingTime / currentRun.MaxTime;
    }

    void Start()
    {
        BrainControl.Get().eventManager.e_updateUI.AddListener(() =>
        {
            UpdateUI();
        });

        BrainControl.Get().eventManager.e_blockSelected.AddListener((block) =>
         {
             ShowInputWidget(block);
         });

        BrainControl.Get().eventManager.e_beginInput.AddListener((block) =>
        {
            ShowInputWidget(block);
        });

        BrainControl.Get().eventManager.e_updateInput.AddListener((block) =>
        {
            ShowInputWidget(block);
        });

        BrainControl.Get().eventManager.e_validateSuccess.AddListener((s) =>
        {
            foreach (var t in s.ValidatedLines)
            {
                PrintRecentWord((GridTools.WordFromLine(t).forwards) + " " + BrainControl.Get().runManager.CurrentRun.RunSettings.ActiveScoringRubrik.ScoreFromBlocks(t));
            }
        });

        BrainControl.Get().eventManager.e_endInput.AddListener(() =>
        {
            ClearInputWidget();
        });

        BrainControl.Get().eventManager.e_clearBlock.AddListener((c) =>
        {
            ClearInputWidget();
        });

        //LEVEL scope UI
        BrainControl.Get().eventManager.e_levelLoaded.AddListener((level) =>
        {
            InitialiseLevelUI(level);
        });
        
        // BrainControl.Get().eventManager.e_restartLevel.AddListener(() =>
        // {
        //   InitialiseLevelUI(BrainControl.Get().runManager.currentRun.ActiveLevel);
        // });
        
        BrainControl.Get().eventManager.e_levelSuccess.AddListener((l) =>
        {
            Debug.Log("PATH COMPLETE: UI Mangager");
            Debug.Log("Completed level was: " + l);
            levelTrack.GetChild(BrainControl.Get().runManager.CurrentRun.ActiveLevelIndex).GetComponent<Image>().color = Color.cyan;
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
            interruptPanelInstance.transform.SetParent(gameCanvas.transform, false);
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
        
        BrainControl.Get().eventManager.e_failRun.AddListener(() =>
        {
            if (interruptPanelInstance != null)
            {
                interruptPanelInstance.KillPanel();
            }

            interruptPanelInstance = Instantiate(m_interruptPanelTemplate, Vector3.zero, Quaternion.identity);
            interruptPanelInstance.transform.SetParent(gameCanvas.transform, false);
            interruptPanelInstance.transform.localPosition = Vector3.zero;
            interruptPanelInstance.transform.localPosition = Vector3.one;
            interruptPanelInstance.BuildFail();
        });

        BrainControl.Get().eventManager.e_winRun.AddListener(() =>
        {
            if (interruptPanelInstance != null)
            {
                interruptPanelInstance.KillPanel();
            }

            interruptPanelInstance = Instantiate(m_interruptPanelTemplate, Vector3.zero, Quaternion.identity);
            interruptPanelInstance.transform.SetParent(gameCanvas.transform, false);
            interruptPanelInstance.transform.localPosition = Vector3.zero;
            interruptPanelInstance.transform.localPosition = Vector3.one;
            interruptPanelInstance.BuildWin();
        });
        ///////////////////////
        

        BrainControl.Get().eventManager.e_quitToMenu.AddListener(() =>
        {
            if (interruptPanelInstance != null)
            {
                interruptPanelInstance.KillPanel();
            }
            
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
        foreach (Transform child in LevelRequirementsGroup.transform)
        {
            Destroy(child.gameObject);
        }
        
        //Clear level pip
        foreach (Transform child in levelTrack.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void InitialiseRunUI(Run run)
    {
        //Initialising automatically does a clear
        ClearRunUI();
        
        //Layout pip track for the levels in this run
        foreach (LevelData level in run.ActiveLevelSet.Levels)
        {
            GameObject newPip = Instantiate(levelPipTemplate, Vector3.zero, Quaternion.identity);
            newPip.transform.SetParent(levelTrack);
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
    }
    
    private void ClearLevelUI()
    { 
        //We want to show recent words over the course of a full run
      //InitialiseRecentWords();

      ClearLevelRequirementsGrubs();
    }

    private void ClearLevelRequirementsGrubs()
    {
        foreach (var entry in LevelRequirementGrubs)
        {
            Debug.LogFormat($"Trying to destroy {entry.Key} grub");
            Destroy(entry.Value.gameObject);
        }

        LevelRequirementGrubs = new Dictionary<LevelRequirements, Image>();
    }
    
    private void InitialiseLevelUI(Level level)
    {
        ClearLevelUI();
        
        levelTrack.GetChild(BrainControl.Get().runManager.CurrentRun.ActiveLevelIndex).GetComponent<Image>().color = Color.green;
        
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
        BrainControl.Get().grid.ValidateChallenges();
        
        //Only show level requirements if there are some
        LevelRequirementsGroup.alpha = level.Data.LevelRequirements == LevelRequirements.None ? 0 : 1;
        
        //The timer group alpha is based on if the level is timed or not
        TimerGroup.alpha = level.Data.IsTimed ? 1 : 0;
        
        //Tile request buttons disabled if the level does not allow them
        TileRequestGroup.alpha = level.Data.AllowTileRequests ? 1 : 0;
        TileRequestGroup.interactable = level.Data.AllowTileRequests;
    }
    
    private void ClearInputWidget()
    {
        if (inputControlInstance != null)
        {
            Destroy(inputControlInstance);
        }
    }

    private void ShowInputWidget(LetterBlock block)
    {
        if (block.lockState == LockState.locked) return;
        if (block.fillState == FillState.empty) return;
        
        if (inputControlInstance == null)
        {
            inputControlInstance = Instantiate(inputControlTemplate, block.transform.position, Quaternion.identity);
        }
            
        //Ensure it faces up
        inputControlInstance.transform.localEulerAngles = new Vector3(90, 0, 0);
        //Set Position
        LeanTween.move(inputControlInstance, new Vector3(block.transform.position.x, 0.75f, block.transform.position.z), 0.35f).setEase(LeanTweenType.easeOutExpo);
    }

    private void InitialiseRecentWords()
    {
        if (inputControlInstance != null)
        {
            Destroy(inputControlInstance);
        }
        
        //Reset the word lists
        foreach (Transform child in recentHolder)
        {
            Destroy((child.gameObject));
        }
    }
    
    public void PrintMessage(string message)
    {
        messageBox.text = message;

        LeanTween.delayedCall(3f, () => LeanTween.value(messageBox.gameObject, 0, -50f, 0.35f).setEase(LeanTweenType.easeOutExpo).setOnUpdate((v) => messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, v, 0)));

        LeanTween.value(messageBox.gameObject, -50f, 0, 0.35f).setEase(LeanTweenType.easeOutExpo).setOnUpdate((v) => messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, v, 0));
    }

    void PrintRecentWord(string recent)
    {
        if (recentHolder.childCount >= BrainControl.Get().runManager.CurrentRun.RunSettings.recentWordBias)
        {
            Destroy(recentHolder.GetChild(recentHolder.childCount - 1).gameObject);
        }

        TextMeshProUGUI newRecent = Instantiate(recentWordTemplate, Vector3.zero, Quaternion.identity).GetComponentInChildren<TextMeshProUGUI>();

        newRecent.transform.SetParent(recentHolder);
        newRecent.transform.SetAsFirstSibling();

        newRecent.text = recent;


        newRecent.transform.localPosition = Vector3.zero;
        newRecent.transform.localEulerAngles = Vector3.zero;
        newRecent.transform.localScale = Vector3.one;
    }


}

