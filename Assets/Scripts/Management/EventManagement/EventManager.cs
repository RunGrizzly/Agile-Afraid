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
    public NewRunEvent e_newRun;
    //public RestartRunEvent e_restartRun;
    //public RestartLevelEvent e_restartLevel;
    public PauseRunEvent e_pauseRun;
    public UnpauseRunEvent e_unpauseRun;

    public FailRunEvent e_failRun;
    public WinRunEvent e_winRun;

    // public NavUpdateEvent e_navUpdate;
    public LevelSuccessEvent e_levelSuccess;
    public LevelFailEvent e_levelFail; //This currently does nothing as nothing listens to it
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

    public GetVowelEvent e_getVowelRequest;
    public GetConsonantEvent e_getConsonantRequest;
    public NewRackEvent e_newRackRequest;
    public FillRackEvent e_fillRackRequest;
    /////////////////////

    public UpdateUIEvent e_updateUI;
    public LevelLoadedEvent e_levelLoaded;
    public PipAddedEvent e_pipAdded;
    public PipRemovedEvent e_pipRemoved;


    void Awake()
    {
        //Game events
        /////////////////////
        if (e_blockSelected == null) e_blockSelected = new BlockSelectEvent();
        if (e_clearBlock == null) e_clearBlock = new ClearBlockEvent();
        if (e_quitToMenu == null) e_quitToMenu = new QuitToMenuEvent();
        if (e_gameInitialised == null) e_gameInitialised = new GameInitialisedEvent();
        if (e_newRun == null) e_newRun = new NewRunEvent();
        //if (e_restartRun == null) e_restartRun = new RestartRunEvent();
        //if (e_restartLevel == null) e_restartLevel = new RestartLevelEvent();
        if (e_pauseRun == null) e_pauseRun = new PauseRunEvent();
        if (e_unpauseRun == null) e_unpauseRun = new UnpauseRunEvent();
        // if (e_navUpdate == null) e_navUpdate = new NavUpdateEvent();
        if (e_levelSuccess == null) e_levelSuccess = new LevelSuccessEvent();
        if (e_levelFail == null) e_levelFail = new LevelFailEvent();
        // if (e_juiceChange == null) e_juiceChange = new JuiceChangeEvent();
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
        if (e_getVowelRequest == null) e_getVowelRequest = new GetVowelEvent();
        if (e_getConsonantRequest == null) e_getConsonantRequest = new GetConsonantEvent();
        if (e_newRackRequest == null) e_newRackRequest = new NewRackEvent();
        if (e_fillRackRequest == null) e_fillRackRequest = new FillRackEvent();
        /////////////////////

        //End Session Events
        /////////////////////
        if (e_failRun == null) e_failRun = new FailRunEvent();
        if (e_winRun == null) e_winRun = new WinRunEvent();
        /////////////////////

        if (e_updateUI == null) e_updateUI = new UpdateUIEvent();
        if (e_levelLoaded == null) e_levelLoaded = new LevelLoadedEvent();

        if (e_pipAdded == null) e_pipAdded = new PipAddedEvent();
        if (e_pipRemoved == null) e_pipRemoved = new PipRemovedEvent();

    }
}