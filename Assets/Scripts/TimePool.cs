using System;
using System.Collections.Generic;
using System.Linq;
using CodingJar;
using UnityEngine;

[Serializable]
public class TimePool
{
    [Readonly]
    public List<TimePip> TimePips = new List<TimePip>();
    
    public float CurrentValue
    {
        get
        {
            float current = 0;
            
            foreach (TimePip pip in TimePips)
            {
                current += pip.CurrentValue;
            }

            return current;
        }
    }
    
    public float MaxValue
    {
        get
        {
            float max = 0;
            
            foreach (TimePip pip in TimePips)
            {
                max += pip.MaxValue;
            }
            
            return max;
        }
    }
    
    public TimePool(List<TimePip> timePips)
    {
        TimePips = timePips;
    }

    public float AddWithOverflow(float addValue)
    {
        float remaining = addValue;

        foreach (var pip in TimePips)
        {
            if (remaining <= 0)
            {
                break;
            }
            
            remaining = pip.AddTime(remaining);
        }

        return remaining; // overflow left after filling all pips
    }

    public float RemoveWithOverflow(float elapseValue, bool killOnDeplete = false)
    {
        float remaining = elapseValue;
        
        foreach (var pip in TimePips.AsEnumerable().Reverse())
        {
            if (remaining <= 0)
            {
                break;
            }
          
            remaining = pip.RemoveTime(remaining);
            
            //If a pip depleted remove it
            if (killOnDeplete)
            {
                //If there is still grey remaining
                //Then the previous pip was depleted
                if (remaining > 0)
                {
                    GameObject.Destroy(pip.Widget.gameObject);
                    TimePips.Remove(pip);
                    BrainControl.Get().eventManager.e_pipRemoved.Invoke(pip);
                }
            }
        }

        return remaining; // amount we tried to remove but couldn’t
    }
}