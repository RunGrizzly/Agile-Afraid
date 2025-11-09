using UnityEngine;

public class ButtonControls : MonoBehaviour
{
    public void GetVowel()
    {
        BrainControl.Get().runManager.CurrentRun.getVowelListener.Invoke(true,true);
        
        //BrainControl.Get().eventManager.e_getVowelRequest.Invoke(false,true);
    }
    public void GetConsonant()
    {
        BrainControl.Get().runManager.CurrentRun.getConsonantListener.Invoke(true,true);
        
        //BrainControl.Get().eventManager.e_getConsonantRequest.Invoke(false,true);
    }
    public void NewRack()
    {
        BrainControl.Get().runManager.CurrentRun.newRackListener.Invoke(null,BrainControl.Get().runManager.CurrentRun.activeBossDungeon.RackSize, true,true);
            
        //BrainControl.Get().eventManager.e_newRackRequest.Invoke(null,BrainControl.Get().runManager.CurrentRun.ActiveLevel.Data.rackSize, false,true);
    }
    public void FillRack()
    {
        BrainControl.Get().runManager.CurrentRun.fillRackListener.Invoke(BrainControl.Get().runManager.CurrentRun.activeBossDungeon.RackSize, true, true);
        
        //BrainControl.Get().eventManager.e_fillRackRequest.Invoke(BrainControl.Get().runManager.CurrentRun.ActiveLevel.Data.rackSize, false, true);
    }
    
    public void Pause()
    {
        BrainControl.Get().runManager.CurrentRun.SetPaused(!BrainControl.Get().runManager.CurrentRun.IsPaused);
    }

    public void QuitRun()
    {
        BrainControl.Get().runManager.ResolveRun(Resolution.Aborted); 
        BrainControl.Get().eventManager.e_quitToMenu.Invoke();
    }

    public void EmptyRack()
    {
        BrainControl.Get().runManager.CurrentRun.emptyRackListener.Invoke(true,true);
    }

    public void PetAbilityA()
    {
        BrainControl.Get().runManager.CurrentRun.ActivePet.BasicAbility.OnExecute();
    }

    public void PetAbilityB()
    {
        BrainControl.Get().runManager.CurrentRun.ActivePet.UltimateAbility.OnExecute();
    }
}
