using UnityEngine;
using UnityEngine.Serialization;

public class Brain : MonoBehaviour
{
    public EventManager eventManager;
    
    public RunManager runManager;

    public UIManager uiManager;

    public StageManager stageManager;

    public TaskManager taskManager;

    public AudioManager audioManager;

    public ButtonControls buttonControls;

    [FormerlySerializedAs("AssetManager")] public DataManager dataManager = null;
    
    public static Brain ins;

    //Key items //CSR
     public GridGenerator Grid;
     public TileRack Rack;

    // public RunSettings RunSettings = null;

    void Awake()
    {
        ins = this;
    }



}
