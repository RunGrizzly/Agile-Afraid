using System;
using System.Collections.Generic;
using System.Linq;
using CodingJar;
using TMPro;
using UnityEngine;

using Random = UnityEngine.Random;


public enum FillState { empty, filled }
public enum LockState { unlocked, locked }

[Serializable]
public class LetterBlock : MonoBehaviour
{
    public GameObject meshObject;
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
    public void SetLockState(LockState state)
    {
        lockState = state;
        meshObject.GetComponent<MeshRenderer>().material.SetInt("_isLocked", state == LockState.locked ? 1 : 0);
    }

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
                pop.setOnComplete(() => pop = LeanTween.scale(meshObject, Vector3.one * emptyPopStrength, Random.Range(0.45f, 0.85f)).setEase(LeanTweenType.punch).setOnComplete(() => transform.localScale = Vector3.one));
            }
            else pop = LeanTween.scale(meshObject, Vector3.one * emptyPopStrength, Random.Range(0.45f, 0.85f)).setEase(LeanTweenType.punch).setOnComplete(() => transform.localScale = Vector3.one);
        }

        fillState = FillState.empty;

        meshObject.GetComponent<MeshRenderer>().material = emptyMaterial;

        baseLetter = new Letter(char.MinValue, 0);

        gameObject.layer = LayerMask.NameToLayer("Default");

        letterBox.text = "";
        scoreBox.text = "";
    }

    public void BuildFromLetter(char _letter)
    {
        //Debug.Log("Assigning letter " + _letter + " to the block");

        if (Application.isPlaying)
        {
            //Force pop to end
            if (pop != null)
            {
                pop.setTime(0.9f);
                pop.setOnComplete(() => pop = LeanTween.scale(meshObject, Vector3.one * buildPopStrength, Random.Range(0.45f, 0.85f)).setEase(LeanTweenType.punch).setOnComplete(() => transform.localScale = Vector3.one));
            }
            else pop = LeanTween.scale(meshObject, Vector3.one * buildPopStrength, Random.Range(0.45f, 0.85f)).setEase(LeanTweenType.punch).setOnComplete(() => transform.localScale = Vector3.one);
        }

        fillState = FillState.filled;

        meshObject.GetComponent<MeshRenderer>().material = filledMaterial;

        //Create a new copy of the letter entry found using the passed letters
        baseLetter = new Letter(GameObject.FindGameObjectWithTag("Brain").GetComponent<Brain>().scoreManager.scoreSet.scoreset.Keys.FirstOrDefault(x => x.character == _letter));

        gameObject.layer = LayerMask.NameToLayer("Navigable");

        //if (Application.isPlaying) Brain.ins.eventManager.e_navUpdate.Invoke();

        letterBox.text = baseLetter.character.ToString();
        scoreBox.text = baseLetter.score.ToString();
    }
    public void BuildFromDistribution(float minDist, float maxDist)
    {

        if (Application.isPlaying) LeanTween.scale(gameObject, Vector3.one * buildPopStrength, Random.Range(0.45f, 0.85f)).setEase(LeanTweenType.punch).setOnComplete(() => transform.localScale = Vector3.one);

        fillState = FillState.filled;

        meshObject.GetComponent<MeshRenderer>().material = filledMaterial;

        //Create a new copy of the letter entry found using the random letter
        baseLetter = GameObject.FindGameObjectWithTag("Brain").GetComponent<Brain>().scoreManager.scoreSet.LetterFromDistribution(minDist, maxDist);

        gameObject.layer = LayerMask.NameToLayer("Navigable");

        // Brain.ins.eventManager.e_navUpdate.Invoke();

        letterBox.text = baseLetter.character.ToString();
        scoreBox.text = baseLetter.score.ToString();
    }

    public void BuildWeighted()
    {
        if (Application.isPlaying) LeanTween.scale(gameObject, Vector3.one * buildPopStrength, Random.Range(0.45f, 0.85f)).setEase(LeanTweenType.punch).setOnComplete(() => transform.localScale = Vector3.one);

        fillState = FillState.filled;

        meshObject.GetComponent<MeshRenderer>().material = filledMaterial;

        //Create a new copy of the letter entry found using the random letter
        baseLetter = GameObject.FindGameObjectWithTag("Brain").GetComponent<Brain>().scoreManager.scoreSet.WeightedLetter();

        gameObject.layer = LayerMask.NameToLayer("Navigable");

        // Brain.ins.eventManager.e_navUpdate.Invoke();

        letterBox.text = baseLetter.character.ToString();
        scoreBox.text = baseLetter.score.ToString();
    }

    public void SetAsStart()
    {
        meshObject.GetComponent<MeshRenderer>().material.SetInt("_isStart", 1);
    }

    public void SetAsTarget()
    {
        meshObject.GetComponent<MeshRenderer>().material.SetInt("_isTarget", 1);
    }
}
