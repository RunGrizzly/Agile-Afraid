using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

//A set of aggregated levels that a "Run" uses to inform the level order and parameters
[Serializable]
[CreateAssetMenu(fileName = "New LevelSet", menuName = "Data/New LevelSet", order = 1)]
public class LevelSet : SerializedScriptableObject
{
    public string LevelSetID;
    
    [OdinSerialize]
    public List<LevelData> Levels;

    public bool IsTimed = true;
    
    public ScoreSet ScoringRubrik;

    public List<Letter> RackSeeds = new List<Letter>();

    public int RackSize = 8;
    
    public int GetLevelIndex(Level level)
    {
        return Levels.IndexOf(level.Data);
    }

    [Button]
    public void AutomaticSeedValues()
    {
        if (ScoringRubrik != null)
        {
            foreach (Letter letter in RackSeeds)
            {
                var key = ScoringRubrik.distribution.Keys.FirstOrDefault(k => k.character.ToString().ToLower() == letter.character.ToString().ToLower());

                if (key != null)
                {
                    letter.score = key.score;
                }
            }
        }
    }
}