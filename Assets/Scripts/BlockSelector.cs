using UnityEngine;
using System.Collections;
using CodingJar;
using Sirenix.OdinInspector;


public class BlockSelector : MonoBehaviour
{
    private Ray ray;
    private RaycastHit hit;

    private RunManager m_runManager = null;
    private GridGenerator m_grid = null;
    
    [Readonly]
    public LetterBlock m_highlightedBlock;
    
    [Readonly]
    public LetterBlock m_selectedBlock;
    
    private float selectPop;
    
    private void Start()
    {
        m_runManager = BrainControl.Get().runManager;
        m_grid = BrainControl.Get().Grid;
    }
    
    private void Update()
    {
        if (m_selectedBlock != null && m_selectedBlock.fillState == FillState.empty)
        {
            m_selectedBlock = null;
        }
        
        if (m_runManager.Runs.Count == 0)
        {
            return;
        }
        
        if (m_runManager.CurrentRun.IsPaused)
        {
            return;
        }
        //Figure out what the current selection state is
        
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out hit))
        {
            LetterBlock highlightedBlock = hit.collider.GetComponent<LetterBlock>();
            
            if (highlightedBlock != null)
            {
                Highlight(highlightedBlock);

                //While highlighted - if we click - make this the selected block
                if (Input.GetMouseButtonUp(0))
                {
                  SelectBlock(highlightedBlock);
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
    
    public void Highlight(LetterBlock highlightedBlock)
    {
        //Debug.LogFormat($"Trying to highlight {highlightedBlock.gridRef}");
        //Debug.LogFormat($"Currently highlighted block is {m_highlightedBlock.gridRef}");
        
        // if (highlightedBlock == m_highlightedBlock)
        // {
        //     Debug.LogFormat($"The highlighted block is already highlighted");
        //     return;
        // }

        //If there is already a highlighted block
        //Unhighlight it
        if (m_highlightedBlock != null && m_highlightedBlock != highlightedBlock)
        {
            Unhilight();
        }
        
        m_highlightedBlock = highlightedBlock;
        m_highlightedBlock.MeshRenderer.gameObject.GetComponent<MeshRenderer>().material.SetInt("_isHighlighted", 1);
        LeanTween.moveLocalY(m_highlightedBlock.MeshRenderer.gameObject, selectPop, 0.15f).setEase(LeanTweenType.easeOutElastic);
        BrainControl.Get().eventManager.e_blockHighlighted.Invoke(m_highlightedBlock);
    }
    

    public void Unhilight()
    {
        if (m_highlightedBlock == null)
        {
            //Debug.LogFormat($"Tried to unhighlight with no actively highlighted block");
            return;
        }

        else
        {
            //Debug.LogFormat($"Unhilighting {m_highlightedBlock.gridRef}");
            
            m_highlightedBlock.MeshRenderer.gameObject.GetComponent<MeshRenderer>().material.SetInt("_isHighlighted", 0);
            LeanTween.moveLocalY(m_highlightedBlock.MeshRenderer.gameObject, 0, 0.15f).setEase(LeanTweenType.easeOutExpo);
     
            m_highlightedBlock = null;
            BrainControl.Get().eventManager.e_blockHighlighted.Invoke(m_highlightedBlock);
        }
    }

    public void SelectBlock(LetterBlock selectedBlock)
    {
        if(selectedBlock.fillState == FillState.filled && selectedBlock.lockState == LockState.unlocked)
        {
            m_selectedBlock = selectedBlock;
            BrainControl.Get().eventManager.e_blockSelected.Invoke(m_selectedBlock);
        }
    }
}