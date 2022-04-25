using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[CreateAssetMenu(fileName = "New WordBankFromTextAsset", menuName = "Data/WordBankFromTextAsset", order = 1)]
public class WordBankFromTextAsset : ScriptableObject
{

    public int minLength;
    public int maxLength;


    public TextAsset textAsset;
    List<string> data;

    // public List<string> filteredData;

    public WordBank filteredData;


    public void RebuildBank()
    {

        data = textAsset.text.Split("\n"[0]).ToList();

        for (int i = 0; i < data.Count; i++)
        {
            data[i] = new string(data[i].Where(c => !char.IsControl(c)).ToArray());
        }

        FilterBank();

    }

    public void FilterBank()
    {

        filteredData = new WordBank();

        foreach (string word in data)
        {

            if (word.Length >= minLength && word.Length <= maxLength) filteredData.AddToBank(word);

        }
    }

    public bool Validate(string checkString)
    {
        bool isPermitted = false;

        if (filteredData.AC.Any(x => x == checkString.ToUpper())) isPermitted = true;
        else if (filteredData.DF.Any(x => x == checkString.ToUpper())) isPermitted = true;
        else if (filteredData.GI.Any(x => x == checkString.ToUpper())) isPermitted = true;
        else if (filteredData.JL.Any(x => x == checkString.ToUpper())) isPermitted = true;
        else if (filteredData.MO.Any(x => x == checkString.ToUpper())) isPermitted = true;
        else if (filteredData.PR.Any(x => x == checkString.ToUpper())) isPermitted = true;
        else if (filteredData.SU.Any(x => x == checkString.ToUpper())) isPermitted = true;
        else if (filteredData.VX.Any(x => x == checkString.ToUpper())) isPermitted = true;
        else if (filteredData.YZ.Any(x => x == checkString.ToUpper())) isPermitted = true;

        Debug.Log("Is " + checkString + " a permitted word?: " + isPermitted);

        return isPermitted;
    }

}
