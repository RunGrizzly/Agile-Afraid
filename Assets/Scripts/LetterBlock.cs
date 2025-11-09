using System;
using System.Collections.Generic;
using System.Linq;
using CodingJar;
using Sirenix.OdinInspector;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


public enum FillState { empty, filled }
public enum LockState { unlocked, locked }

[Serializable]
public class LetterBlock : MonoBehaviour, ILetterDestination //Selectable?
{
    public NavMeshModifier NavMeshModifier = null;
    
    
    [SerializeField]
    public MeshRenderer MeshRenderer = null;
    
    //public GameObject m_meshRenderer.gameObject;
    public FillState fillState;

    public LockState lockState;

    public Material filledMaterial;
    public Material emptyMaterial;
    public TextMeshProUGUI letterBox;
    public TextMeshProUGUI scoreBox;

    private Letter _baseLetter;

    //The block itself can hold special properties
    [SerializeField]
    private ScriptableObject pickupExample = null;
    
    
    //So how to intercept scores and apply bonuses etc
    
    //OnBlockValidated
    //OnBlockFilled
    
    //OnTileReceived
    //OnTileDiscarded
    
    //OnLetterScored
    
    //SO:RunModifierTrigger
    
    //Not only blocks right?
    
    
    
    public Letter BaseLetter
    {
        set
        {
            _baseLetter = value;
            letter = ModifyLetter();
        }
        get
        {
            return _baseLetter;
        }
    }

    [Readonly]
    public Letter letter;



    public List<ILetterEffect> letterEffects = new List<ILetterEffect>();


    public Vector2Int gridRef;
    LTDescr pop;
    public float buildPopStrength;
    public float emptyPopStrength;


    public Letter ModifyLetter()
    {
        Letter m = _baseLetter;

        foreach (ILetterEffect e in letterEffects)
        {
            m = e.ApplyEffect(m);
        }

        return m;
    }
    
    [Button]
    public void SetLockState(LockState state)
    {
        lockState = state;
        MeshRenderer.material.SetInt("_isLocked", state == LockState.locked ? 1 : 0);
    }

    [Button]
    public void Empty()
    {   
        Debug.LogFormat(gameObject,$"Emptying the letter block");

        //This clashes with fill pops that are called too early
        if (Application.isPlaying)
        {
            //Force pop to end
            if (pop != null)
            {
                pop.setTime(0.9f);
                pop.setOnComplete(() => pop = LeanTween.scale(MeshRenderer.gameObject, Vector3.one * emptyPopStrength, Random.Range(0.45f, 0.85f)).setEase(LeanTweenType.punch).setOnComplete(() => transform.localScale = Vector3.one));
            }
            else pop = LeanTween.scale(MeshRenderer.gameObject, Vector3.one * emptyPopStrength, Random.Range(0.45f, 0.85f)).setEase(LeanTweenType.punch).setOnComplete(() => transform.localScale = Vector3.one);
        }

        fillState = FillState.empty;

        MeshRenderer.material = new Material(emptyMaterial);

        BaseLetter = new Letter(char.MinValue, 0);

        NavMeshModifier.area = NavMesh.GetAreaFromName("Not Set");
        //gameObject.layer = LayerMask.NameToLayer("Default");

        letterBox.text = "";
        scoreBox.text = "";
    }

    //Letter block custom validity check
    //A letter block is valid for sending if it create a route to the start tile
    public bool IsValid()
    {
        BlockInput activeInput = null;
        
        //First check - if we are locked or filled - not valid
        if (lockState != LockState.unlocked || fillState != FillState.empty)
        {
            return false;
        }

        //Assume this block is an input block
        NavMeshModifier.area = NavMesh.GetAreaFromName("Input");

        //We can use the grid, to check if there is a navigable path the start position and the new block
        //We want to include both validated, and non validated inputs

        int checkMask = (1 << NavMesh.GetAreaFromName("Input")) | (1 << NavMesh.GetAreaFromName("Validated"));

        if (!BrainControl.Get().Grid.CheckPath(BrainControl.Get().Grid.StartPosition, transform.position, checkMask))
        {
            Debug.LogFormat($"The destination block does not form a valid path with the origin");
            return false;
        }
        
        //If there is no input yet - just fill the block
        //This shouldn't be the purview of the block
        if (BrainControl.Get().runManager.CurrentRun.ActiveLevel.InputInProgress() == null)
        {
            //So it always returns true if there is no prior input
            Debug.LogFormat($"Block is possible (there are no prior inputs)");
            return true;   
        }
        else
        {
            //There are already inputs
            Debug.LogFormat($"There is an input in progress");
            
            //We get the latest input and highlight the possible lines from it
            activeInput = BrainControl.Get().runManager.CurrentRun.ActiveLevel.InputInProgress();
            //We reveal the latest input blocks
            //RevealInputBlocks(latestInput);
            
            
            //We get the possible lines that are already in play
            var possibleLines = activeInput.PossibleLines;
            // RevealPossibleLines(possibleLines);

            //If this block is contained in those possible lines
            if (possibleLines.SelectMany(x => x.blocks).ToList().Contains(this))
            {
                //We are a valid block
                Debug.LogFormat($"Block is possible (the block is withing possible input lines)");
                return true;
            }
            else
            {
                //We are not within those possible lines
                //We are not valid
                Debug.LogFormat($"Tried to enter a block that is not part of the current input.");

                //Reset navmesh
                NavMeshModifier.area = NavMesh.GetAreaFromName("Not Set");
                return false;
            }
        }
    }

