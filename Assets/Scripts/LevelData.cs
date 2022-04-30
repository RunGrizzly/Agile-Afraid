using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelData
{
    //Grid dimensions by level
    public Vector2Int gridDims;
    public PlayableClip music;
    public Color deadZoneColor;
    //Seeds
    public List<WordRequest> seeds = new List<WordRequest>();

    //Score goals // Get all to trophy a level

    //A target number of moves - this or under counts towards a trophy
    public int par;

    //A target score to beat - this or more counts towards a trophy
    public int trophyScore;


    public int rackSize;
    public int repeatLimit;

}