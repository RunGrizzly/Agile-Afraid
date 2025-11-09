using UnityEngine;

public enum InputModification
{
    Confirm,
    Cancel
}

public class InputModificationWidget : MonoBehaviour
{
    public LetterBlock TargetBlock = null;
    public InputModifier ConfirmModificationWidget = null;
    public InputModifier CancelModificationWidget = null;

    private BlockInput AssignedInput = null;
    
    Ray ray;
    RaycastHit hit;
    
    public float hoverPop;

    public void OnEnable()
    {
        AssignedInput = BrainControl.Get().runManager.CurrentRun.ActiveLevel.InputInProgress();
        
        BrainControl.Get().eventManager.e_blockSelected.AddListener(AssignTarget);

        BrainControl.Get().eventManager.e_blockSelected.AddListener(Show);

        BrainControl.Get().eventManager.e_beginInput.AddListener(Show);

        BrainControl.Get().eventManager.e_updateInput.AddListener(AssignTarget);

        BrainControl.Get().eventManager.e_confirmInput.AddListener(Clear);

        BrainControl.Get().eventManager.e_clearBlock.AddListener(Clear);
    }

    public void OnDisable()
    {
        BrainControl.Get().eventManager.e_blockSelected.RemoveListener(AssignTarget);

        BrainControl.Get().eventManager.e_blockSelected.RemoveListener(Show);

        BrainControl.Get().eventManager.e_beginInput.RemoveListener(Show);

        BrainControl.Get().eventManager.e_updateInput.RemoveListener(AssignTarget);

        BrainControl.Get().eventManager.e_confirmInput.RemoveListener(Clear);

        BrainControl.Get().eventManager.e_clearBlock.RemoveListener(Clear);
    }

    public void AssignTarget(LetterBlock newTarget)
    {
        TargetBlock = newTarget;
        transform.position = TargetBlock.transform.position;
    }

    public void Show(LetterBlock targetBlock)
    {
    }

    public void Clear(LetterBlock letterBlock)
    {
    }

    public void Clear()
    {
    }
    
    void Update()
    {
        //Debug.Log("Button: " + modification.ToString() + " is hovered: " + isHovered);

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject != null && hit.collider.GetComponent<InputModifier>())
            {
                // if (isHovered == false)
                // {
                //     LeanTween.scale(gameObject, new Vector3(0.25f, 0.25f, 0.25f) * hoverPop, 0.25f).setEase(LeanTweenType.easeOutElastic);
                //     isHovered = true;
                // }

                if (Input.GetMouseButtonUp(0))
                {
                    //Debug.LogFormat("Mouse button up.");
                  var m = hit.collider.GetComponent<InputModifier>();

                    switch (m.modification)
                    {
                        case InputModification.Confirm:
                            BrainControl.Get().eventManager.e_confirmInput.Invoke();
                            //Debug.Log("Input confirmed");
                            Destroy(gameObject);
                            break;
                        case InputModification.Cancel:
                            AssignedInput.RemoveFromInput(TargetBlock);
                            Destroy(gameObject);
                            break;
                    }
                }
            }

            else
            {
                // if (isHovered == true)
                // {
                //     LeanTween.scale(gameObject, new Vector3(0.25f, 0.25f, 0.25f), 0.35f).setEase(LeanTweenType.easeOutElastic);
                //     isHovered = false;
                // }
            }
        }
    }
}