using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class IndexableCanvasgroup
{
    public string Key = "";
    public CanvasGroup Value = null;
}

public class UIManager : MonoBehaviour
{
    public Canvas MainMenuCanvas = null;
   
    
    public Canvas GameCanvas = null;

    //Holders, panels, canvasses
    [SerializeField]
    private List<IndexableCanvasgroup> m_sourcePanelIndex = null;
    private Dictionary<string, CanvasGroup> m_panelIndex = new Dictionary<string, CanvasGroup>();
    
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
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI messageBox;
    ////////////////////

    //UI Element spawning
    ////////////////////
    
    public InputModificationWidget inputControlTemplate;
    private InputModificationWidget inputControlInstance;

    [SerializeField]
    private float m_bagButtonRadius = 5;

    [SerializeField]
    private float m_endAngleDeg = 45;

    [SerializeField]
    private float m_startAngleDeg = 0;
    
    
    
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

    //Tweens
    private int m_printMessageTween = -99;
    private int m_clearMessageTween = -98;
    private int m_bagButtonShowTween = -97;
    private int m_bagButtonSizeTween = -96;
    
    [SerializeField]
    private Transform m_tileHolder;
    private LetterTile m_tileTemplate;
    private SerializableDictionary<Letter, LetterTile> LetterTiles = new SerializableDictionary<Letter, LetterTile>();
    
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

        scoreText.text = Brain.ins.runManager.CurrentRun.Score.ToString();
        energyText.text = Brain.ins.runManager.CurrentRun.Energy.ToString();
        
        SetLevelText(Brain.ins.runManager.CurrentRun.ActiveLevelIndex);
        
