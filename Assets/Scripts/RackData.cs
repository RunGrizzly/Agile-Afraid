using System;
using System.Collections.Generic;

[Serializable]
public class RackData
{
    public List<Letter> Letters = new List<Letter>();
    private int m_capacity = 8;
    
    public RackData(int capacity)
    {
        m_capacity = capacity;
    }
    
    public void Add(Letter letter)
    {
        Letter newLetter = new Letter(letter);
        Letters.Add(newLetter);

        BrainControl.Get().eventManager.e_addedToRack.Invoke(newLetter);
    }

    public void Remove(Letter letter)
    {
        Letters.Remove(letter);
        
        BrainControl.Get().eventManager.e_removedFromRack.Invoke(letter);
    }

    public void Empty()
    {
        Letters.Clear();
        BrainControl.Get().eventManager.e_rackEmptied.Invoke();
    }

    public void GetNew(ScoreSet rubrik)
    {
        Empty();
        Fill(null, rubrik);
    }

    public void Fill(List<Letter> seed, ScoreSet rubrik)
    {
        if (seed != null)
        {
            foreach (Letter letter in seed)
            {
                //When we create a new letter we don't have a clean way to hook up the correct score
                // Letter newLetter = new Letter(character, 0);
                Add(letter);
            }
        }

        // Debug.LogFormat($"TILE RACK - FILLED FROM {letterTiles.Count} TO {fillTo}");
        int diff = m_capacity - Letters.Count;

        for (int i = 0; i < diff; i++)
        {
            //Add a random letter from the rubrik
            Add(rubrik.WeightedRandom());
        }
        
        // //We filled the rack - secondary broadcast
        // BrainControl.Get().eventManager.e_fillRackRequest.Invoke(10, true, true);
    }
  
    
    public bool AtCapacity()
    {
    
        bool atCapacity = Letters.Count >= m_capacity;
        
        if (atCapacity)
        {
            BrainControl.Get().uiManager.PrintMessage("Your rack is at capacity");
        }
        
        return atCapacity;
    }
    
    int RepeatLimit()
    {
        return BrainControl.Get().runManager.CurrentRun.ActiveLevel.Data.repeatLimit;
    }
    //
    // //Get the number of tiles of a specific character
    // int GetCountOfChar(char _char)
    // {
    //     int count = letterTiles.Where(x => x.Letter.character == _char).ToList().Count;
    //     Debug.Log("Requested character " + _char + " existing count of: " + count);
    //     return letterTiles.Where(x => x.Letter.character == _char).ToList().Count;
    //
    // }
    
    
    
}