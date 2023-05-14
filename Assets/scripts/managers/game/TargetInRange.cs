using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TargetInRange
{
    public TargetInRange(UnitController target, int alliance, bool inStop) 
    {
        Target = target;
        Alliance = alliance;
        InStopRange = inStop;
        targetalliance = alliance;
    }
    public UnitController Target { get; private set; }
    public int Alliance { get; private set; }
    public bool InStopRange { get; private set; }
    public int targetalliance;
}
