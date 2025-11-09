using UnityEngine.Events;

public interface IGeoButton
{
    public UnityEvent SetSelected { get; }
    public UnityEvent SetHovered { get; }
    public UnityEvent SetUnhovered { get; }

    //Private Reaction To Hover
    public void OnSetHovered();
}
