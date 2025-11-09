using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class CustomButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [SerializeField]
    protected Button SourceButton = null; 
    
    public UnityEvent ClickButton = new UnityEvent();
    
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        Debug.LogFormat(gameObject,$"Custom button triggered pointer down");
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        Debug.LogFormat(gameObject,$"Custom button triggered pointer up");
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
       Debug.LogFormat(gameObject,$"Custom button triggered click");
       ClickButton.Invoke();
    }
}