using UnityEngine;
using System.Collections;


public class BlockSelector : MonoBehaviour
{
    Ray ray;
    RaycastHit hit;

    void Update()
    {

        if (BrainControl.Get().sessionManager.currentSession == null) return;
        if (BrainControl.Get().sessionManager.currentSession.isPaused) return;


        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {

            GridGenerator grid = BrainControl.Get().grid;

            if (hit.collider.GetComponent<LetterBlock>())
            {
                //Cache this bullshit damn
                grid.HighlightBlock(hit.collider.GetComponent<LetterBlock>());

                if (Input.GetMouseButtonUp(0))
                {
                    BrainControl.Get().eventManager.e_blockSelected.Invoke(hit.collider.GetComponent<LetterBlock>());
                }

            }
            else grid.Unhilight();
        }
    }
}