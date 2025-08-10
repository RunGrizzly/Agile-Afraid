using UnityEngine.Events;
using UnityEngine;
using System.Collections.Generic;


//Game Events
/////////////////////
public class BlockSelectEvent : UnityEvent<LetterBlock> { }
public class ClearBlockEvent : UnityEvent<LetterBlock> { }
public class QuitToMenuEvent : UnityEvent { }
public class GameInitialisedEvent : UnityEvent { }

//Run Scope     //////////////////////////////////
public class NewRunEvent : UnityEvent<Run> { }
//public class RestartRunEvent : UnityEvent { }
public class PauseRunEvent : UnityEvent { }
public class UnpauseRunEvent : UnityEvent { }
public class FailRunEvent : UnityEvent { }
public class WinRunEvent : UnityEvent { }
////////////////////////////////////////////////

//Level Scope   //////////////////////////////////
public class LevelLoadedEvent : UnityEvent<Level> { }
public class LevelSuccessEvent : UnityEvent<Level> { }
public class LevelFailEvent : UnityEvent<Level> { }
//Not allowed anymore
//public class RestartLevelEvent : UnityEvent { }
////////////////////////////////////////////////

//Input Events   //////////////////////////////////
public class BeginInputEvent : UnityEvent<LetterBlock> { }
public class UpdateInputEvent : UnityEvent<LetterBlock> { }
public class EndInputEvent : UnityEvent { }
/////////////////////

//Scoring events
/////////////////////
public class ValidateFailEvent : UnityEvent { }
public class ValidateSuccessEvent : UnityEvent<BlockInput> { }
/////////////////////

//Tile Rack Events
/////////////////////
public class EmptyRackEvent : UnityEvent { }
public class GetTileEvent : UnityEvent<Letter, bool, bool> { } //Tile base letter, costs score, costs time
public class GetVowelEvent : UnityEvent<bool, bool> { }  //costs score, costs time
public class GetConsonantEvent : UnityEvent<bool, bool> { } //costs score, costs time
public class NewRackEvent : UnityEvent<List<char>,int, bool, bool> { } //Seed chars, fill to, costs score, costs time
public class FillRackEvent : UnityEvent<int, bool, bool> { } //fill to, costs score, costs time
/////////////////////

public class UpdateUIEvent : UnityEvent { }
public class PipAddedEvent: UnityEvent<TimePip> { }
public class PipRemovedEvent: UnityEvent<TimePip> { }