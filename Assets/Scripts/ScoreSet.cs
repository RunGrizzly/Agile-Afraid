using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Scoreset", menuName = "Scoresets/New Scoreset", order = 1)]
public class ScoreSet : ScriptableObject
{
    public LetterFloatDictionary scoreset = new LetterFloatDictionary();
    public int specificTileCost;
    public int vowelCost;
    public int consonantCost;
    public int fillCost;
    public int newRackCost;

    public int levelRestartCost;


    public Letter LetterFromCharacter(char c)
    {
        return scoreset.Keys.FirstOrDefault(x => x.character == c);
    }

    public Letter LetterFromDistribution(float minDist, float maxDist)
    {

        float r = Random.Range(minDist, maxDist);

        Letter newLetter;

        int i = 0;

        do
        {
            i = Random.Range(0, scoreset.Keys.Count);
            newLetter = new Letter(scoreset.Keys.ElementAt(i));
        }
        while (scoreset.Values.ElementAt(i) < minDist || scoreset.Values.ElementAt(i) > maxDist);

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
        return scoreset.Keys.FirstOrDefault(x => x.character == s[r]);
    }

    public Letter Consonant()
    {
        string s = "bcdfghjklmnpqrstvwxyz";
        int r = Random.Range(0, s.Length);
        return scoreset.Keys.FirstOrDefault(x => x.character == s[r]);
    }

}