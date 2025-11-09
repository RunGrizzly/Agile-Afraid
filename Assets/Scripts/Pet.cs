using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//How does this slot in?
//Run >> Driver
//How do you pick? Some menu? StartRun (driver, options)
[CreateAssetMenu(fileName = "New Pet", menuName = "Data/Pets/New Pet", order = 1)]
public class Pet: ScriptableObject
{
    //Character
    [SerializeField]
    private Sprite m_portrait;
    
    [SerializeField]
    private string m_id = "NewPet";
    
    //Passive Ability (Run Modification) (Ws score more)
    public CharacterPassive Passive;

    //Buttons change depending on driver
    //Ability A - 10 mana
    //Ability B - 50 mana
    public Ability BasicAbility = null;
    public Ability UltimateAbility = null;

    public void Activate()
    {
        BasicAbility.Activate();
        UltimateAbility.Activate();
    }
}

public class UIPanel : MonoBehaviour
{
    protected TextMeshProUGUI m_header;
    protected CanvasGroup m_canvasGroupMaster;
    protected List<UIPanel> m_subPanels = new();
}

public class RunInfoPanel : UIPanel
{
    private Image m_petImage;
    
}