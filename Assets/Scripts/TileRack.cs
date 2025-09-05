using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TileRack : MonoBehaviour
{
    public LetterTile TileTemplate;
    public Transform TileHolder;

    //We assign this data to this object dynamically via run events
    public RackData RackData = null;

    //Matching letters from the rack data to spawned letter tiles
    public SerializableDictionary<Letter, LetterTile> LetterTiles = new SerializableDictionary<Letter, LetterTile>();

    private void OnEnable()
    {
        BrainControl.Get().eventManager.e_addedToRack.AddListener(AddTile);
        BrainControl.Get().eventManager.e_removedFromRack.AddListener(RemoveTile);
        BrainControl.Get().eventManager.e_rackEmptied.AddListener(Refresh);
        BrainControl.Get().eventManager.e_newRun.AddListener(OnNewRun);
    }
    
    private void OnDisable()
    {
        BrainControl.Get().eventManager.e_addedToRack.RemoveListener(AddTile);
        BrainControl.Get().eventManager.e_removedFromRack.RemoveListener(RemoveTile);
        BrainControl.Get().eventManager.e_rackEmptied.RemoveListener(Refresh);
        BrainControl.Get().eventManager.e_newRun.RemoveListener(OnNewRun);
    }

    void Start()
    {
        // BrainControl.Get().eventManager.e_getTile.AddListener((letter, costScore,costTime) => _ = AddTile(letter.character));
        // BrainControl.Get().eventManager.e_getVowelRequest.AddListener((costsScore,costsTime) => _ = AddVowel());
        // BrainControl.Get().eventManager.e_getConsonantRequest.AddListener((costsScore,costsTime) => _ = AddConsonant());
        // BrainControl.Get().eventManager.e_emptyRack.AddListener(() => _ = EmptyRack());
        //
        // BrainControl.Get().eventManager.e_newRackRequest.AddListener(    (seedCharacters,fillTo, costsScore, costsTime) =>
        // {
        //     _ = EmptyRack();
        //     _ = FillRack(seedCharacters,fillTo);
        // });
        //
        // BrainControl.Get().eventManager.e_fillRackRequest.AddListener((fillTo, costsScore, costsTime) =>
        // {
        //     _ = FillRack(null,fillTo);
        // });
    }

    private void OnNewRun(Run newRun)
    {
        RackData = newRun.RackData;
        Refresh();
    }
    
    public void Refresh()
    {
        //Study the rack data and refresh the visuals
        //Should it be hierachy/index based?
        //I think so

        //So they will be spawned from Letters 0 to Letters Count
        //They will be spawned in order, in the hierarchy
        //So the layout group should spawn them L to R
        //0|1|2|3|4|5|6|7|8
        
        //If we want to change the order we can just change the list indexing
        //ex - we swap tiles 3 and 5
        //0|1|2|5|4|3|6|7|8

        foreach (KeyValuePair<Letter, LetterTile> v in LetterTiles)
        {
            Destroy(v.Value.gameObject);
        }
        
        LetterTiles.Clear();
        
        for (int i = 0; i < RackData.Letters.Count; i++)
        {
            Letter newLetter = RackData.Letters[i];
            
            LetterTile newTile = Instantiate(TileTemplate, Vector3.zero, Quaternion.identity);
            newTile.Build(newLetter);
            
            newTile.transform.SetParent(TileHolder);
            // newTile.transform.SetAsLastSibling();
            newTile.transform.localScale = Vector3.one;
            newTile.transform.localEulerAngles = Vector3.zero;
            newTile.transform.localPosition = Vector3.zero;
            
            LetterTiles.Add(newLetter,newTile);
        }
    }

    private void AddTile(Letter letter)
    {
        //We generate a new instance to prevent key confusion and easy identification
        //Letter newLetter = new Letter(letter);
        
        Debug.LogFormat($"Add a tile to the rack with letter {letter.character}");
        
        LetterTile newTile = Instantiate(TileTemplate, Vector3.zero, Quaternion.identity);
        newTile.Build(letter);
        
        newTile.transform.SetParent(TileHolder);
        newTile.transform.SetAsLastSibling();
        newTile.transform.localScale = Vector3.one;
        newTile.transform.localEulerAngles = Vector3.zero;
        newTile.transform.localPosition = Vector3.zero;
        
        LetterTiles.Add(letter,newTile);
    }
    private void AddTileAtIndex(Letter letter, int index)
    {
        //We generate a new instance to prevent key confusion and easy identification
        Letter newLetter = new Letter(letter);
        
        LetterTile newTile = Instantiate(TileTemplate, Vector3.zero, Quaternion.identity);
        newTile.Build(newLetter);
        
        newTile.transform.SetParent(TileHolder);
        newTile.transform.SetSiblingIndex(index);
        newTile.transform.localScale = Vector3.one;
        newTile.transform.localEulerAngles = Vector3.zero;
        newTile.transform.localPosition = Vector3.zero;
        
        LetterTiles.Add(newLetter,newTile);
    }
    
    //Set the index and rack position of a tile
    //If the tile would overwrite and existing tile at that index, swap it
    public void ModifyTileIndex(int oldIndex, int newIndex)
    {
        
    }

    private void RemoveTile(Letter letter)
    {
        if (LetterTiles.TryGetValue(letter, out LetterTile tile) && tile != null)
        {
            Destroy(tile.gameObject); // Destroy the GameObject
            LetterTiles.Remove(letter);   // Remove the dictionary entry
        }
    }
    
    private void RemoveAtIndex(int index)
    {
        
    }

    // public bool AddRandomTile()
    // {
    //     if (AtCapacity())
    //     {
    //         Debug.LogWarningFormat($"The tile rack is at capacity. You cannot add a new tile.");
    //         return false;
    //     }
    //
    //     //Add a new blank tile 
    //     LetterTile newTile = Instantiate(tileTemplate, Vector3.zero, Quaternion.identity);
    //
    //     Letter newLetter = null;
    //     int tries = 0;
    //
    //     do
    //     {
    //         //Separate concern
    //         
    //         //We get a letter from the scoring Rubrik
    //         newLetter = BrainControl.Get().runManager.CurrentRun.ActiveLevel.Data.ScoringRubrik.WeightedRandom();
    //         
    //         //Here we build on the tile itself
    //         newTile.Build(newLetter);
    //         tries += 1;
    //     }
    //     while (GetCountOfChar(newTile.Letter.character) >= RepeatLimit() && tries < 10);
    //
    //     // newTile.name = newTile.Letter.character.ToString();
    //
    //     newTile.transform.SetParent(tileHolder);
    //     newTile.transform.SetAsLastSibling();
    //     newTile.transform.localScale = Vector3.one;
    //     newTile.transform.localEulerAngles = Vector3.zero;
    //     newTile.transform.localPosition = Vector3.zero;
    //
    //     // //I want THIS to drive the add
    //     // //So it would already be added by this point
    //     // RackData.Letters.Add(newLetter);
    //
    //     return true;
    //
    // }
    //
    // public bool AddVowel()
    // {
    //
    //     if (AtCapacity()) return false;
    //
    //     LetterTile newTile = Instantiate(tileTemplate, Vector3.zero, Quaternion.identity).GetComponent<LetterTile>();
    //
    //     int tries = 0;
    //
    //     do
    //     {
    //         newTile.BuildVowel();
    //     }
    //     while (GetCountOfChar(newTile.Letter.character) >= RepeatLimit() && tries < 10);
    //
    //     newTile.name = newTile.Letter.character.ToString();
    //
    //     newTile.transform.SetParent(tileHolder);
    //     newTile.transform.SetAsLastSibling();
    //
    //     newTile.transform.localScale = Vector3.one;
    //     newTile.transform.localEulerAngles = Vector3.zero;
    //     newTile.transform.localPosition = Vector3.zero;
    //
    //     letterTiles.Add(newTile);
    //
    //     return true;
    // }
    //
    // public bool AddConsonant()
    // {
    //
    //     if (AtCapacity()) return false;
    //
    //     LetterTile newTile = Instantiate(tileTemplate, Vector3.zero, Quaternion.identity).GetComponent<LetterTile>();
    //     int tries = 0;
    //
    //     do
    //     {
    //         newTile.BuildConsonant();
    //     }
    //     while (GetCountOfChar(newTile.Letter.character) >= RepeatLimit() && tries < 10);
    //
    //
    //     newTile.name = newTile.Letter.character.ToString();
    //
    //     newTile.transform.SetParent(tileHolder);
    //     newTile.transform.SetAsLastSibling();
    //
    //     newTile.transform.localScale = Vector3.one;
    //     newTile.transform.localEulerAngles = Vector3.zero;
    //     newTile.transform.localPosition = Vector3.zero;
    //
    //     letterTiles.Add(newTile);
    //
    //     return true;
    // }

    // public bool AddTile(char character)
    // {
    //     Debug.Log("Requested a tile with character: " + character);
    //
    //     if (AtCapacity()) return false;
    //
    //     LetterTile newTile = Instantiate(tileTemplate, Vector3.zero, Quaternion.identity).
    //     GetComponent<LetterTile>();
    //     
    //     //int tries = 0;
    //
    //     //You can get stuck here
    //     //do
    //     //{
    //         newTile.BuildFromCharacter(character);
    //     //}
    //     //while (GetCountOfChar(newTile.letter.character) >= RepeatLimit() && tries < 10);
    //
    //     Debug.Log("Rack added a tile with character: " + character);
    //
    //     newTile.name = character.ToString();
    //
    //     newTile.transform.SetParent(tileHolder);
    //     newTile.transform.SetAsLastSibling();
    //
    //     newTile.transform.localScale = Vector3.one;
    //     newTile.transform.localEulerAngles = Vector3.zero;
    //     newTile.transform.localPosition = Vector3.zero;
    //
    //     letterTiles.Add(newTile);
    //
    //     return true;
    // }

    
}
