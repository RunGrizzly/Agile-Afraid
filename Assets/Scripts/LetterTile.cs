using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

[ExecuteAlways]
public class LetterTile : DraggableUI, ILetterDestination
{
    public Letter Letter;

    public TextMeshProUGUI letterBox;
    public TextMeshProUGUI scoreBox;

    [ShowInInspector]
    [ReadOnly]
    private ILetterDestination m_destination = null;
    
    [Button]
    public void Build(Letter letter)
    {
        Debug.LogFormat($"Letter tile received instruction to build a {letter.character}");
        Debug.LogFormat($"As a string this is {letter.character.ToString()}");
        
        Letter = letter;
        
        letterBox.text = Letter.character.ToString();
        scoreBox.text = Letter.score.ToString(); 
        
        Debug.LogFormat($"Letterbox text is now {letterBox.text}");
    }

    //This should be a generic letter destination
    private void SetDestination(ILetterDestination destination)
    {
        m_destination = destination;
    }
    
    //When we begin to drag.
    public override void OnBeginDrag(PointerEventData eventData)
    {
        //Where the drag was initiated.
        m_startPos = gameObject.GetComponent<RectTransform>().localPosition;

        //BrainControl.Get().eventManager.e_blockHighlighted.AddListener(SetDestination);
    }
    
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
        
        FindDestination(eventData);
    }

    //When we end the drag.
    //We should have a generic (tile target) thing that has a "send tile" method that
    //Does some transformation or process
    public override void OnEndDrag(PointerEventData eventData) //Once the image is let go.
    {
        if (m_destination != null)
        {
            if (m_destination.IsValid())
            {
                //It shouldn't be able to fail at this point
                m_destination.SendLetter(Letter);
                BrainControl.Get().Rack.RackData.Remove(Letter);
            }
            else
            {
                Debug.LogWarning("Destination is not valid");
                ReturnToRack();
            }
        }
        else
        {
            Debug.LogWarning("Destination is null");
            ReturnToRack();
        }
    }
    
    private void ReturnToRack()
    {
        gameObject.transform.localPosition = m_startPos;
    }
    
    public bool IsValid()
    {
        return true;
    }
    
    public void SendLetter(Letter letter)
    {
       //Send a letter to this tile
    }
    
    private void FindDestination(PointerEventData pointerData)
    {
        //Find UI Targets
        var uiResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, uiResults);
        
        for (int i = 0; i < uiResults.Count; i++)
        {
            ILetterDestination result = uiResults[i].gameObject.GetComponent<ILetterDestination>();
            
            if (result != null)
            {
                Debug.LogFormat($"Found UI result {result}");
                if (result is MonoBehaviour mono)
                {
                    if (mono is not LetterTile)
                    {
                        Debug.LogFormat(mono.gameObject,$"Found a valid UI destination {mono.name}");
                        SetDestination(result);
                        return;
                    }
                }
            }
        }

        //Find world Targets
        var worldResults = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 200f);
        
        for (int i = 0; i < worldResults.Length; i++)
        {
            ILetterDestination result = worldResults[i].transform.GetComponent<ILetterDestination>();
        
            if (result != null)
            {
                Debug.LogFormat($"Found world result {result}");
                if (result is MonoBehaviour mono)
                {
                        Debug.LogFormat(mono.gameObject,$"Found a valid UI destination {mono.name}");
                        SetDestination(result);
                        return;
                }
            }
        }
        
        SetDestination(null);
    }
}
