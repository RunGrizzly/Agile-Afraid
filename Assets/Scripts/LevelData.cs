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

    public int rackSize;
    public int repeatLimit;

}