using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class TileRack : MonoBehaviour
{
    public LetterTile tileTemplate;
    public Transform tileHolder;
    public List<LetterTile> letterTiles = new List<LetterTile>();

    void Start()
    {
        BrainControl.Get().eventManager.e_getTile.AddListener((letter, costScore,costTime) => _ = AddTile(letter.character));
        BrainControl.Get().eventManager.e_getVowelRequest.AddListener((costsScore,costsTime) => _ = AddVowel());
        BrainControl.Get().eventManager.e_getConsonantRequest.AddListener((costsScore,costsTime) => _ = AddConsonant());
        BrainControl.Get().eventManager.e_emptyRack.AddListener(() => _ = EmptyRack());
        BrainControl.Get().eventManager.e_newRackRequest.AddListener(    (seedCharacters,fillTo, costsScore, costsTime) =>
        {
            _ = EmptyRack();
            _ = FillRack(seedCharacters,fillTo);
        });
        BrainControl.Get().eventManager.e_fillRackRequest.AddListener((fillTo, costsScore, costsTime) =>
        {
            _ = FillRack(null,fillTo);
        });
    }

    public bool AddRandomTile()
    {
        if (AtCapacity())
        {
            Debug.LogWarningFormat($"The tile rack is at capacity. You cannot add a new tile.");
            return false;
        }

        //Add a new blank tile 
        LetterTile newTile = Instantiate(tileTemplate, Vector3.zero, Quaternion.identity);

        
        int tries = 0;

        do
        {
            newTile.BuildWeighted();
            tries += 1;
        }
        while (GetCountOfChar(newTile.Letter.character) >= RepeatLimit() && tries < 10);

        newTile.name = newTile.Letter.character.ToString();

        newTile.transform.SetParent(tileHolder);
        newTile.transform.SetAsLastSibling();

        newTile.transform.localScale = Vector3.one;
        newTile.transform.localEulerAngles = Vector3.zero;
        newTile.transform.localPosition = Vector3.zero;

        letterTiles.Add(newTile);

        return true;

    }

    public bool AddVowel()
    {

        if (AtCapacity()) return false;

        LetterTile newTile = Instantiate(tileTemplate, Vector3.zero, Quaternion.identity).GetComponent<LetterTile>();

        int tries = 0;

        do
        {
            newTile.BuildVowel();
        }
        while (GetCountOfChar(newTile.Letter.character) >= RepeatLimit() && tries < 10);

        newTile.name = newTile.Letter.character.ToString();

        newTile.transform.SetParent(tileHolder);
        newTile.transform.SetAsLastSibling();

        newTile.transform.localScale = Vector3.one;
        newTile.transform.localEulerAngles = Vector3.zero;
        newTile.transform.localPosition = Vector3.zero;

        letterTiles.Add(newTile);

        return true;
    }

    public bool AddConsonant()
    {

        if (AtCapacity()) return false;

        LetterTile newTile = Instantiate(tileTemplate, Vector3.zero, Quaternion.identity).GetComponent<LetterTile>();
        int tries = 0;

        do
        {
            newTile.BuildConsonant();
        }
        while (GetCountOfChar(newTile.Letter.character) >= RepeatLimit() && tries < 10);


        newTile.name = newTile.Letter.character.ToString();

        newTile.transform.SetParent(tileHolder);
        newTile.transform.SetAsLastSibling();

        newTile.transform.localScale = Vector3.one;
        newTile.transform.localEulerAngles = Vector3.zero;
        newTile.transform.localPosition = Vector3.zero;

        letterTiles.Add(newTile);

        return true;
    }

    public bool AddTile(char character)
    {
        Debug.Log("Requested a tile with character: " + character);

        if (AtCapacity()) return false;

        LetterTile newTile = Instantiate(tileTemplate, Vector3.zero, Quaternion.identity).
        GetComponent<LetterTile>();
        
        int tries = 0;

        //You can get stuck here
        //do
        //{
            newTile.BuildFromCharacter(character);
        //}
        //while (GetCountOfChar(newTile.letter.character) >= RepeatLimit() && tries < 10);

        Debug.Log("Rack added a tile with character: " + character);

        newTile.name = character.ToString();

        newTile.transform.SetParent(tileHolder);
        newTile.transform.SetAsLastSibling();

        newTile.transform.localScale = Vector3.one;
        newTile.transform.localEulerAngles = Vector3.zero;
        newTile.transform.localPosition = Vector3.zero;

        letterTiles.Add(newTile);

        return true;
    }

    public bool EmptyRack()
    {
        foreach (LetterTile tile in letterTiles)
        {
            DestroyImmediate(tile.gameObject);
        }
        letterTiles.Clear();
        return true;
    }

    public bool FillRack(List<char> seed, int fillTo)
    {
        if (seed != null)
        {
            foreach (char character in seed)
            {
                AddTile(character);
            }
        }

        Debug.LogFormat($"TILE RACK - FILLED FROM {letterTiles.Count} TO {fillTo}");
        int diff = fillTo - letterTiles.Count;

        for (int i = 0; i < diff; i++)
        {
            AddRandomTile();
        }
        return true;
    }

    bool AtCapacity()
    {

        bool atCapacity = letterTiles.Count > BrainControl.Get().runManager.CurrentRun.ActiveLevel.Data.rackSize;
        if (atCapacity) BrainControl.Get().uiManager.PrintMessage("Your rack is at capacity");
        return atCapacity;
    }

    int RepeatLimit()
    {
        return BrainControl.Get().runManager.CurrentRun.ActiveLevel.Data.repeatLimit;
    }

    //Get the number of tiles of a specific character
    int GetCountOfChar(char _char)
    {
        int count = letterTiles.Where(x => x.Letter.character == _char).ToList().Count;
        Debug.Log("Requested character " + _char + " existing count of: " + count);
        return letterTiles.Where(x => x.Letter.character == _char).ToList().Count;

    }


}