        //This is apparently being called on a run that hasn't initialised them yet
        TimeBar.fillAmount = currentRun.WorkingTime / currentRun.MaxTime;
    }

    private void FetchPanels()
    {
        m_panelIndex = new();

        foreach (var entry in m_sourcePanelIndex)
        {
            m_panelIndex.Add(entry.Key, entry.Value);
        }

        foreach (var entry in m_panelIndex)
        {
            Debug.LogFormat($"{entry.Key}:{entry.Value}");
        }
    }


    void Start()
    {
        BrainControl.Get().eventManager.e_updateUI.AddListener(UpdateUI);

        BrainControl.Get().eventManager.e_blockSelected.AddListener(ShowInputWidget);
        
        BrainControl.Get().eventManager.e_beginInput.AddListener(ShowInputWidget);
        
        //This flow aint great
        BrainControl.Get().eventManager.e_validateSuccess.AddListener((s) =>
        {
            foreach (var t in s.ValidatedStrings)
            {
                PrintRecentWord(t + " " + BrainControl.Get().runManager.CurrentRun.activeBossDungeon.ScoringRubrik.ScoreFromBlocks(s.ValidatedLines[0]));
            }
        });
        
        //LEVEL scope UI
        BrainControl.Get().eventManager.e_levelLoaded.AddListener((level) =>
        {
            InitialiseLevelUI(level);
        });
        
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

            FetchPanels();
            InitialiseRunUI(run);
        });
        
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

        //Game is initialised everytime the main menu is loaded
        BrainControl.Get().eventManager.e_gameInitialised.AddListener(FetchPanels);
        BrainControl.Get().eventManager.e_gameInitialised.AddListener(() => ShowHideRunInfo(false));
        
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
            GameCanvas.worldCamera = Camera.main.transform.GetChild(0).GetComponent<Camera>();
            GameCanvas.planeDistance = 3;
        }
        
        if (MainMenuCanvas != null && MainMenuCanvas.worldCamera == null)
        {
            MainMenuCanvas.worldCamera = Camera.main.transform.GetChild(0).GetComponent<Camera>();
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
        foreach (LevelData level in run.activeBossDungeon.Levels)
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

        if (m_panelIndex.TryGetValue("timepanel", out CanvasGroup timerGroup))
        {
            //The timer group alpha is based on if the level is timed or not
            timerGroup.alpha = run.activeBossDungeon.IsTimed ? 1 : 0;
        }
        else
        {
            Debug.LogWarningFormat($"'timepanel' could not be found in the panel index");
        }
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

        if (m_panelIndex.TryGetValue("levelrequirementspanel", out CanvasGroup levelRequirementsPanel))
        {
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
                    newLevelRequirementGrub.transform.SetParent(levelRequirementsPanel.transform);

                    newLevelRequirementGrub.transform.localPosition = Vector3.zero;
                    newLevelRequirementGrub.transform.localRotation = quaternion.identity;
                    newLevelRequirementGrub.transform.localScale = Vector3.one;

                    newLevelRequirementGrub.GetComponentInChildren<TextMeshProUGUI>().text = flag.ToString();

                    //Populate the list
                    LevelRequirementGrubs.Add(flag, newLevelRequirementGrub);
                    Debug.LogFormat($"Added {flag} grub to the UI manager");
                }
            }

            //This should always fail but it initialises the challenge grubs
            //But its circular - it wall call back to this
            BrainControl.Get().Grid.ValidateChallenges();

            //Only show level requirements if there are some
            levelRequirementsPanel.alpha = level.Data.LevelRequirements == LevelRequirements.None ? 0 : 1;
        }
        else
        {
            Debug.LogWarningFormat($"'levelrequirementspanel' could not be found in the panel index");
        }

        if (m_panelIndex.TryGetValue("tilerequestpanel", out CanvasGroup tileRequestPanel))
        {
            //Tile request buttons disabled if the level does not allow them
            tileRequestPanel.alpha = level.Data.AllowTileRequests ? 1 : 0;
            tileRequestPanel.interactable = level.Data.AllowTileRequests;
        }
        else
        {
            Debug.LogWarningFormat($"'tilerequestpanel' could not be found in the panel index");
        }
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





    //Get one of the retrievable panels
    //If it doesn't exist, return null
    public CanvasGroup GetPanelByID(string id)
    {
        CanvasGroup panel = null;
        m_panelIndex.TryGetValue(id, out panel);

        return panel != null ? panel : null;
    }

    //Control everything via their canvasgroup?
    public void ShowHidePanelGeneric(CanvasGroup panel, bool newState)
    {
        int integerState = newState ? 1 : 0;

        panel.alpha = integerState;
        panel.interactable = newState;
        panel.blocksRaycasts = newState;
    }

    //Do I need to pass context here?
    //I need to get references to pet info etc
    //So its not just run info I need

    //And the pet info changes

    //New dialogue with required context?
    //Some run setup class that loads all possible options?

    //Data manager that provides retrievable data?
    public void ShowHideRunInfo(bool newState)
    {
        if (m_panelIndex.TryGetValue("mainmenuruninfopanel", out CanvasGroup runInfoPanel))
        {
            int integerState = newState ? 1 : 0;

            LeanTween.value(0, 1, 0.5f).setOnComplete(() => {
                runInfoPanel.alpha = integerState;
                runInfoPanel.interactable = newState;
                runInfoPanel.blocksRaycasts = newState;
            });
        }
    }

    public void ShowHideBagPanel(bool showHide)
    {
        LeanTween.cancel(m_bagButtonShowTween);
        LeanTween.cancel(m_bagButtonSizeTween);

        string panelID = "bagbuttonpanel";
        CanvasGroup targetPanel = null;
        m_panelIndex.TryGetValue(panelID, out targetPanel);

        if (targetPanel == null)
        {
            Debug.LogErrorFormat($"Panel ID:{panelID} could not be found in the panel index");
            return;
        }

        m_bagButtonSizeTween = LeanTween.value(1, 1.5f, 0.055f).setEase(LeanTweenType.easeInQuad).setLoopPingPong(1)
            .setOnUpdate((float val) => {
                for (int i = 0; i < targetPanel.transform.childCount; i++)
                {
                    targetPanel.transform.GetChild(i).GetComponent<RectTransform>().localScale = Vector3.one * val;
                }
            })
            .setOnComplete(() => {
                targetPanel.transform.localScale = Vector3.one;
            }).id;

        if (showHide)
        {
            targetPanel.interactable = true;

            //Get circle points
            List<Vector2> points = new List<Vector2>();

            // float endAngleDeg = 0;
            // float startAngleDeg = 45;

            Vector2 center = Input.mousePosition;

            float step = (m_endAngleDeg - m_startAngleDeg) / (targetPanel.transform.childCount - 1);

            for (int i = 0; i < targetPanel.transform.childCount; i++)
            {
                float angle = m_startAngleDeg + step * i;
                float rad = angle * Mathf.Deg2Rad;

                float x = center.x + m_bagButtonRadius * Mathf.Cos(rad);
                float y = center.y + m_bagButtonRadius * Mathf.Sin(rad);

                targetPanel.transform.GetChild(i).GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
            }

            m_bagButtonShowTween = LeanTween.value(targetPanel.alpha, 1, 0.2f).setOnUpdate((float val) => {
                    targetPanel.alpha = val;
                })
                .setOnComplete(() => {
                    targetPanel.alpha = 1;
                }).id;
        }
        else
        {
            targetPanel.interactable = false;

            m_bagButtonShowTween = LeanTween.value(targetPanel.alpha, 0, 0.2f).setOnUpdate((float val) => {
                    targetPanel.alpha = val;
                })
                .setOnComplete(() => {
                    targetPanel.alpha = 0;
                }).id;
        }
    }
}

