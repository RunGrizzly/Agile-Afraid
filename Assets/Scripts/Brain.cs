using UnityEngine;

public class Brain : MonoBehaviour
{
    public EventManager eventManager;
    public ScoreManager scoreManager;

    public SessionManager sessionManager;

    public UIManager uiManager;

    public StageManager stageManager;

    public TaskManager taskManager;

    public AudioManager audioManager;

    public ButtonControls buttonControls;

    public static Brain ins;

    //Key items
    public GridGenerator grid;
    public TileRack rack;



    void Awake()
    {
        ins = this;
    }



}
