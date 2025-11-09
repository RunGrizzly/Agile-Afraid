using UnityEngine;

public class TooltipProvider : MonoBehaviour
{
    public void ShowTooltip()
    {
        Debug.LogFormat(GetTooltipInfo());
    }
    private string GetTooltipInfo()
    {
        return "Tooltip Info";
    }
}
