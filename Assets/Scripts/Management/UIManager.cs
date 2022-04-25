using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public Transform gameCanvas;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;

    public GameObject inputControlTemplate;

    GameObject inputControlCanvas;

    public GameObject interruptPanelTemplate;

    public InterruptPanel interruptPanel;

    void SetLevelText(int level)
    {
        levelText.text = (level + 1).ToString();
    }


    void Start()
    {

        // BrainControl.Get().eventManager.e_juiceChange.AddListener((s) => BrainControl.Get().eventManager.e_updateUI.Invoke());
        // BrainControl.Get().eventManager.e_pathComplete.AddListener(() => SetLevelText(Brain.ins.sessionManager.currentSession.level + 1));

        BrainControl.Get().eventManager.e_updateUI.AddListener(() =>
        {
            scoreText.text = (Brain.ins.sessionManager.currentSession.score).ToString();
            SetLevelText(Brain.ins.sessionManager.currentSession.level);
        });

        BrainControl.Get().eventManager.e_blockSelected.AddListener((b) =>
         {
             if (b.lockState == LockState.locked) return;
             if (b.fillState == FillState.empty) return;

             if (inputControlCanvas == null) inputControlCanvas = Instantiate(inputControlTemplate, b.transform.position, Quaternion.identity);
             //inputControlCanvas.transform.SetParent(b.transform);
             //Ensure it faces up
             inputControlCanvas.transform.localEulerAngles = new Vector3(90, 0, 0);
             //Set Position
             LeanTween.move(inputControlCanvas, new Vector3(b.transform.position.x, 0.75f, b.transform.position.z), 0.35f).setEase(LeanTweenType.easeOutExpo);


         });

        BrainControl.Get().eventManager.e_beginInput.AddListener((b) =>
        {
            if (inputControlCanvas == null) inputControlCanvas = Instantiate(inputControlTemplate, b.transform.position, Quaternion.identity);
            //Ensure it faces up
            inputControlCanvas.transform.localEulerAngles = new Vector3(90, 0, 0);
            //Set Position
            LeanTween.move(inputControlCanvas, new Vector3(b.transform.position.x, 0.75f, b.transform.position.z), 0.35f).setEase(LeanTweenType.easeOutExpo);
        });

        BrainControl.Get().eventManager.e_updateInput.AddListener((u) =>
        {
            if (inputControlCanvas == null) inputControlCanvas = Instantiate(inputControlTemplate, u.transform.position, Quaternion.identity);

            //Ensure it faces up
            inputControlCanvas.transform.localEulerAngles = new Vector3(90, 0, 0);
            //Set Position
            LeanTween.move(inputControlCanvas, new Vector3(u.transform.position.x, 0.75f, u.transform.position.z), 0.35f).setEase(LeanTweenType.easeOutExpo);
        });

        BrainControl.Get().eventManager.e_endInput.AddListener(() =>
        {
            Destroy(inputControlCanvas);
        });

        BrainControl.Get().eventManager.e_clearBlock.AddListener((c) =>
        {
            Destroy(inputControlCanvas);
        });

        BrainControl.Get().eventManager.e_restartSession.AddListener(() =>
        {
            Destroy(inputControlCanvas);
        });

        BrainControl.Get().eventManager.e_restartLevel.AddListener(() =>
      {
          Destroy(inputControlCanvas);
      });

        BrainControl.Get().eventManager.e_quitToMenu.AddListener(() =>
        {
            Destroy(inputControlCanvas);
        });

        BrainControl.Get().eventManager.e_pauseSession.AddListener(() =>
        {

            if (interruptPanel != null) interruptPanel.KillPanel();

            interruptPanel = Instantiate(interruptPanelTemplate, Vector3.zero, Quaternion.identity).GetComponent<InterruptPanel>();
            interruptPanel.transform.SetParent(gameCanvas.transform, false);
            interruptPanel.transform.localPosition = Vector3.zero;
            interruptPanel.BuildPause();

        });

        BrainControl.Get().eventManager.e_failSession.AddListener(() =>
        {

            if (interruptPanel != null) interruptPanel.KillPanel();

            interruptPanel = Instantiate(interruptPanelTemplate, Vector3.zero, Quaternion.identity).GetComponent<InterruptPanel>();
            interruptPanel.transform.SetParent(gameCanvas.transform, false);
            interruptPanel.transform.localPosition = Vector3.zero;
            interruptPanel.BuildFail();

        });

        BrainControl.Get().eventManager.e_winSession.AddListener(() =>
        {

            if (interruptPanel != null) interruptPanel.KillPanel();

            interruptPanel = Instantiate(interruptPanelTemplate, Vector3.zero, Quaternion.identity).GetComponent<InterruptPanel>();
            interruptPanel.transform.SetParent(gameCanvas.transform, false);
            interruptPanel.transform.localPosition = Vector3.zero;
            interruptPanel.BuildWin();

        });





        // case InterruptType.Win:
        //     newPanel = Instantiate(interruptPanelTemplate, Vector3.zero, Quaternion.identity).GetComponent<InterruptPanel>();
        //     newPanel.transform.SetParent(gameCanvas.transform, false);
        //     newPanel.transform.localPosition = Vector3.zero;
        //     newPanel.BuildWin();
        //     break;

        // case InterruptType.Fail:
        //     newPanel = Instantiate(interruptPanelTemplate, Vector3.zero, Quaternion.identity).GetComponent<InterruptPanel>();
        //     newPanel.transform.SetParent(gameCanvas.transform, false);
        //     newPanel.transform.localPosition = Vector3.zero;
        //     newPanel.BuildLose();
        //     break;

        BrainControl.Get().eventManager.e_unpauseSession.AddListener(() =>
        {

            if (interruptPanel != null) interruptPanel.KillPanel();

        });

    }


}

