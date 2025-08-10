using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Flags]
public enum LevelRequirements
{
    None = 0,
    AllBlocksFilled = 1<<0, //Fill all spaces on the grid
    AllUniqueCharacters = 1<<1, //Every input character different
    UseAllTiles = 1<<2, //Use all tiles from the rack
    DontStutter = 1<<3 //No words with repeat letters
}


public class LevelData
{
    public GridData gridData = new GridData();

    public PlayableClip music;
    public Color DeadZoneColorA;
    public Color DeadZoneColorB;
    public LevelRequirements LevelRequirements;
    public bool IsTimed = true;
    public bool AllowTileRequests = true;
    
    //Seeds
    //public List<WordRequest> seeds = new List<WordRequest>();
    
    //Score goals // Get all to trophy a level
    //A target number of moves - this or under counts towards a trophy
    public int par;

    //A target score to beat - this or more counts towards a trophy
    public int trophyScore;
    public List<char> RackSeeds = new List<char>();
    public int rackSize;
    public int repeatLimit;

}