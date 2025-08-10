using System;
using System.Collections.Generic;
using System.Linq;
using CodingJar;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public enum FillState { empty, filled }
public enum LockState { unlocked, locked }

[Serializable]
public class LetterBlock : MonoBehaviour
{
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

    public Letter baseLetter
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
        Debug.Log("Emptying the letter block");

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

        baseLetter = new Letter(char.MinValue, 0);

        gameObject.layer = LayerMask.NameToLayer("Default");

        letterBox.text = "";
        scoreBox.text = "";
    }
    
     public void BuildFromGridSeed(GridSeed seed)
    {
        MeshRenderer.material = new Material(filledMaterial);
        
        //Debug.Log("Assigning character " + token + " to the block");
        SetLockState(LockState.unlocked);
        
        if ((seed.Flags & GridElementFlags.Start) != 0)
        {
           //Start
           Debug.Log($"Interpreted the block into a start block", gameObject);
           SetAsStart();
        }
        else if ((seed.Flags & GridElementFlags.End) != 0)
        {
            //End
            Debug.Log($"Interpreted the block into a end block", gameObject);
            SetAsTarget();
        }

        if ((seed.Flags & GridElementFlags.Empty) != 0)
        {
            //None
            Debug.Log($"Interpreted the block into a blank, unlocked block.", gameObject);
            Empty();
        }
        
        if ((seed.Flags & GridElementFlags.Blocked) != 0)
        {
            //Blocked
            Debug.Log($"Interpreted the block into a blank, locked block.", gameObject);
            // Empty();
            SetLockState(LockState.locked);
            //return;
        }
        
        if ((seed.Flags & GridElementFlags.Empty) != 0)
        {
            return;
        }
        
        var targetChar = seed.Content[0];
        
        //Create a new copy of the letter entry found using the passed letters
        //Why does this fail?
        var match = BrainControl.Get().runManager.CurrentRun.RunSettings.ActiveScoringRubrik.distribution.Keys.FirstOrDefault(x => char.ToLower(x.character) == char.ToLower(targetChar)); 
        
        if(match != null)
        {
            Debug.LogFormat($"Found a match for character {targetChar}");
            baseLetter = new Letter(match);
        }
        else
        {
            Debug.LogFormat($"{targetChar} does not exist in the active scoreset");
            return;
        }
      
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

        fillState = FillState.filled;
        
        letterBox.text = baseLetter.character.ToString();
        scoreBox.text = baseLetter.score.ToString();
    }
    
     public void BuildTokenised(string token)
    {
        Debug.Log("Assigning character " + token + " to the block");
        
        //So can we do like - tokenisation?
        if (token.Contains('-'))
        {
            Debug.Log($"Tokenised the block into a blank, unlocked block.", gameObject);
            Empty();
            SetLockState(LockState.unlocked);
            return;
        }
        
        else if (token.Contains('#'))
        {
            Debug.Log($"Tokenised the block into a blank, locked block.", gameObject);
            Empty();
            SetLockState(LockState.locked);
            return;
        }
        
        //Create a new copy of the letter entry found using the passed letters
        //Why does this fail?
        var match = BrainControl.Get().runManager.CurrentRun.RunSettings.ActiveScoringRubrik.distribution.Keys.FirstOrDefault(x => char.ToLower(x.character) == char.ToLower(token[0])); 
        
        if(match != null)
        {
            Debug.LogFormat($"Found a match for character {token}");
            baseLetter = new Letter(match);
        }
        else
        {
            Debug.LogFormat($"{token} does not exist in the active scoreset");
            return;
        }
      
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

        fillState = FillState.filled;
        
        MeshRenderer.material = new Material(filledMaterial);
        
        letterBox.text = baseLetter.character.ToString();
        scoreBox.text = baseLetter.score.ToString();
        
        if (token.Contains("1"))
        {
            Debug.Log($"Tokenised the block into a start block", gameObject);
            SetAsStart();
        }
        
        else if (token.Contains("2"))
        {
            Debug.Log($"Tokenised the block into a end block", gameObject);
            SetAsTarget();
        }
        
        // BrainControl.Get().eventManager.e_navUpdate.Invoke();
    }
    public void BuildFromDistribution(float minDist, float maxDist)
    {
        if (Application.isPlaying) LeanTween.scale(gameObject, Vector3.one * buildPopStrength, Random.Range(0.45f, 0.85f)).setEase(LeanTweenType.punch).setOnComplete(() => transform.localScale = Vector3.one);

        fillState = FillState.filled;

        MeshRenderer.material = new Material(filledMaterial);

        //Create a new copy of the letter entry found using the random letter
        baseLetter = BrainControl.Get().runManager.CurrentRun.RunSettings.ActiveScoringRubrik.LetterFromDistribution(minDist, maxDist);

        gameObject.layer = LayerMask.NameToLayer("Navigable");

        // Brain.ins.eventManager.e_navUpdate.Invoke();

        letterBox.text = baseLetter.character.ToString();
        scoreBox.text = baseLetter.score.ToString();
    }

    public void BuildWeighted()
    {
        if (Application.isPlaying) LeanTween.scale(gameObject, Vector3.one * buildPopStrength, Random.Range(0.45f, 0.85f)).setEase(LeanTweenType.punch).setOnComplete(() => transform.localScale = Vector3.one);

        fillState = FillState.filled;

        MeshRenderer.material = new Material(filledMaterial);

        //Create a new copy of the letter entry found using the random letter
        baseLetter = BrainControl.Get().runManager.CurrentRun.RunSettings.ActiveScoringRubrik.WeightedLetter();

        gameObject.layer = LayerMask.NameToLayer("Navigable");

        // Brain.ins.eventManager.e_navUpdate.Invoke();

        letterBox.text = baseLetter.character.ToString();
        scoreBox.text = baseLetter.score.ToString();
    }

    public void SetAsStart()
    {
        MeshRenderer.material.SetInt("_isStart", 1);
        //Lock this to make it unselectable
        //SetLockState(LockState.locked);
        gameObject.layer = LayerMask.NameToLayer("Navigable");
        BrainControl.Get().grid.startPosition = transform.position;
    }

    public void SetAsTarget()
    {
        MeshRenderer.material.SetInt("_isTarget", 1);
        //Lock this to make it unselectable
        //SetLockState(LockState.locked);
        gameObject.layer = LayerMask.NameToLayer("Navigable");
        BrainControl.Get().grid.targetPosition = transform.position;
    }
}
