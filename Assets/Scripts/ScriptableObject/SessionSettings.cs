using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "New Session Settings", menuName = "Game Settings/New Session Settings", order = 1)]
public class SessionSettings : ScriptableObject
{

    public List<LevelData> levels;

    public WordBankFromTextAsset permittedWords;

}