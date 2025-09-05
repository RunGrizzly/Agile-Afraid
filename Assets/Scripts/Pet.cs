using UnityEngine;

[CreateAssetMenu(fileName = "New Pet", menuName = "Data/Pets/New Pet", order = 1)]
public class Pet: ScriptableObject
{
    //Character
    [SerializeField]
    private string m_id = "NewPet";
    
    //Passive Ability (Run Modification) (Ws score more)
    public CharacterPassive Passive;
    
    //Buttons change depending on driver
    //Ability A - 10 mana
    //Ability B - 50 mana

    //How does this slot in?
    //Run >> Driver
    //How do you pick? Some menu? StartRun (driver, options)
}