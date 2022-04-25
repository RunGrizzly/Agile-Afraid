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


    public void BuildPause()
    {
        panelTitle.text = "Paused";
        panelQuip.text = "This is a funny quip lol.";

        //Add Buttons
        /////////////////
        Button newButton;

        newButton = BuildButton("Resume");
        newButton.onClick.AddListener(() => { BrainControl.Get().sessionManager.currentSession.SetPaused(false); });

        newButton = BuildButton("Restart Session");
        newButton.onClick.AddListener(() => { BrainControl.Get().eventManager.e_restartSession.Invoke(); });

        newButton = BuildButton("Restart Level");
        newButton.onClick.AddListener(() =>
         {
             BrainControl.Get().eventManager.e_restartLevel.Invoke();
             BrainControl.Get().sessionManager.currentSession.SetPaused(false);
         });

        newButton = BuildButton("Quit To Menu");
        newButton.onClick.AddListener(() => { BrainControl.Get().eventManager.e_quitToMenu.Invoke(); });

        /////////////////

    }

    public void BuildWin()
    {
        panelTitle.text = "You Won";
        panelQuip.text = "GG WP";

        Button newButton;

        newButton = BuildButton("Restart");
        newButton.onClick.AddListener(() => { BrainControl.Get().eventManager.e_restartSession.Invoke(); });

        newButton = BuildButton("Quit To Menu");
        newButton.onClick.AddListener(() => { BrainControl.Get().eventManager.e_quitToMenu.Invoke(); });
    }

    public void BuildFail()
    {
        panelTitle.text = "You Lost";
        panelQuip.text = "Actual sad face";

        Button newButton;

        newButton = BuildButton("Restart");
        newButton.onClick.AddListener(() => { BrainControl.Get().eventManager.e_restartSession.Invoke(); });

        newButton = BuildButton("Quit To Menu");
        newButton.onClick.AddListener(() => { BrainControl.Get().eventManager.e_quitToMenu.Invoke(); });
    }

    public Button BuildButton(string label)
    {
        Button newButton = Instantiate(buttonTemplate, Vector3.zero, Quaternion.identity).GetComponent<Button>();
        newButton.transform.SetParent(buttonList);
        newButton.transform.localPosition = Vector3.zero;
        newButton.transform.localEulerAngles = Vector3.zero;
        newButton.GetComponentInChildren<TextMeshProUGUI>().text = label;
        return newButton;
    }

    public void KillPanel()
    {
        Destroy(gameObject);
    }

}
