using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


//Activated slots have a draggable UI component activated.
public class DraggableUI : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{


    public Vector3 startPos;

    //When we begin to drag.
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
    }

    //While dragging.
    public virtual void OnDrag(PointerEventData eventData)
    {
    }

    //When we end the drag.
    public virtual void OnEndDrag(PointerEventData eventData) //Once the image is let go.
    {
    }

}