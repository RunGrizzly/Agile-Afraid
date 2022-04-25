using System.Collections;
using UnityEngine;
using UnityEngine.Events;



public class EventManager : MonoBehaviour
{

    //Game Events
    /////////////////////
    public BlockSelectEvent e_blockSelected;
    public ClearBlockEvent e_clearBlock;
    public QuitToMenuEvent e_quitToMenu;
    public GameInitialisedEvent e_gameInitialised;
    public NewSessionEvent e_newSession;
    public RestartSessionEvent e_restartSession;
    public RestartLevelEvent e_restartLevel;
    public PauseSessionEvent e_pauseSession;
    public UnpauseSessionEvent e_unpauseSession;

    public FailSessionEvent e_failSession;
    public WinSessionEvent e_winSession;

    public NavUpdateEvent e_navUpdate;
    public PathCompleteEvent e_pathComplete;
    public JuiceChangeEvent e_juiceChange;
    public BeginInputEvent e_beginInput;
    public UpdateInputEvent e_updateInput;
    public EndInputEvent e_endInput;
    /////////////////////

    //Scoring
    /////////////////////
    public ValidateFailEvent e_validateFail;
    public ValidateSuccessEvent e_validateSuccess;
    /////////////////////

    //Tile Rack Events
    /////////////////////
    public EmptyRackEvent e_emptyRack;

    public GetTileEvent e_getTile;

    public GetVowelEvent e_getVowel;
    public GetConsonantEvent e_getConsonant;
    public NewRackEvent e_newRack;
    public FillRackEvent e_fillRack;
    /////////////////////

    public UpdateUIEvent e_updateUI;

    public LevelLoadedEvent e_levelLoaded;


    void Awake()
    {
        //Game events
        /////////////////////
        if (e_blockSelected == null) e_blockSelected = new BlockSelectEvent();
        if (e_clearBlock == null) e_clearBlock = new ClearBlockEvent();
        if (e_quitToMenu == null) e_quitToMenu = new QuitToMenuEvent();
        if (e_gameInitialised == null) e_gameInitialised = new GameInitialisedEvent();
        if (e_newSession == null) e_newSession = new NewSessionEvent();
        if (e_restartSession == null) e_restartSession = new RestartSessionEvent();
        if (e_restartLevel == null) e_restartLevel = new RestartLevelEvent();
        if (e_pauseSession == null) e_pauseSession = new PauseSessionEvent();
        if (e_unpauseSession == null) e_unpauseSession = new UnpauseSessionEvent();
        if (e_navUpdate == null) e_navUpdate = new NavUpdateEvent();
        if (e_pathComplete == null) e_pathComplete = new PathCompleteEvent();
        if (e_juiceChange == null) e_juiceChange = new JuiceChangeEvent();
        if (e_beginInput == null) e_beginInput = new BeginInputEvent();
        if (e_updateInput == null) e_updateInput = new UpdateInputEvent();
        if (e_endInput == null) e_endInput = new EndInputEvent();
        /////////////////////

        //Score Events
        /////////////////////
        if (e_validateFail == null) e_validateFail = new ValidateFailEvent();
        if (e_validateSuccess == null) e_validateSuccess = new ValidateSuccessEvent();
        /////////////////////

        //Tile Rack Events
        /////////////////////
        if (e_emptyRack == null) e_emptyRack = new EmptyRackEvent();
        if (e_getTile == null) e_getTile = new GetTileEvent();
        if (e_getVowel == null) e_getVowel = new GetVowelEvent();
        if (e_getConsonant == null) e_getConsonant = new GetConsonantEvent();
        if (e_newRack == null) e_newRack = new NewRackEvent();
        if (e_fillRack == null) e_fillRack = new FillRackEvent();
        /////////////////////

        //End Session Events
        /////////////////////
        if (e_failSession == null) e_failSession = new FailSessionEvent();
        if (e_winSession == null) e_winSession = new WinSessionEvent();
        /////////////////////

        if (e_updateUI == null) e_updateUI = new UpdateUIEvent();
        if (e_levelLoaded == null) e_levelLoaded = new LevelLoadedEvent();

    }
}