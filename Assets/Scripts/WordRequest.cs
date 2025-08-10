using UnityEngine;
using System;
using System.Collections.Generic;

public enum PlacementType { BottomLeft, BottomRight, TopRight, TopLeft, Random, Center }

[Serializable]
public class WordRequest
{
    public string word;
    //public Vector2Int start;

    public PlacementType placementType = PlacementType.BottomLeft;

    // public List<LetterBlock> assignedBlocks = new List<LetterBlock>();
    //
    // public List<BlockLine> possibleLines;

    public WordRequest(string _word, PlacementType _placementType)
    {
        word = _word;
        placementType = _placementType;
    }

}