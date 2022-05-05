using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class TileRack : MonoBehaviour
{

    public GameObject tileTemplate;
    public Transform tileHolder;
    public List<LetterTile> letterTiles = new List<LetterTile>();

    void Start()
    {
        BrainControl.Get().eventManager.e_getTile.AddListener((l, c) => _ = AddTile(l.character));
        BrainControl.Get().eventManager.e_getVowel.AddListener((c) => _ = AddVowel());
        BrainControl.Get().eventManager.e_getConsonant.AddListener((c) => _ = AddConsonant());
        BrainControl.Get().eventManager.e_emptyRack.AddListener(() => _ = EmptyRack());
        BrainControl.Get().eventManager.e_newRack.AddListener((i, c) =>
        {
            _ = EmptyRack();
            _ = FillRack(i);
        });
        BrainControl.Get().eventManager.e_fillRack.AddListener((i, c) =>
        {
            _ = FillRack(i);
        });
    }

    public bool AddRandomTile()
    {

        if (AtCapacity()) return false;

        LetterTile newTile = Instantiate(tileTemplate, Vector3.zero, Quaternion.identity).GetComponent<LetterTile>();
        newTile.transform.localScale = Vector3.one;
        newTile.transform.localEulerAngles = Vector3.zero;
        newTile.transform.localPosition = Vector3.zero;

        int tries = 0;

        do
        {
            newTile.BuildWeighted();
            tries += 1;
        }
        while (GetCountOfChar(newTile.letter.character) >= RepeatLimit() && tries < 10);

        newTile.name = newTile.letter.character.ToString();

        newTile.transform.SetParent(tileHolder);

        letterTiles.Add(newTile);

        return true;

    }

    public bool AddVowel()
    {

        if (AtCapacity()) return false;

        LetterTile newTile = Instantiate(tileTemplate, Vector3.zero, Quaternion.identity).GetComponent<LetterTile>();
        newTile.transform.localScale = Vector3.one;
        newTile.transform.localEulerAngles = Vector3.zero;
        newTile.transform.localPosition = Vector3.zero;

        int tries = 0;

        do
        {
            newTile.BuildVowel();
        }
        while (GetCountOfChar(newTile.letter.character) >= RepeatLimit() && tries < 10);

        newTile.name = newTile.letter.character.ToString();

        newTile.transform.SetParent(tileHolder);

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
        while (GetCountOfChar(newTile.letter.character) >= RepeatLimit() && tries < 10);


        newTile.name = newTile.letter.character.ToString();

        newTile.transform.SetParent(tileHolder);

        letterTiles.Add(newTile);

        return true;
    }

    public bool AddTile(char _character)
    {

        Debug.Log("Requested a tile with character: " + _character);

        if (AtCapacity()) return false;

        LetterTile newTile = Instantiate(tileTemplate, Vector3.zero, Quaternion.identity).
        GetComponent<LetterTile>();
        newTile.transform.localScale = Vector3.one;
        newTile.transform.localEulerAngles = Vector3.zero;
        newTile.transform.localPosition = Vector3.zero;



        int tries = 0;

        do
        {
            newTile.BuildFromLetter(_character);
        }
        while (GetCountOfChar(newTile.letter.character) >= RepeatLimit() && tries < 10);

        Debug.Log("Rack added a tile with characte: " + newTile.letter.character);

        newTile.name = newTile.letter.character.ToString();

        newTile.transform.SetParent(tileHolder);

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

    public bool FillRack(int fillTo)
    {

        int diff = fillTo - letterTiles.Count;

        for (int i = 0; i < diff - 1; i++)
        {
            AddRandomTile();
        }
        return true;
    }

    bool AtCapacity()
    {

        bool atCapacity = letterTiles.Count >= BrainControl.Get().sessionManager.currentSession.currentLevel.data.rackSize;
        if (atCapacity) BrainControl.Get().uiManager.PrintMessage("Your rack is at capacity");
        return atCapacity;
    }

    int RepeatLimit()
    {
        return BrainControl.Get().sessionManager.currentSession.currentLevel.data.repeatLimit;
    }

    //Get the number of tiles of a specific character
    int GetCountOfChar(char _char)
    {
        int count = letterTiles.Where(x => x.letter.character == _char).ToList().Count;
        Debug.Log("Requested character " + _char + " existing count of: " + count);
        return letterTiles.Where(x => x.letter.character == _char).ToList().Count;

    }


}
