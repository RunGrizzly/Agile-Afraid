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
            Initialise();
        });

        Initialise();

    }

    public void Initialise()
    {
        StartCoroutine(InitialiseGame());
        BrainControl.Get().eventManager.e_gameInitialised.Invoke();
    }

    public void StartNewSession()
    {
        Task t = new Task(InitialiseSession());
    }

    IEnumerator InitialiseSession()
    {
        List<string> initialScenes = new List<string>() { "Cameras", "UI", "WordGame" };
        Task t_initialiseScenes = new Task(LoadScenes(initialScenes, true));

        while (t_initialiseScenes.Running)
        {
            Debug.Log("Scenes are being initialised");
            yield return null;
        }
        Debug.Log("All scenes initialised");
        //Fire off a new session
        Brain.ins.eventManager.e_newSession.Invoke();
    }



    // IEnumerator LoadScenes(List<string> scenes, string camera, bool fade, bool cleanup)
    // {
    //     //Unload unwanted scenes (all loaded except scene 0)
    //     if (cleanup) SceneUnloader(new List<string> { "Camera", "ClientControls", "Stage", "Backstage", "ControlRoom" });
    //     //Load them asynchronously
    //     SceneLoader(scenes);
    //     //While they are not loaded - wait
    //     while (!CheckLoaded(scenes))
    //     {
    //         yield return null;
    //     }
    //     //Once loaded, end of frame check
    //     yield return new WaitForEndOfFrame();

    //     //Set the stage
    //     Brain.ins.stageManager.SetCameraPriority(camera, fade);
    // }

    IEnumerator InitialiseGame()
    {

        Task t_initialiseGame = new Task(LoadScenes(new List<string>() { "MainMenu" }, true));

        while (t_initialiseGame.Running)
        {
            Debug.Log("Game is being initialised");
            yield return null;
        }
        Debug.Log("Game was initialised");
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
