using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public ScoreSet scoreSet;

    void Start()
    {
        //Tile rack costs
        BrainControl.Get().eventManager.e_fillRack.AddListener((i, c) =>
        {
            if (c) BrainControl.Get().eventManager.e_juiceChange.Invoke(-scoreSet.fillCost);
        });
        BrainControl.Get().eventManager.e_newRack.AddListener((i, c) =>
        {
            if (c) BrainControl.Get().eventManager.e_juiceChange.Invoke(-scoreSet.newRackCost);
        });
        BrainControl.Get().eventManager.e_getTile.AddListener((l, c) =>
       {
           if (c) BrainControl.Get().eventManager.e_juiceChange.Invoke(-scoreSet.specificTileCost);
       });
        BrainControl.Get().eventManager.e_getConsonant.AddListener((c) =>
        {
            if (c) BrainControl.Get().eventManager.e_juiceChange.Invoke(-scoreSet.consonantCost);
        });
        BrainControl.Get().eventManager.e_getVowel.AddListener((c) =>
        {
            if (c) BrainControl.Get().eventManager.e_juiceChange.Invoke(-scoreSet.vowelCost);
        });

        BrainControl.Get().eventManager.e_validateFail.AddListener(() => BrainControl.Get().eventManager.e_juiceChange.Invoke(-scoreSet.validateFailPenalty));

        BrainControl.Get().eventManager.e_validateSuccess.AddListener((f) =>
        {

            int score = 0;

            foreach (BlockLine b in f.compiledWords)
            {

                string validated = GridTools.WordFromLine(b).forwards;

                Debug.Log("Compiled word set: " + GridTools.WordFromLine(b));

                //Figure out how much the compiled block line is worth
                int scoreAdd = ScoreFromBlocks(b);


                Debug.Log("Compiled word set is worth: " + scoreAdd);
                score += scoreAdd;
            }
            Debug.Log("Full input was worth: " + score);
            BrainControl.Get().eventManager.e_juiceChange.Invoke(score);


        });



        BrainControl.Get().eventManager.e_restartLevel.AddListener(() => BrainControl.Get().eventManager.e_juiceChange.Invoke(-scoreSet.levelRestartCost));


    }

    public int ScoreFromBlocks(BlockLine line)
    {

        int score = 0;

        foreach (LetterBlock block in line.blocks)
        {
            score += block.letter.score;

        }

        return (score * line.blocks.Count) / (1 + BrainControl.Get().sessionManager.currentSession.RecencyScore(GridTools.WordFromLine(line).forwards));

    }

}
