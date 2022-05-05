using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{

    //Holders, panels, canvasses
    ////////////////////
    public Transform gameCanvas;
    public Transform recentHolder;
    public Transform levelTrack;
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
    public GameObject interruptPanelTemplate;
    InterruptPanel interruptPanelInstance;
    public GameObject recentWordTemplate;
    public GameObject levelPipTemplate;
    ////////////////////

    void SetLevelText(int level)
    {
        levelText.text = (level + 1).ToString();
    }

    void Start()
    {


        BrainControl.Get().eventManager.e_updateUI.AddListener(() =>
        {
            scoreText.text = (Brain.ins.sessionManager.currentSession.score).ToString();
            SetLevelText(Brain.ins.sessionManager.currentSession.level);
        });

        BrainControl.Get().eventManager.e_blockSelected.AddListener((b) =>
         {
             if (b.lockState == LockState.locked) return;
             if (b.fillState == FillState.empty) return;

             if (inputControlInstance == null) inputControlInstance = Instantiate(inputControlTemplate, b.transform.position, Quaternion.identity);
             //inputControlCanvas.transform.SetParent(b.transform);
             //Ensure it faces up
             inputControlInstance.transform.localEulerAngles = new Vector3(90, 0, 0);
             //Set Position
             LeanTween.move(inputControlInstance, new Vector3(b.transform.position.x, 0.75f, b.transform.position.z), 0.35f).setEase(LeanTweenType.easeOutExpo);
         });

        BrainControl.Get().eventManager.e_beginInput.AddListener((b) =>
        {
            if (inputControlInstance == null) inputControlInstance = Instantiate(inputControlTemplate, b.transform.position, Quaternion.identity);
            //Ensure it faces up
            inputControlInstance.transform.localEulerAngles = new Vector3(90, 0, 0);
            //Set Position
            LeanTween.move(inputControlInstance, new Vector3(b.transform.position.x, 0.75f, b.transform.position.z), 0.35f).setEase(LeanTweenType.easeOutExpo);
        });

        BrainControl.Get().eventManager.e_updateInput.AddListener((u) =>
        {
            if (inputControlInstance == null) inputControlInstance = Instantiate(inputControlTemplate, u.transform.position, Quaternion.identity);

            //Ensure it faces up
            inputControlInstance.transform.localEulerAngles = new Vector3(90, 0, 0);
            //Set Position
            LeanTween.move(inputControlInstance, new Vector3(u.transform.position.x, 0.75f, u.transform.position.z), 0.35f).setEase(LeanTweenType.easeOutExpo);
        });

        BrainControl.Get().eventManager.e_validateSuccess.AddListener((s) =>
        {
            foreach (var t in s.compiledWords)
            {
                PrintRecentWord((GridTools.WordFromLine(t).forwards) + " " + BrainControl.Get().scoreManager.
                ScoreFromBlocks(t));
            }
        });

        BrainControl.Get().eventManager.e_endInput.AddListener(() =>
        {
            if (inputControlInstance != null) Destroy(inputControlInstance);
        });

        BrainControl.Get().eventManager.e_clearBlock.AddListener((c) =>
        {
            if (inputControlInstance != null) Destroy(inputControlInstance);
        });

        BrainControl.Get().eventManager.e_newSession.AddListener((s) =>
       {
           if (inputControlInstance != null) Destroy(inputControlInstance);
           foreach (LevelData level in s.levels)
           {
               GameObject newPip = Instantiate(levelPipTemplate, Vector3.zero, Quaternion.identity);
               newPip.transform.SetParent(levelTrack);
               newPip.transform.localPosition = Vector3.zero;
               newPip.transform.localEulerAngles = Vector3.zero;
               newPip.transform.localScale = Vector3.one;
           }
       });

        // BrainControl.Get().eventManager.e_restartSession.AddListener(() =>
        // {
        //     if (inputControlInstance != null) Destroy(inputControlInstance);
        // });

        BrainControl.Get().eventManager.e_restartLevel.AddListener(() =>
        {
            if (inputControlInstance != null) Destroy(inputControlInstance);
        });

        BrainControl.Get().eventManager.e_quitToMenu.AddListener(() =>
        {
            if (inputControlInstance != null) Destroy(inputControlInstance);
        });

        BrainControl.Get().eventManager.e_pauseSession.AddListener(() =>
        {

            if (interruptPanelInstance != null) interruptPanelInstance.KillPanel();

            interruptPanelInstance = Instantiate(interruptPanelTemplate, Vector3.zero, Quaternion.identity).GetComponent<InterruptPanel>();
            interruptPanelInstance.transform.SetParent(gameCanvas.transform, false);
            interruptPanelInstance.transform.localPosition = Vector3.zero;
            interruptPanelInstance.BuildPause();

        });

        BrainControl.Get().eventManager.e_failSession.AddListener(() =>
        {

            if (interruptPanelInstance != null) interruptPanelInstance.KillPanel();

            interruptPanelInstance = Instantiate(interruptPanelTemplate, Vector3.zero, Quaternion.identity).GetComponent<InterruptPanel>();
            interruptPanelInstance.transform.SetParent(gameCanvas.transform, false);
            interruptPanelInstance.transform.localPosition = Vector3.zero;
            interruptPanelInstance.BuildFail();
        });

        BrainControl.Get().eventManager.e_winSession.AddListener(() =>
        {
            if (interruptPanelInstance != null) interruptPanelInstance.KillPanel();

            interruptPanelInstance = Instantiate(interruptPanelTemplate, Vector3.zero, Quaternion.identity).GetComponent<InterruptPanel>();
            interruptPanelInstance.transform.SetParent(gameCanvas.transform, false);
            interruptPanelInstance.transform.localPosition = Vector3.zero;
            interruptPanelInstance.BuildWin();
        });

        BrainControl.Get().eventManager.e_unpauseSession.AddListener(() =>
        {
            if (interruptPanelInstance != null) interruptPanelInstance.KillPanel();
        });
    }

    public void PrintMessage(string message)
    {
        messageBox.text = message;

        LeanTween.delayedCall(3f, () => LeanTween.value(messageBox.gameObject, 0, -50f, 0.35f).setEase(LeanTweenType.easeOutExpo).setOnUpdate((v) => messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, v, 0)));

        LeanTween.value(messageBox.gameObject, -50f, 0, 0.35f).setEase(LeanTweenType.easeOutExpo).setOnUpdate((v) => messageBox.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, v, 0));
    }

    void PrintRecentWord(string recent)
    {

        if (recentHolder.childCount >= BrainControl.Get().sessionManager.currentSession.sessionSettings.recentWordBias) Destroy(recentHolder.GetChild(recentHolder.childCount - 1).gameObject);

        TextMeshProUGUI newRecent = Instantiate(recentWordTemplate, Vector3.zero, Quaternion.identity).GetComponentInChildren<TextMeshProUGUI>();

        newRecent.transform.SetParent(recentHolder);
        newRecent.transform.SetAsFirstSibling();

        newRecent.text = recent;


        newRecent.transform.localPosition = Vector3.zero;
        newRecent.transform.localEulerAngles = Vector3.zero;
        newRecent.transform.localScale = Vector3.one;
    }


}

