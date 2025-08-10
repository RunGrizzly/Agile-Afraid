using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine.Serialization;

[ExecuteAlways]
public class LetterTile : DraggableUI
{

    public Letter Letter;

    public TextMeshProUGUI letterBox;
    public TextMeshProUGUI scoreBox;

    void Start()
    {
        //    BuildWeighted();
    }

    public void BuildFromCharacter(char character)
    {
        // Debug.Log("Assigning random letter " + _letter + " to the tile");


        //Create a new copy of the letter entry found using the passed letters
        Letter = new Letter(BrainControl.Get().runManager.CurrentRun.RunSettings.ActiveScoringRubrik.distribution.Keys.FirstOrDefault(x => char.ToLower(x.character) ==char.ToLower(character)));

        letterBox.text = Letter.character.ToString();
        scoreBox.text = Letter.score.ToString();
    }

    public void BuildFromDistribution(float minDist, float maxDist)
    {

        //Create a new copy of the letter entry found using the random letter
        Letter = BrainControl.Get().runManager.CurrentRun.RunSettings.ActiveScoringRubrik.LetterFromDistribution(minDist, maxDist);

        letterBox.text = Letter.character.ToString();
        scoreBox.text = Letter.score.ToString();
    }

    public void BuildWeighted()
    {
        //Create a new copy of the letter entry found using the random letter
        Letter = BrainControl.Get().runManager.CurrentRun.RunSettings.ActiveScoringRubrik.WeightedRandom();

        letterBox.text = Letter.character.ToString();
        scoreBox.text = Letter.score.ToString();
    }


    public void BuildVowel()
    {
        //Create a new copy of the letter entry found using the random letter
        Letter = BrainControl.Get().runManager.CurrentRun.RunSettings.ActiveScoringRubrik.Vowel();

        letterBox.text = Letter.character.ToString();
        scoreBox.text = Letter.score.ToString();
    }


    public void BuildConsonant()
    {
        //Create a new copy of the letter entry found using the random letter
        Letter =BrainControl.Get().runManager.CurrentRun.RunSettings.ActiveScoringRubrik.Consonant();

        letterBox.text = Letter.character.ToString();
        scoreBox.text = Letter.score.ToString();
    }


    //When we begin to drag.
    public override void OnBeginDrag(PointerEventData eventData)
    {
        //Where the drag was initiated.
        startPos = gameObject.GetComponent<RectTransform>().localPosition;
    }
    
    // public override void OnBeginDrag(PointerEventData eventData)
    // {
    //     Vector2 localPoint;
    //     
    //     RectTransformUtility.ScreenPointToLocalPointInRectangle(
    //         GameObject.FindGameObjectWithTag("Canvas").GetComponent<RectTransform>(), 
    //         Input.mousePosition, 
    //         GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>().worldCamera,
    //         out localPoint
    //     );
    // }

    //While dragging.
    public override void OnDrag(PointerEventData eventData)
    {
        //While dragging, pointer event data allows us to use the pointer data to set the UI elements position.
        //gameObject.transform.position = eventData.position;
        
        Vector2 localPoint;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>().parent.GetComponent<RectTransform>(), 
            eventData.position, 
            eventData.pressEventCamera,
            out localPoint
        );
        
        
        
        //While dragging, pointer event data allows us to use the pointer data to set the UI elements position.
        GetComponent<RectTransform>().localPosition = new Vector3(localPoint.x, localPoint.y, 0);

    }

    //When we end the drag.
    public override void OnEndDrag(PointerEventData eventData) //Once the image is let go.
    {
        LetterBlock highlightedBlock = BrainControl.Get().grid.highlightedBlock;


        if (highlightedBlock != null && highlightedBlock.lockState == LockState.unlocked)
        {
            //Control session input
            ///////////
            if (BrainControl.Get().runManager.CurrentRun.ActiveLevel.InputInProgress())
            {
                if (BrainControl.Get().runManager.CurrentRun.ActiveLevel.LatestInput().BlockIsPossible(highlightedBlock))
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
        block.BuildTokenised(Letter.character.ToString());
        BrainControl.Get().rack.letterTiles.Remove(this);
        DestroyImmediate(gameObject);
    }

    void ReturnToRack()
    {
        gameObject.transform.localPosition = startPos;
    }
}
