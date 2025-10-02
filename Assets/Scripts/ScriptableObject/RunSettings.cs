using System;
using UnityEngine;

//This doesn't do much
//Sesssion level settings
[Serializable]
[CreateAssetMenu(fileName = "New Session Settings", menuName = "Game Settings/New Session Settings", order = 1)]
public class RunSettings : ScriptableObject
{
    //The amount of words stored as recent - adds to penalty
    public int recentWordBias;
    
    public WordBankFromTextAsset permittedWords;

    //public ScoreSet ActiveScoringRubrik = null;
    
}