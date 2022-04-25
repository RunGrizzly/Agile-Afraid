using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonControls : MonoBehaviour
{



    public void GetVowel()
    {
        BrainControl.Get().eventManager.e_getVowel.Invoke(true);

    }
    public void GetConsonant()
    {
        BrainControl.Get().eventManager.e_getConsonant.Invoke(true);
    }
    public void NewRack()
    {
        BrainControl.Get().eventManager.e_newRack.Invoke(BrainControl.Get().sessionManager.currentSession.currentLevel.data.rackSize, true);
    }
    public void FillRack()
    {
        BrainControl.Get().eventManager.e_fillRack.Invoke(BrainControl.Get().sessionManager.currentSession.currentLevel.data.rackSize, true);
    }

}
