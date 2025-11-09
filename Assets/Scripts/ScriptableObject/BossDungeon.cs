using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

public abstract class BossType
{
    public List<string> PreferredWords = new List<string>();
}

public class Boss : BossType
{
    //The starting HP of the boss
    public float MaxHP = 100;
}

//A set of aggregated levels that a "Run" uses to inform the level order and parameters
//Level sets are being recontextualised as "bosses"
[Serializable]
[CreateAssetMenu(fileName = "New BossDungeon", menuName = "Data/New BossDungeon", order = 1)]
public class BossDungeon : SerializedScriptableObject
{
    public Boss Boss = null;

    public string SelectionID;

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