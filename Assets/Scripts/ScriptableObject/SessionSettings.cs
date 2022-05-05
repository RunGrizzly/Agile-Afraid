using System;
using System.Collections.Generic;
using UnityEngine;

//Sesssion level settings
[Serializable]
[CreateAssetMenu(fileName = "New Session Settings", menuName = "Game Settings/New Session Settings", order = 1)]
public class SessionSettings : ScriptableObject
{

    //The amount of words stored as recent - adds to penalty
    public int recentWordBias;
    public List<LevelData> levels;

    public WordBankFromTextAsset permittedWords;

}