using System;
using System.Linq;
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

    public Letter letter;



    public Vector2Int gridRef;

    LTDescr pop;

    public float buildPopStrength;
    public float emptyPopStrength;

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

        letter = new Letter(char.MinValue, 0);

        gameObject.layer = LayerMask.NameToLayer("Default");

        letterBox.text = "";
        scoreBox.text = "";
    }

    public void BuildFromLetter(char _letter)
    {
        Debug.Log("Assigning letter " + _letter + " to the block");

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
        letter = new Letter(GameObject.FindGameObjectWithTag("Brain").GetComponent<Brain>().scoreManager.scoreSet.scoreset.Keys.FirstOrDefault(x => x.character == _letter));

        gameObject.layer = LayerMask.NameToLayer("Navigable");

        //if (Application.isPlaying) Brain.ins.eventManager.e_navUpdate.Invoke();

        letterBox.text = letter.character.ToString();
        scoreBox.text = letter.score.ToString();


    }
    public void BuildFromDistribution(float minDist, float maxDist)
    {

        if (Application.isPlaying) LeanTween.scale(gameObject, Vector3.one * buildPopStrength, Random.Range(0.45f, 0.85f)).setEase(LeanTweenType.punch).setOnComplete(() => transform.localScale = Vector3.one);

        fillState = FillState.filled;

        meshObject.GetComponent<MeshRenderer>().material = filledMaterial;

        //Create a new copy of the letter entry found using the random letter
        letter = GameObject.FindGameObjectWithTag("Brain").GetComponent<Brain>().scoreManager.scoreSet.LetterFromDistribution(minDist, maxDist);

        gameObject.layer = LayerMask.NameToLayer("Navigable");

        // Brain.ins.eventManager.e_navUpdate.Invoke();

        letterBox.text = letter.character.ToString();
        scoreBox.text = letter.score.ToString();


    }

    public void BuildWeighted()
    {
        if (Application.isPlaying) LeanTween.scale(gameObject, Vector3.one * buildPopStrength, Random.Range(0.45f, 0.85f)).setEase(LeanTweenType.punch).setOnComplete(() => transform.localScale = Vector3.one);

        fillState = FillState.filled;

        meshObject.GetComponent<MeshRenderer>().material = filledMaterial;

        //Create a new copy of the letter entry found using the random letter
        letter = GameObject.FindGameObjectWithTag("Brain").GetComponent<Brain>().scoreManager.scoreSet.WeightedLetter();

        gameObject.layer = LayerMask.NameToLayer("Navigable");

        // Brain.ins.eventManager.e_navUpdate.Invoke();

        letterBox.text = letter.character.ToString();
        scoreBox.text = letter.score.ToString();
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
