using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.AI.Navigation;
using UnityEngine.AI;

public class GridGenerator : MonoBehaviour
{
    [SerializeField] public Vector2Int gridDims;

    public LetterBlock[,] letterBlocks = new LetterBlock[,]{};

    public List<WordRequest> requests = new List<WordRequest>();

    public GameObject blockTemplate;

    public LetterBlock highlightedBlock;
    public LetterBlock selectedBlock;

    public float selectPop;

    public CinemachineTargetGroup targetGroup;

    public NavMeshSurface navSurface;

    public Vector3 startPosition = Vector3.zero, targetPosition = new Vector3(100, 100, 100);

    public GameObject deadZone;

    public NavMeshPath path;

    public GUISkin editorSkin;


    void Start()
    {

        BrainControl.Get().eventManager.e_levelLoaded.AddListener((l) => deadZone.GetComponent<MeshRenderer>().material.SetColor("_MainColor", l.data.deadZoneColor));

        //Level level event responses
        /////////////////////////////
        Brain.ins.eventManager.e_blockSelected.AddListener((b) =>
        {
            if (b.lockState != LockState.locked) SelectBlock(b);
        });

        Brain.ins.eventManager.e_clearBlock.AddListener((c) =>
        {
            c.Empty();
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

        Brain.ins.eventManager.e_endInput.AddListener(() =>
        {

        });


        Brain.ins.eventManager.e_navUpdate.AddListener(() =>
        {
            navSurface.BuildNavMesh();
            //Try to calculate a path
            path = new NavMeshPath();

            //Check if grid is built
            //Do this by comparing the actual size of the grid with the request dims
            if (!GridIsValid()) return;

            bool p = NavMesh.CalculatePath(startPosition, targetPosition, NavMesh.AllAreas, path);

            if (path.status == NavMeshPathStatus.PathComplete)
            {
                Debug.Log("A path was completed");
                BrainControl.Get().eventManager.e_pathComplete.Invoke();
            }

            else Debug.Log("There is no available path");
        });

        // StartCoroutine(Generate());
    }

    bool GridIsValid()
    {
        return !(letterBlocks == null || letterBlocks[0, 0] == null || letterBlocks[letterBlocks.GetLength(0) - 1, letterBlocks.GetLength(1) - 1] == null);
    }

    public void HighlightBlock(LetterBlock b)
    {

        if (b == highlightedBlock) return;

        if (highlightedBlock != null)
        {
            highlightedBlock.meshObject.GetComponent<MeshRenderer>().material.SetInt("_isHighlighted", 0);
            LeanTween.moveLocalY(highlightedBlock.meshObject, 0, 0.15f).setEase(LeanTweenType.easeOutExpo);
        }

        highlightedBlock = b;
        b.meshObject.GetComponent<MeshRenderer>().material.SetInt("_isHighlighted", 1);
        LeanTween.moveLocalY(highlightedBlock.meshObject, selectPop, 0.15f).setEase(LeanTweenType.easeOutExpo);
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
            highlightedBlock.meshObject.GetComponent<MeshRenderer>().material.SetInt("_isHighlighted", 0);
            LeanTween.moveLocalY(highlightedBlock.meshObject, 0, 0.15f).setEase(LeanTweenType.easeOutExpo);
            highlightedBlock = null;
        }


    }

    public void SelectBlock(LetterBlock a)
    {
        selectedBlock = a;
    }

    public IEnumerator Generate(Vector2Int _gridDims)
    {
        letterBlocks = new LetterBlock[_gridDims.x, _gridDims.y];

        if (transform.Find("gridHolder")) DestroyImmediate(transform.Find("gridHolder").gameObject);
        GameObject gridHolder = new GameObject("gridHolder");
        gridHolder.transform.SetParent(transform);
        gridHolder.name = "gridHolder";


        for (int x = 0; x < letterBlocks.GetLength(0); x++)
        {
            for (int y = 0; y < letterBlocks.GetLength(1); y++)
            {
                LetterBlock newBlock = Instantiate(blockTemplate, new Vector3(x, 0, y), Quaternion.identity).GetComponent<LetterBlock>();

                newBlock.gridRef = new Vector2Int(x, y);
                newBlock.name = newBlock.gridRef.ToString();

                newBlock.Empty();

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

        // Brain.ins.eventManager.e_navUpdate.Invoke();

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
