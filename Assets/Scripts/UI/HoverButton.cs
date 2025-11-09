using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HoverButton: CustomButton, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent HoverEvent = new UnityEvent();

    private bool m_isHovered = false;

    private Coroutine m_waitForPointerUp = null;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        m_isHovered = true;

        if (m_waitForPointerUp != null)
        {
            StopCoroutine(m_waitForPointerUp);            
        }
        
        m_waitForPointerUp = StartCoroutine(WaitForPointerUp());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_isHovered = false;
    }

    private IEnumerator WaitForPointerUp()
    {
        while (m_isHovered)
        {
            if (Input.GetMouseButtonUp(0))
            {
                HoverEvent.Invoke();
                m_isHovered = false;
            }
            
            yield return null;
        }
    }
}