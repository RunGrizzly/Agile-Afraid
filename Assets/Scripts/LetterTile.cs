using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

[ExecuteAlways]
public class LetterTile : DraggableUI
{

    public Letter letter;

    public TextMeshProUGUI letterBox;
    public TextMeshProUGUI scoreBox;

    void Start()
    {
        //    BuildWeighted();
    }

    public void BuildFromLetter(char _letter)
    {
        // Debug.Log("Assigning random letter " + _letter + " to the tile");


        //Create a new copy of the letter entry found using the passed letters
        letter = new Letter(BrainControl.Get().scoreManager.scoreSet.scoreset.Keys.FirstOrDefault(x => x.character == _letter));

        letterBox.text = letter.character.ToString();
        scoreBox.text = letter.score.ToString();
    }

    public void BuildFromDistribution(float minDist, float maxDist)
    {

        //Create a new copy of the letter entry found using the random letter
        letter = BrainControl.Get().scoreManager.scoreSet.LetterFromDistribution(minDist, maxDist);

        letterBox.text = letter.character.ToString();
        scoreBox.text = letter.score.ToString();
    }

    public void BuildWeighted()
    {
        //Create a new copy of the letter entry found using the random letter
        letter = BrainControl.Get().scoreManager.scoreSet.WeightedLetter();

        letterBox.text = letter.character.ToString();
        scoreBox.text = letter.score.ToString();
    }


    public void BuildVowel()
    {
        //Create a new copy of the letter entry found using the random letter
        letter = BrainControl.Get().scoreManager.scoreSet.Vowel();

        letterBox.text = letter.character.ToString();
        scoreBox.text = letter.score.ToString();
    }


    public void BuildConsonant()
    {
        //Create a new copy of the letter entry found using the random letter
        letter = BrainControl.Get().scoreManager.scoreSet.Consonant();

        letterBox.text = letter.character.ToString();
        scoreBox.text = letter.score.ToString();
    }


    //When we begin to drag.
    public override void OnBeginDrag(PointerEventData eventData)
    {
        //Where the drag was initiated.
        startPos = gameObject.GetComponent<RectTransform>().localPosition;
    }

    //While dragging.
    public override void OnDrag(PointerEventData eventData)
    {
        //While dragging, pointer event data allows us to use the pointer data to set the UI elements position.
        gameObject.transform.position = eventData.position;
    }

    //When we end the drag.
    public override void OnEndDrag(PointerEventData eventData) //Once the image is let go.
    {
        LetterBlock highlightedBlock = BrainControl.Get().grid.highlightedBlock;


        if (highlightedBlock != null && highlightedBlock.lockState == LockState.unlocked)
        {
            //Control session input
            ///////////
            if (BrainControl.Get().sessionManager.currentSession.currentLevel.InputInProgress())
            {
                if (BrainControl.Get().sessionManager.currentSession.currentLevel.LatestInput().BlockIsPossible(highlightedBlock))
                {
                    BuildToBlock(highlightedBlock);
                    BrainControl.Get().eventManager.e_updateInput.Invoke(highlightedBlock);
                }
                else
                {
                    ReturnToRack();
                }
            }
            else
            {
                BuildToBlock(highlightedBlock);
                BrainControl.Get().eventManager.e_beginInput.Invoke(highlightedBlock);
            }
            ///////////
        }
        else
        {
            Debug.LogWarning("The requested block is not valid or is locked");
            ReturnToRack();
        }
    }

    void BuildToBlock(LetterBlock block)
    {
        block.BuildFromLetter(letter.character);
        BrainControl.Get().rack.letterTiles.Remove(this);
        DestroyImmediate(gameObject);
    }

    void ReturnToRack()
    {
        gameObject.transform.localPosition = startPos;
    }
}
