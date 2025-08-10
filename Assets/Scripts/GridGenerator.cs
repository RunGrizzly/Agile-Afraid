using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;
using Sirenix.OdinInspector;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEngine.UI;

public class GridGenerator : MonoBehaviour
{
    public LetterBlock[,] letterBlocks = new LetterBlock[,] { };

    public LetterBlock blockTemplate;

    public LetterBlock highlightedBlock;
    public LetterBlock selectedBlock;

    public float selectPop;

    public CinemachineTargetGroup targetGroup;

    public NavMeshSurface navSurface;

    public Vector3 startPosition = Vector3.zero, targetPosition = new Vector3(100, 100, 100);

    public GameObject deadZone;

    public NavMeshPath path;

    public GUISkin editorSkin;

    private LevelRequirements m_activeRequirements;
    
    
    void Start()
    {

        BrainControl.Get().eventManager.e_levelLoaded.AddListener((l) =>
        {
            var deadZoneRenderer = deadZone.GetComponent<MeshRenderer>(); 
           deadZoneRenderer.material.SetColor("_ColorA", l.Data.DeadZoneColorA);
           deadZoneRenderer.material.SetColor("_ColorB", l.Data.DeadZoneColorB);
        });

        //Level level event responses
        /////////////////////////////
        Brain.ins.eventManager.e_blockSelected.AddListener((b) =>
        {
            if (b.lockState != LockState.locked) SelectBlock(b);
        });

        Brain.ins.eventManager.e_clearBlock.AddListener((c) =>
        {
            c.Empty();
            c.SetLockState(LockState.unlocked);
            selectedBlock = null;
        });

        //Track input mode
        Brain.ins.eventManager.e_beginInput.AddListener((a) =>
        {
            SelectBlock(a);
        });

        Brain.ins.eventManager.e_updateInput.AddListener((u) =>
        {
            SelectBlock(u);
        });
        
        //When an input is validated
        Brain.ins.eventManager.e_validateSuccess.AddListener((BlockInput input) =>
        {
            //So we made a complete path
            if (CheckPath())
            {
                //Continue to check the challenges
                if (ValidateChallenges())
                {
                    //We completed the grid
                    BrainControl.Get().runManager.CurrentRun.ActiveLevel.Complete();
                }
                //The path was completed but the challenges failed
                else
                {
                    //This currently does nothing as nothing listens to it
                    BrainControl.Get().eventManager.e_levelFail.Invoke(BrainControl.Get().runManager.CurrentRun.ActiveLevel);
                }
            }
            //There was no valid path
            //Just continue
            else
            {
                
            }
        });

        // StartCoroutine(Generate());
    }

    bool GridIsValid()
    {
        return !(letterBlocks == null || letterBlocks[0, 0] == null || letterBlocks[letterBlocks.GetLength(0) - 1, letterBlocks.GetLength(1) - 1] == null);
    }

    public bool ValidateChallenges()
    {
        //Pre gate challenges?
        //No - these should happen when the grid is finished
        if(m_activeRequirements.HasFlag(LevelRequirements.AllBlocksFilled))
        {
            if (!GridValidations.AllBlocksFilled(this))
            {
                Debug.LogWarning($"Challenge parameters not met - all blocks filled");
                return false;
            }
        }
        
        if(m_activeRequirements.HasFlag(LevelRequirements.AllUniqueCharacters))
        {
            if (!GridValidations.AllUniqueCharacters(this))
            {
                Debug.LogWarning($"Challenge parameter - all unique letters - is set but has no associated validator");
                return false;
            }
        }
        
        if(m_activeRequirements.HasFlag(LevelRequirements.UseAllTiles))
        {
            Image grub = null;
            BrainControl.Get().uiManager.LevelRequirementGrubs.TryGetValue(LevelRequirements.UseAllTiles, out grub);
            
            if (!GridValidations.UsedAllTiles(this))
            {
                Debug.LogWarning($"Challenge parameters not met - use all tiles");

                if (grub != null)
                {
                    grub.color = Color.red;
                }
                
                return false;
            }
            else
            {
                if (grub != null)
                {
                    grub.color = Color.green;
                }
            }
        }
        
        if(m_activeRequirements.HasFlag(LevelRequirements.DontStutter))
        {
            if (!GridValidations.NoWordsRepeatCharacter(this))
            {
                Debug.LogWarning($"Challenge parameters not met - dont stutter");
                return false;
            }
        }
        
        Debug.LogFormat("All challenges validated");
        return true;
    }

    [Button]
    public bool CheckPath()
    {
        navSurface.BuildNavMesh();

        //Check if grid is built
        //Do this by comparing the actual size of the grid with the request dims
        if (!GridIsValid())
        {
            return false;
        }

        if (!NavMesh.SamplePosition(startPosition, out NavMeshHit hitA, 1, NavMesh.AllAreas))
        {
            return false;
        }

        if (!NavMesh.SamplePosition(targetPosition, out NavMeshHit hitB, 1, NavMesh.AllAreas))
        {
            return false;
        }


        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(hitA.position, hitB.position, NavMesh.AllAreas, path);
        
        
        if (path.status == NavMeshPathStatus.PathComplete)
        {
            Debug.Log("A path was completed");
            return true;
        }

        else
        {
            Debug.Log("There is no available path");
            return false;
        }
    }

