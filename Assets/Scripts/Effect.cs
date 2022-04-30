using System;
using UnityEngine;
using Random = UnityEngine.Random;


//Defines a block effect that can be activated 

[Serializable]
public class Effect
{


}

public interface ILetterEffect
{
    public Letter ApplyEffect(Letter _l);

}

[Serializable]
public class MultiplyLetterScore : Effect, ILetterEffect
{
    public int multiplier;

    public MultiplyLetterScore(int _multiplier)
    {
        multiplier = _multiplier;
    }

    public virtual Letter ApplyEffect(Letter _l)
    {
        Letter newLetter = _l;
        newLetter.score *= multiplier;
        return newLetter;
    }
}


[Serializable]
public class DivideLetterScore : Effect, ILetterEffect
{
    public int divisor;

    public DivideLetterScore(int _divisor)
    {
        divisor = _divisor;
    }

    public virtual Letter ApplyEffect(Letter _l)
    {
        Letter newLetter = _l;
        newLetter.score /= divisor;
        return newLetter;
    }
}


[Serializable]
public class RandomLetterScore : Effect, ILetterEffect
{
    public float lowerBound;
    public float upperBound;

    public RandomLetterScore(int _lower, int _upper)
    {
        lowerBound = _lower;
        upperBound = _upper;
    }

    public virtual Letter ApplyEffect(Letter _l)
    {
        Letter newLetter = _l;
        newLetter.score = (int)MathF.Round(Random.Range(lowerBound, upperBound));
        return newLetter;
    }
}
