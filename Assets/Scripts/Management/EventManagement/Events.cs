using UnityEngine.Events;
using UnityEngine;
using System.Collections.Generic;


//Game Events
/////////////////////
public class BlockSelectEvent : UnityEvent<LetterBlock> { }
public class ClearBlockEvent : UnityEvent<LetterBlock> { }
public class QuitToMenuEvent : UnityEvent { }
public class GameInitialisedEvent : UnityEvent { }
public class NewSessionEvent : UnityEvent { }
public class RestartSessionEvent : UnityEvent { }
public class RestartLevelEvent : UnityEvent { }
public class PauseSessionEvent : UnityEvent { }
public class UnpauseSessionEvent : UnityEvent { }
public class NavUpdateEvent : UnityEvent { }
public class PathCompleteEvent : UnityEvent { }
public class JuiceChangeEvent : UnityEvent<int> { }
public class BeginInputEvent : UnityEvent<LetterBlock> { }
public class UpdateInputEvent : UnityEvent<LetterBlock> { }
public class EndInputEvent : UnityEvent { }
/////////////////////

//Scoring events
/////////////////////
public class ValidateFailEvent : UnityEvent<int> { }
public class ValidateSuccessEvent : UnityEvent<int> { }
/////////////////////



//Tile Rack Events
/////////////////////
public class EmptyRackEvent : UnityEvent { }

public class GetTileEvent : UnityEvent<Letter, bool> { }
public class GetVowelEvent : UnityEvent<bool> { }
public class GetConsonantEvent : UnityEvent<bool> { }
public class NewRackEvent : UnityEvent<int, bool> { }
public class FillRackEvent : UnityEvent<int, bool> { }
/////////////////////

public class FailSessionEvent : UnityEvent { }
public class WinSessionEvent : UnityEvent { }

public class UpdateUIEvent : UnityEvent { }

public class LevelLoadedEvent : UnityEvent<Level> { }