    public void HighlightBlock(LetterBlock b)
    {
        if (b == highlightedBlock)
        {
            return;
        }

        //If there is already a highlighted block
        //Unhighlight it
        if (highlightedBlock != null)
        {
            //Change the material to it's highlighted state
            //We should cache this inside the block
            highlightedBlock.MeshRenderer.material.SetInt("_isHighlighted", 0);
            
            //Tween it down
            //This looks unmanaged - we should stop any tweens in progress
            LeanTween.moveLocalY(highlightedBlock.MeshRenderer.gameObject, 0, 0.15f).setEase(LeanTweenType.easeOutExpo);
        }

      
        highlightedBlock = b;
        b.MeshRenderer.gameObject.GetComponent<MeshRenderer>().material.SetInt("_isHighlighted", 1);
        LeanTween.moveLocalY(highlightedBlock.MeshRenderer.gameObject, selectPop, 0.15f).setEase(LeanTweenType.easeOutElastic);
    }

    public void SetLockAll(LockState state)
    {
        foreach (LetterBlock block in letterBlocks)
        {
            block.SetLockState(state);
        }
    }

    public void Unhilight()
    {

        if (highlightedBlock == null) return;

        else
        {
            highlightedBlock.MeshRenderer.gameObject.GetComponent<MeshRenderer>().material.SetInt("_isHighlighted", 0);
            LeanTween.moveLocalY(highlightedBlock.MeshRenderer.gameObject, 0, 0.15f).setEase(LeanTweenType.easeOutExpo);
            highlightedBlock = null;
        }


    }

    public void SelectBlock(LetterBlock a)
    {
        selectedBlock = a;
    }

    public IEnumerator Generate(GridData gridData)
    {
        letterBlocks = new LetterBlock[gridData.gridSeed.GetLength(0),gridData.gridSeed.GetLength(1)];
        
        //OK this is clunky - we are tracking it on the session manager but we need to fetch it here
        //We should do it in one of the places
        //The level tracker could probably do this validation
        m_activeRequirements = BrainControl.Get().runManager.CurrentRun.ActiveLevel.Data.LevelRequirements;
        
        if (transform.Find("gridHolder")) DestroyImmediate(transform.Find("gridHolder").gameObject);
        GameObject gridHolder = new GameObject("gridHolder");
        gridHolder.transform.SetParent(transform);
        gridHolder.name = "gridHolder";


        for (int x = 0; x < letterBlocks.GetLength(0); x++)
        {
            for (int y = 0; y < letterBlocks.GetLength(1); y++)
            {
                LetterBlock newBlock = Instantiate(blockTemplate, new Vector3(x, 0, y), Quaternion.identity);

                newBlock.gridRef = new Vector2Int(x, y);
                newBlock.name = newBlock.gridRef.ToString();

                newBlock.BuildFromGridSeed(gridData.gridSeed[x,y]);

                newBlock.transform.SetParent(gridHolder.transform);
                letterBlocks[x, y] = newBlock;
            }
        }

        //Set targets for camera
        CinemachineTargetGroup.Target targetA;
        targetA.target = letterBlocks[0, 0].transform;
        targetA.radius = 0;
        targetA.weight = 1;

        CinemachineTargetGroup.Target targetB;
        targetB.target = letterBlocks[letterBlocks.GetLength(0) - 1, letterBlocks.GetLength(1) - 1].transform;
        targetB.radius = 0;
        targetB.weight = 1;

        targetGroup.m_Targets = new CinemachineTargetGroup.Target[] { targetA, targetB };

        yield return new WaitForEndOfFrame();
    }

    public List<LetterBlock> CheckAdjacency(LetterBlock a)
    {

        List<LetterBlock> adjacencies = new List<LetterBlock>();

        if (GridTools.IsInBounds(new Vector2Int(a.gridRef.x + 1, a.gridRef.y)) && letterBlocks[a.gridRef.x + 1, a.gridRef.y].fillState == FillState.filled) adjacencies.Add(letterBlocks[a.gridRef.x + 1, a.gridRef.y]);
        if (GridTools.IsInBounds(new Vector2Int(a.gridRef.x, a.gridRef.y - 1)) && letterBlocks[a.gridRef.x, a.gridRef.y - 1].fillState == FillState.filled) adjacencies.Add(letterBlocks[a.gridRef.x, a.gridRef.y - 1]);
        if (GridTools.IsInBounds(new Vector2Int(a.gridRef.x - 1, a.gridRef.y)) && letterBlocks[a.gridRef.x - 1, a.gridRef.y].fillState == FillState.filled) adjacencies.Add(letterBlocks[a.gridRef.x - 1, a.gridRef.y]);
        if (GridTools.IsInBounds(new Vector2Int(a.gridRef.x, a.gridRef.y + 1)) && letterBlocks[a.gridRef.x, a.gridRef.y + 1].fillState == FillState.filled) adjacencies.Add(letterBlocks[a.gridRef.x, a.gridRef.y + 1]);
        return adjacencies;
    }


}
