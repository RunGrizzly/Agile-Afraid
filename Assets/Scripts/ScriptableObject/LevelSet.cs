using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New LevelSet", menuName = "Data/New LevelSet", order = 1)]
public class LevelSet : SerializedScriptableObject
{
    public string LevelSetID;

    public float SqueakyTime = 5f;
    public float CoreTime = 10f;
        
    [OdinSerialize]
    public List<LevelData> Levels;

    public int GetLevelIndex(Level level)
    {
        return Levels.IndexOf(level.Data);
    }
    
}