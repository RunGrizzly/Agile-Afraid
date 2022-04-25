using System;
using System.Collections.Generic;

[Serializable]
public class WordBank
{



    public List<string> AC = new List<string>();
    public List<string> DF = new List<string>();

    public List<string> GI = new List<string>();
    public List<string> JL = new List<string>();
    public List<string> MO = new List<string>();
    public List<string> PR = new List<string>();
    public List<string> SU = new List<string>();
    public List<string> VX = new List<string>();
    public List<string> YZ = new List<string>();




    public void AddToBank(string newWord)
    {

        switch (newWord[0])
        {
            //AF
            case 'A':
            case 'B':
            case 'C':
                AC.Add(newWord);
                break;
            case 'D':
            case 'E':
            case 'F':
                DF.Add(newWord);
                break;
            //GL
            case 'G':
            case 'H':
            case 'I':
                GI.Add(newWord);
                break;
            case 'J':
            case 'K':
            case 'L':
                JL.Add(newWord);
                break;
            //MR
            case 'M':
            case 'N':
            case 'O':
                MO.Add(newWord);
                break;
            case 'P':
            case 'Q':
            case 'R':
                PR.Add(newWord);
                break;
            //SZ
            case 'S':
            case 'T':
            case 'U':
                SU.Add(newWord);
                break;
            case 'V':
            case 'W':
            case 'X':
                VX.Add(newWord);
                break;
            case 'Y':
            case 'Z':
                YZ.Add(newWord);
                break;
        }




    }



}

