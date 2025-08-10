using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class StageManager : MonoBehaviour
{
    void Start()
    {
        //Subscribe to quit to menu event
        BrainControl.Get().eventManager.e_quitToMenu.AddListener(() =>
        {
            InitialiseGame();
        });

        InitialiseGame();
    }

    private void InitialiseGame()
    {
        Task t = new Task(InitialiseGameRoutine());
    }
    
     
    private IEnumerator InitialiseGameRoutine()
    {

        Task t_initialiseGame = new Task(LoadScenes(new List<string>() { "Cameras", "MainMenu" }, true));

        while (t_initialiseGame.Running)
        {
            Debug.Log("Game is being initialised");
            yield return null;
        }
        
        Debug.Log("Game was initialised");
        BrainControl.Get().eventManager.e_gameInitialised.Invoke();
    }

    public void InitialiseRun(LevelSet levelSet)
    {
        Task t = new Task(InitialiseRunRoutine(levelSet));
    }

    private IEnumerator InitialiseRunRoutine(LevelSet levelSet)
    {
        //Actual scene load
        List<string> initialScenes = new List<string>() { "UI", "WordGame" };
        Task t_initialiseScenes = new Task(LoadScenes(initialScenes, true));
        
        while (t_initialiseScenes.Running)
        {
            Debug.Log("Scenes are being initialised");
            yield return null;
        }
        
        //We have entered the game scene
        Debug.Log("All scenes initialised");
        
        //As soon as the appropriate scenes are loaded, we tacitly permit the run to start
        Brain.ins.runManager.StartNewRun(Brain.ins.RunSettings, levelSet);
        
        //So we are ready to initialise the run
        //This should be the entry point for the run kickoff
        
        //This should only be done to respond to an actually loaded run
        //Brain.ins.eventManager.e_newRun.Invoke(Brain.ins.RunSettings, levelSet);
        
    }
  
    IEnumerator LoadScenes(List<string> scenes, bool cleanup)
    {
        //Unload unwanted scenes (all loaded except scene 0)
        if (cleanup) SceneUnloader(new List<string> { "Camera", "UI", "Stage", "WordGame", "MainMenu" });
        Debug.Log("Scenes cleaned");
        //Load them asynchronously
        SceneLoader(scenes);
        //While they are not loaded - wait
        while (!CheckLoaded(scenes))
        {
            yield return null;
        }
        //Once loaded, end of frame check
        yield return new WaitForEndOfFrame();
    }

    IEnumerator UnloadScenes(List<string> scenes)
    {
        //Unload unwanted scenes (all loaded except scene 0)
        SceneUnloader(new List<string>(scenes));
        //While they are not loaded - wait
        while (CheckLoaded(scenes))
        {
            yield return null;
        }
        //Once loaded, end of frame check
        yield return new WaitForEndOfFrame();
    }

    public void SceneLoader(List<string> scenes)
    {
        Debug.Log("Loading requested scenes");
        foreach (string scene in scenes)
        {
            if (!SceneManager.GetSceneByName(scene).isLoaded) SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        }
    }

    public void SceneUnloader(List<string> scenes)
    {
        Debug.Log("Unloading requested scenes");
        foreach (string scene in scenes)
        {
            if (SceneManager.GetSceneByName(scene).isLoaded) SceneManager.UnloadSceneAsync(scene);
        }
    }

    //Returns true if any of the passed scenes are loaded
    public bool CheckLoaded(List<string> scenes)
    {
        foreach (string scene in scenes)
        {
            bool l = SceneManager.GetSceneByName(scene).isLoaded;
            Debug.Log("Scene: " + scene + " is loaded? - " + l);
            if (l == false) return false;
        }

        return true;
    }

    public void QuitGame()
    {
        // save any game data here
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
             Application.Quit();
#endif
    }
}
