using System.Collections;
using UnityEngine;
using Cinemachine;
using Sirenix.OdinInspector;
using Unity.AI.Navigation;
using UnityEngine.AI;
using UnityEngine.UI;

public class GridGenerator : MonoBehaviour
{
    //We use the start and end position to calculate a path - maybe this could be better?
    [ReadOnly]
    public Vector3 StartPosition = Vector3.zero;
    [ReadOnly]
    public Vector3 TargetPosition = new Vector3(100, 100, 100);
    
    public LetterBlock[,] letterBlocks = new LetterBlock[,] { };

    [SerializeField]
    private LetterBlock m_blockTemplate;
    
    [SerializeField]
    private CinemachineTargetGroup m_targetGroup;

    [SerializeField]
    private NavMeshSurface m_navSurface;
  
    [SerializeField]
    private MeshRenderer m_deadzoneRenderer = null;
    
    private LevelRequirements m_activeRequirements;
    
    private void OnEnable()
    {
        BrainControl.Get().eventManager.e_levelLoaded.AddListener(SetDeadzoneColors);
        // BrainControl.Get().eventManager.e_validateSuccess.AddListener(CheckForCompletion);
    }

    private void OnDisable()
    {
        BrainControl.Get().eventManager.e_levelLoaded.RemoveListener(SetDeadzoneColors);
        // BrainControl.Get().eventManager.e_validateSuccess.RemoveListener(CheckForCompletion);
    }

    private void SetDeadzoneColors(Level level)
    {
        m_deadzoneRenderer.material.SetColor("_ColorA", level.Data.DeadZoneColorA);
        m_deadzoneRenderer.material.SetColor("_ColorB", level.Data.DeadZoneColorB);
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
                    BrainControl.Get().uiManager.PrintMessage("Challenge parameters not met");
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

    public void CheckForCompletion(BlockInput blockInput)
    {
        //We check if there is a route from the start to end filled with validated blocks
        if (CheckPath(StartPosition, TargetPosition, 1 << NavMesh.GetAreaFromName("Validated")))
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
                Debug.LogWarningFormat($"The path was completed but the challenges failed.");
            }
        }
        else
        {
         Debug.LogWarningFormat($"There is no valid path to check completion against.");
        }
    }

    [Button]
    public bool CheckPath(Vector3 positionA, Vector3 positionB, int areaMask)
    {
        //Problem exists here where it doesn't take into account non-validated blocks
        m_navSurface.BuildNavMesh();

        //Check if grid is built
        //Do this by comparing the actual size of the grid with the request dims
        if (!GridIsValid())
        {
            return false;
        }

        
        if (!NavMesh.SamplePosition(positionA, out NavMeshHit hitA, 1, areaMask))
        {
            return false;
        }

        if (!NavMesh.SamplePosition(positionB, out NavMeshHit hitB, 1, areaMask))
        {
            return false;
        }


        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(hitA.position, hitB.position, areaMask, path);
        
        
        if (path.status == NavMeshPathStatus.PathComplete)
        {
            //Debug.Log("A path was completed");
            return true;
        }

        else
        {
            //Debug.Log("There is no available path");
            return false;
        }
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
                LetterBlock newBlock = Instantiate(m_blockTemplate, new Vector3(x, 0, y), Quaternion.identity);

                newBlock.gridRef = new Vector2Int(x, y);
                newBlock.name = newBlock.gridRef.ToString();
                
                Debug.LogFormat(newBlock.transform.gameObject,$"Attempting build a seeded block at {x},{y}");
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

        m_targetGroup.m_Targets = new CinemachineTargetGroup.Target[] { targetA, targetB };

        yield return new WaitForEndOfFrame();
    }
}
