using UnityEngine;
using UnityEngine.Serialization;

public class Brain : MonoBehaviour
{
    public EventManager eventManager;
    
    [FormerlySerializedAs("sessionManager")] public RunManager runManager;

    public UIManager uiManager;

    public StageManager stageManager;

    public TaskManager taskManager;

    public AudioManager audioManager;

    public ButtonControls buttonControls;

    public static Brain ins;

    //Key items
    public GridGenerator grid;
    public TileRack rack;

    public RunSettings RunSettings = null;

    void Awake()
    {
        ins = this;
    }



}
