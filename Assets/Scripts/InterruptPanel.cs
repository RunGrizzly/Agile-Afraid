using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InterruptPanel : MonoBehaviour
{

    public GameObject buttonTemplate;

    public Transform buttonList;

    public InterruptType interruptType;

    public TextMeshProUGUI panelTitle;
    public TextMeshProUGUI panelQuip;

    [SerializeField]
    private CanvasGroup m_runInfoPanel = null;
    
    [SerializeField]
    private CanvasGroup m_optionsPanel = null;
    
    [SerializeField]
    private TextMeshProUGUI m_scoreInfoText = null;
    
    [SerializeField]
    private TextMeshProUGUI m_levelInfoText = null;
    
    [SerializeField]
    private TextMeshProUGUI m_timeInfoText = null;


    public void BuildPause()
    {
        //We only show run info at game end
        m_runInfoPanel.alpha = 0;
        
        //Instead we show settings
        m_optionsPanel.alpha = 1;
        m_optionsPanel.interactable = true;
        
        panelTitle.text = "Paused";
        panelQuip.text = "This is a funny quip lol.";
        
        //Add Buttons
        /////////////////
        Button newButton;

        newButton = BuildButton("Resume");
        newButton.onClick.AddListener(() => { BrainControl.Get().runManager.CurrentRun.SetPaused(false); });

        newButton = BuildButton("Restart Run");
        newButton.onClick.AddListener(() => { BrainControl.Get().runManager.ResolveRun(Resolution.Restarted); });

        // newButton = BuildButton("Restart Level");
        // newButton.onClick.AddListener(() =>
        //  {
        //      BrainControl.Get().eventManager.e_restartLevel.Invoke();
        //      BrainControl.Get().runManager.currentRun.SetPaused(false);
        //  });

        newButton = BuildButton("Quit To Menu");
        newButton.onClick.AddListener(() => { BrainControl.Get().runManager.ResolveRun(Resolution.Aborted); });
        newButton.onClick.AddListener(() => { BrainControl.Get().eventManager.e_quitToMenu.Invoke(); });
        /////////////////

    }

    public void BuildWin(Run run)
    {
        //We only show run info at game end
        m_runInfoPanel.alpha = 1;
        
        m_scoreInfoText.text = string.Format($"{run.Score}");
        m_levelInfoText.text = string.Format($"{run.ActiveLevelIndex+1}/{run.ActiveLevelSet.Levels.Count}");
        m_timeInfoText.text = string.Format($"{run.Elapsed}");
        
        m_optionsPanel.alpha = 0;
        m_optionsPanel.interactable = false;
        
        panelTitle.text = "You Won";
        panelQuip.text = "GG WP";

        Button newButton;

        newButton = BuildButton("Restart");
        newButton.onClick.AddListener(() => { BrainControl.Get().runManager.ResolveRun(Resolution.Restarted); });

        newButton = BuildButton("Quit To Menu");
        newButton.onClick.AddListener(() => { BrainControl.Get().runManager.ResolveRun(Resolution.Aborted); });
        newButton.onClick.AddListener(() => { BrainControl.Get().eventManager.e_quitToMenu.Invoke(); });
    }

    public void BuildFail(Run run)
    {
        //We only show run info at game end
        m_runInfoPanel.alpha = 1;

        m_scoreInfoText.text = string.Format($"{run.Score}");
        m_levelInfoText.text = string.Format($"{run.ActiveLevelIndex+1}/{run.ActiveLevelSet.Levels.Count}");
        m_timeInfoText.text = string.Format($"{run.Elapsed}");
        
        m_optionsPanel.alpha = 0;
        m_optionsPanel.interactable = false;
        
        panelTitle.text = "You Lost";
        panelQuip.text = "Actual sad face";

        Button newButton;

        newButton = BuildButton("Restart");
        newButton.onClick.AddListener(() => { BrainControl.Get().runManager.ResolveRun(Resolution.Restarted); });

        newButton = BuildButton("Quit To Menu");
        newButton.onClick.AddListener(() => { BrainControl.Get().runManager.ResolveRun(Resolution.Aborted); });
        newButton.onClick.AddListener(() => { BrainControl.Get().eventManager.e_quitToMenu.Invoke(); });
    }

    public Button BuildButton(string label)
    {
        Button newButton = Instantiate(buttonTemplate, Vector3.zero, Quaternion.identity).GetComponent<Button>();
        newButton.transform.SetParent(buttonList);
        newButton.transform.localPosition = Vector3.zero;
        newButton.transform.localEulerAngles = Vector3.zero;
        newButton.transform.localScale = Vector3.one;
        newButton.GetComponentInChildren<TextMeshProUGUI>().text = label;
        return newButton;
    }

    public void KillPanel()
    {
        Destroy(gameObject);
    }

}
