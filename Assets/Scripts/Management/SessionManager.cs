using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class SessionManager : MonoBehaviour
{

    public SessionSettings sessionSettings;

    public Session currentSession;

    public Task sessionTracker;

    //Game framework broadly looks like this

    //Game////////////////
    //////////////////////
    //                  //
    //  //Session/////////
    //////////////////////
    //  //              //
    //  //  //Level///////
    //  //  //////////////
    //  //  //          //
    //  //  //          //
    //  //  //          //
    //  //  //          //
    //  //  //          //
    //////////////////////
    //////////////////////


    //Track session here
    void Start()
    {
        Brain.ins.eventManager.e_newSession.AddListener(() =>
        {
            currentSession = new Session(sessionSettings);
            sessionTracker = new Task(currentSession.TrackSession());
        });


        Brain.ins.eventManager.e_quitToMenu.AddListener(() =>
        {
            currentSession = null;
        });


        BrainControl.Get().eventManager.e_restartSession.AddListener(() =>
        {
            currentSession.InitialiseSession();
        });

        BrainControl.Get().eventManager.e_restartLevel.AddListener(() =>
       {
           Debug.Log("Level Restarted");
           //Level incrementation
           currentSession.currentLevel = new Level(sessionSettings.levels[currentSession.level]);
           currentSession.levelTracker.Stop();
           currentSession.levelTracker = new Task(currentSession.currentLevel.TrackLevel());
       });

        //Level incrementation
        //////////////////////
        BrainControl.Get().eventManager.e_pathComplete.AddListener(() =>
        {
            Debug.Log("A path was completed");
            if (currentSession.level >= sessionSettings.levels.Count - 1)
            {
                BrainControl.Get().eventManager.e_winSession.Invoke();
                return;
            }
            currentSession.level += 1;
            currentSession.currentLevel = new Level(sessionSettings.levels[currentSession.level]);
            currentSession.levelTracker.Stop();
            currentSession.levelTracker = new Task(currentSession.currentLevel.TrackLevel());

        });
        //////////////////////

        //Modify score
        //////////////////////
        BrainControl.Get().eventManager.e_juiceChange.AddListener((c) =>
        {
            currentSession.score = (int)Mathf.Clamp((float)currentSession.score + (float)c, 0, Mathf.Infinity);
            BrainControl.Get().eventManager.e_updateUI.Invoke();
            if (currentSession.score <= 0) BrainControl.Get().eventManager.e_failSession.Invoke();
        });
        //////////////////////


        Brain.ins.eventManager.e_clearBlock.AddListener((c) =>
        {
            currentSession.currentLevel.LatestInput().RemoveFromInput(c);
            //If this would kill the input
            if (currentSession.currentLevel.LatestInput().blocks.Count == 0) currentSession.currentLevel.RemoveInput(currentSession.currentLevel.LatestInput());
        });

        //A new input is called
        //////////////////////
        Brain.ins.eventManager.e_beginInput.AddListener((a) =>
        {
            // //Check for any initial adjacency
            currentSession.currentLevel.inputs.Add(new BlockInput(new List<LetterBlock>() { /*BrainControl.Get().grid.CheckAdjacency(a),*/ a }, false, false));
        });
        //////////////////////

        //The current input is updated
        //////////////////////
        Brain.ins.eventManager.e_updateInput.AddListener((u) =>
        {
            currentSession.currentLevel.LatestInput().AddToInput(u);
        });
        //////////////////////

        //The current input is ended
        //////////////////////
        Brain.ins.eventManager.e_endInput.AddListener(() =>
        {
            currentSession.currentLevel.LatestInput().Compile();
        });
        //////////////////////

        BrainControl.Get().eventManager.e_pauseSession.AddListener(() =>
        {


        });

        BrainControl.Get().eventManager.e_unpauseSession.AddListener(() =>
        {

        });

        ////////////////////////
        ////////////////////////

    }



}
