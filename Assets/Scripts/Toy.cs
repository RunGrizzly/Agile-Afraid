using UnityEngine;

//Ball passives can be picked up and accumulated
//Are the available passives specific to the pet?
public abstract class Toy: ScriptableObject
{
    //On hover
    //Show info about the toy
    //A toy that goes into the run modification slot
    public ToyPassive Passive; //EX Words with a W score x2
    public abstract void OnPickUp();
    public abstract void OnDiscard();
}