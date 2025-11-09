using Sirenix.OdinInspector;
using UnityEngine;

//This should be generified into a context aware raycasting and selection system

[ExecuteAlways]
public class MainMenuController : MonoBehaviour
{
    [ReadOnly]
    public IGeoButton SelectedObject = null;

    [ReadOnly]
    public IGeoButton HighlightedObject = null;

    [SerializeField]
    private ParticleSystem m_transitionBurst = null;

    private Ray ray;
    private RaycastHit hit;

    private void Update()
    {
        if (Camera.main == null)
        {
            return;
        }

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            IGeoButton hitObject = hit.collider.GetComponent<IGeoButton>();

            if (hitObject != null)
            {
                Highlight(hitObject);

                //While highlighted - if we click - make this the selected block
                if (Input.GetMouseButtonUp(0))
                {
                    Select(HighlightedObject);
                }
            }
            else
            {
                Unhilight();
            }
        }
        else
        {
            Unhilight();
        }
    }

    public void Highlight(IGeoButton newObject)
    {
        // Already highlighted, ignore   
        if (newObject == HighlightedObject)
        {
            return;
        }

        // Unhighlight previous if it exists
        if (HighlightedObject != null)
        {
            Unhilight();
        }

        // Highlight new one
        HighlightedObject = newObject;
        HighlightedObject?.SetHovered.Invoke();
    }

    public void Select(IGeoButton newObject)
    {
        SelectedObject = newObject;
        SelectedObject.SetSelected.Invoke();
        m_transitionBurst.gameObject.SetActive(false);
        m_transitionBurst.gameObject.SetActive(true);
    }

    public void Unhilight()
    {
        if (HighlightedObject != null)
        {
            HighlightedObject.SetUnhovered.Invoke();
            HighlightedObject = null;
        }
    }
}
