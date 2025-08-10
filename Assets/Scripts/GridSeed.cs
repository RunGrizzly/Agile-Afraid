using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Flags]
public enum GridElementFlags
{
    None = 0,
    Start =1 << 0,
    End = 1 << 1,
    Bonus = 1 << 2,
    Required = 1 << 3,
    Empty = 1 << 4,
    Blocked = 1 << 5
}

[Serializable]
public class GridSeed
{
    public string Content;
    public GridElementFlags Flags;
    public Color Color;

    public GridSeed(string content, GridElementFlags flags )
    {
        Content = content;
        Flags = flags;
        Color = Color.gray;
    }
}