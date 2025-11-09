using System;
using UnityEngine;

[Serializable]
public class TimePip
{
    public TimePoolType TimePoolType = TimePoolType.Unknown;
    public float MaxValue = 5;
    public float CurrentValue = 5;
    public TimePipWidget Widget = null;

    public TimePip(TimePoolType timePoolType, float maxValue, float startValue)
    {
        //Debug.LogFormat("Added a new time pip");
        TimePoolType = timePoolType;
        MaxValue = maxValue;
        CurrentValue = startValue;
    }
    
    public float AddTime(float value)
    {
        float space = MaxValue - CurrentValue;
        float added = Mathf.Min(space, value);
        CurrentValue += added;
        
        Widget.PipFace.fillAmount = CurrentValue / MaxValue;
        
        return value - added; // leftover
    }

    public float RemoveTime(float value)
    {
        //Debug.LogFormat($"Removing time from {this} of type {TimePoolType}");
        float removed = Mathf.Min(CurrentValue, value);
        CurrentValue -= removed;

        Widget.PipFace.fillAmount = CurrentValue / MaxValue;
        
        return value - removed; // leftover to remove
    }
}