using UnityEngine.EventSystems;

public class AbilityButton : CustomButton
{
    public Ability TriggeredAbility = null;

    public override void OnPointerClick(PointerEventData eventData)
    {
        TriggeredAbility.OnExecute();
        base.OnPointerClick(eventData);
    }
}