using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "New Scoreset", menuName = "Scoresets/New Scoreset", order = 1)]
public class ScoreSet : SerializedScriptableObject
{
    [OdinSerialize]
    [DictionaryDrawerSettings(KeyLabel = "Letter",ValueLabel = "Rarity Score")]
    public Dictionary<Letter,float> distribution = new Dictionary<Letter,float>();

    //This cost is in pips
    public int specificTileScoreCost;
    public int specificTilePipCost;

    public int vowelScoreCost;
    public int vowelPipCost;

    public int consonantScoreCost;
    public int consonantPipCost;

    public int fillRackScoreCost;
    public int fillRackPipCost;

    public int newRackScoreCost;
    public int newRackPipCost;

    public int levelRestartCost;
    public int validateFailPenalty;

    public int ScoreFromBlocks(BlockLine line)
    {
        int score = 0;

        foreach (LetterBlock block in line.blocks)
        {
            score += block.letter.score;

        }

        return (score * line.blocks.Count) / (1 + BrainControl.Get().runManager.CurrentRun.RecencyScore(GridTools.WordFromLine(line).forwards));

    }
    
    public Letter WeightedRandom()
    {
        //Get the total weights
        var sum = distribution.Values.Sum();

        //Get a random point along the sum of values
        float r = Random.value * sum;

        //We accumulate?
        float cumulative = 0f;

        //Go through the distribution
        foreach (var kvp in distribution)
        {
            //We add the value of the selected key
            cumulative += kvp.Value;

            if (r <= cumulative)
            {
                return kvp.Key;
            }
        }

        return null;
    }

    public List<Letter> WeightedRandomSet(int setSize)
    {
        List<Letter> newSet = new List<Letter>();

        //Get the total weights
        var sum = distribution.Values.Sum();
        
        for (int i = 0; i < setSize; i++)
        {
            //Get a random point along the sum of values
            float r = Random.value * sum;
            
            //We accumulate?
            float cumulative = 0f;

            //Go through the distribution
            foreach (var kvp in distribution)
            {
                //We add the value of the selected key
                cumulative += kvp.Value;
                
                if (r <= cumulative)
                {
                    newSet.Add(kvp.Key);
                    break;
                }
            }
        }
        
        return newSet;
    }
    
    public Letter LetterFromCharacter(char c)
    {
        return distribution.Keys.FirstOrDefault(x => x.character == c);
    }

    public Letter LetterFromDistribution(float minDist, float maxDist)
    {

        float r = Random.Range(minDist, maxDist);

        Letter newLetter;

        int i = 0;

        do
        {
            i = Random.Range(0, distribution.Keys.Count);
            newLetter = new Letter(distribution.Keys.ElementAt(i));
        }
        while (distribution.Values.ElementAt(i) < minDist || distribution.Values.ElementAt(i) > maxDist);

        return newLetter;
    }

    public Letter WeightedLetter()
    {
        //Get a lower value that will randomly cut off a portion of the letters.
        float minDist = Random.value * 100;

        return (LetterFromDistribution(minDist, 100));
    }

    public Letter Vowel()
    {
        string s = "aeiou";
        int r = Random.Range(0, s.Length);
        return distribution.Keys.FirstOrDefault(x => x.character == s[r]);
    }

    public Letter Consonant()
    {
        string s = "bcdfghjklmnpqrstvwxyz";
        int r = Random.Range(0, s.Length);
        return distribution.Keys.FirstOrDefault(x => x.character == s[r]);
    }

}