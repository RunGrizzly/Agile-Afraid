using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum InterruptType { Pause, Win, Fail }

[Serializable]
public class Session
{
    GridGenerator gridGenerator;
    TileRack tileRack;
    public SessionSettings sessionSettings;

    public int score;
    public int level;
    public List<string> recentWords = new List<string>();
    public float elapsed;
    public Level currentLevel;
    public Task levelTracker = null;

    public bool isPaused;


    public Session(SessionSettings _sessionSettings)
    {
        sessionSettings = _sessionSettings;
    }

    public void SetPaused(bool state)
    {

        isPaused = state;

        if (isPaused == true)
        {
            BrainControl.Get().eventManager.e_pauseSession.Invoke();
        }
        else
        {

            BrainControl.Get().eventManager.e_unpauseSession.Invoke();
        }
    }

    //Return a number that represents the amount of times the given string appears in the recent words buffer
    public int RecencyScore(string check)
    {
        return recentWords.Where(x => x == check).ToList().Count;
    }

    public void InitialiseSession()
    {
        //Juice change will handle score
        level = 0;
        elapsed = 0;
        SetPaused(false);

        BrainControl.Get().eventManager.e_juiceChange.Invoke(100 - score);

        //Debug control
        score = 99999;

        //Initial level load
        currentLevel = new Level(sessionSettings.levels[level]);
        //Do initial session track
        levelTracker = new Task(currentLevel.TrackLevel());
        //Initial rack populate
        //Empty the rack
        BrainControl.Get().eventManager.e_emptyRack.Invoke();
        BrainControl.Get().eventManager.e_fillRack.Invoke(sessionSettings.levels[0].rackSize, false);


    }

    public IEnumerator TrackSession()
    {

        InitialiseSession();

        //While the session has not been ended
        while (BrainControl.Get().sessionManager.currentSession != null && BrainControl.Get().sessionManager.currentSession == this)
        {
            //Dev shortcuts
            ///////////////////////////////////
            //Level skip
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.Alpha0))
            {
                Debug.Log("DEVELOPER CONTROL: Skip Level");
                BrainControl.Get().eventManager.e_pathComplete.Invoke();
            }
            //Pause
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Debug.Log("Escape key pressed");
                if (!isPaused) SetPaused(true);
                else SetPaused(false);
            }
            //Empty rack
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKeyUp(KeyCode.Alpha9))
            {
                Debug.Log("DEVELOPER CONTROL: Empty Rack");
                BrainControl.Get().rack.EmptyRack();
            }
            ///////////////////////////////////

            elapsed += Time.deltaTime;

            yield return null;
        }
        //Session end
    }
}

