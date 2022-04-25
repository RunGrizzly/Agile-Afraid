using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InputModification { Confirm, Cancel }

public class InputModifier : MonoBehaviour
{
    public InputModification modification;

    Ray ray;
    RaycastHit hit;

    private bool isHovered;

    public float hoverPop;

    public void ApplyModification()
    {

        switch (modification)
        {

            default: return;
            case InputModification.Confirm:
                BrainControl.Get().eventManager.e_endInput.Invoke();
                Debug.Log("Input confirmed");
                break;

            case InputModification.Cancel:
                Debug.Log("Input cancelled");
                BrainControl.Get().eventManager.e_getTile.Invoke(BrainControl.Get().grid.selectedBlock.letter, false);
                BrainControl.Get().eventManager.e_clearBlock.Invoke(BrainControl.Get().grid.selectedBlock);
                break;
        }
    }


    void Update()
    {

        Debug.Log("Button: " + modification.ToString() + " is hovered: " + isHovered);

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject == this.gameObject)
            {
                if (isHovered == false)
                {
                    LeanTween.scale(gameObject, new Vector3(0.25f, 0.25f, 0.25f) * hoverPop, 0.25f).setEase(LeanTweenType.easeOutElastic);
                    isHovered = true;
                }

                if (Input.GetMouseButtonUp(0)) ApplyModification();
            }

            else
            {
                if (isHovered == true)
                {
                    LeanTween.scale(gameObject, new Vector3(0.25f, 0.25f, 0.25f), 0.35f).setEase(LeanTweenType.easeOutElastic);
                    isHovered = false;
                }
            }
        }

    }

}
