using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ClickAndHoldButton: CustomButton
{
    private Coroutine m_holdWait = null;
    
    public UnityEvent<bool> HoldEvent = new UnityEvent<bool>();
    
    [SerializeField]
    private float m_holdTime = 3f;
    
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        m_holdWait = StartCoroutine(WaitForHold(eventData));
    }
    
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        
        //If we were already waiting - cancel the wait
        if (m_holdWait != null)
        {
            m_holdWait = null;
        }
        
        //Ensure reverse any effect
        HoldEvent.Invoke(false);
    }

    private IEnumerator WaitForHold(PointerEventData eventData)
    {
        float t = 0;
        Debug.LogFormat($"Started waiting for hold.");

        yield return new WaitForEndOfFrame();
        
        while (m_holdWait != null)
        {
            t += Time.deltaTime;
            Debug.LogFormat($"Waiting for hold for {t} seconds.");
            
            if (t >= m_holdTime)
            {
                m_holdWait = null;
                HoldEvent.Invoke(true);
            }
            yield return null;
        }
        
        Debug.LogFormat($"Finished waiting for hold: {t} seconds.");
    }
}
