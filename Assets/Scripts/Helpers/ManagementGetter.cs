using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BrainControl
{
    public static Brain Get()
    {
        return Application.isPlaying ? Brain.ins : GameObject.FindGameObjectWithTag("Brain").GetComponent<Brain>();
    }
}