    //Actually set the letter
    public void SetLetter(Letter letter)
    {
        BaseLetter = letter;
        letterBox.text = BaseLetter.character.ToString();
        scoreBox.text = BaseLetter.score.ToString(); 
        
        fillState = FillState.filled;
        
        //We have assigned a letter so we can set the material to filled
        MeshRenderer.material = new Material(filledMaterial);
    }
    
    //A letter has been sent via an input
    public void SendLetter(Letter letter)
    {
        SetLetter(letter);
        
        //Can we consolidate?
        //We already started the input
        if (BrainControl.Get().runManager.CurrentRun.ActiveLevel.InputInProgress() != null)
        {
            BrainControl.Get().eventManager.e_updateInput.Invoke(this);
        }
        //This is a new input
        else
        {
            BrainControl.Get().eventManager.e_beginInput.Invoke(this);
        }
    }
    
     public void BuildFromGridSeed(GridSeed seed)
    {
        //So sometimes the seed content can be null
        if (seed.Content.Length > 0)
        {
            var targetChar = seed.Content[0];

            //Create a new copy of the letter entry found using the passed letters
            //Why does this fail?
            var match = BrainControl.Get().runManager.CurrentRun.activeBossDungeon.ScoringRubrik.distribution.Keys.FirstOrDefault(x => char.ToLower(x.character) == char.ToLower(targetChar));

            if (match != null)
            {
                // /Debug.LogFormat($"Found a match for character {targetChar}");
                SetLetter(match);
                NavMeshModifier.area = NavMesh.GetAreaFromName("Validated");
            }
            else
            {
                //Debug.LogFormat($"{targetChar} does not exist in the active scoreset");
                return;
            }
        }

        //Debug.Log("Assigning character " + token + " to the block");
        SetLockState(LockState.unlocked);
        
        if ((seed.Flags & GridElementFlags.Start) != 0)
        {
           //Start
           //Debug.Log($"Interpreted the block into a start block", gameObject);
           SetAsStart();
        }
        else if ((seed.Flags & GridElementFlags.End) != 0)
        {
            //End
            //Debug.Log($"Interpreted the block into a end block", gameObject);
            SetAsTarget();
        }
    
        if ((seed.Flags & GridElementFlags.Empty) != 0)
        {
            //None
            //Debug.Log($"Interpreted the block into a blank, unlocked block.", gameObject);
            Empty();
        }
        
        if ((seed.Flags & GridElementFlags.Blocked) != 0)
        {
            //Blocked
            //Debug.Log($"Interpreted the block into a blank, locked block.", gameObject);
            // Empty();
            SetLockState(LockState.locked);
            //return;
        }
        
        if ((seed.Flags & GridElementFlags.Empty) != 0)
        {
            return;
        }
        
        // fillState = FillState.filled;
        
        if (Application.isPlaying)
        {
            //Force pop to end
            if (pop != null)
            {
                pop.setTime(0.9f);
                pop.setOnComplete(() => pop = LeanTween.scale(MeshRenderer.gameObject, Vector3.one * buildPopStrength, Random.Range(0.45f, 0.85f)).setEase(LeanTweenType.punch).setOnComplete(() => transform.localScale = Vector3.one));
            }
            else pop = LeanTween.scale(MeshRenderer.gameObject, Vector3.one * buildPopStrength, Random.Range(0.45f, 0.85f)).setEase(LeanTweenType.punch).setOnComplete(() => transform.localScale = Vector3.one);
        }
    }
     
    public void SetAsStart()
    {
        MeshRenderer.material.SetInt("_isStart", 1);
        //Lock this to make it unselectable
        //SetLockState(LockState.locked);
        // gameObject.layer = LayerMask.NameToLayer("Navigable");
        BrainControl.Get().Grid.StartPosition = transform.position;
    }

    public void SetAsTarget()
    {
        MeshRenderer.material.SetInt("_isTarget", 1);
        //Lock this to make it unselectable
        //SetLockState(LockState.locked);
        // gameObject.layer = LayerMask.NameToLayer("Navigable");
        BrainControl.Get().Grid.TargetPosition = transform.position;
    }
}
