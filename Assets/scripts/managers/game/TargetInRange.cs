using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TargetInRange
{
    public TargetInRange(UnitController target, int alliance, bool inStop, float distance) 
    {
        Target = target;
        Alliance = alliance;
        InStopRange = inStop;
        DistanceToTarget = distance;
    }
    public UnitController Target { get; private set; }
    public int Alliance { get; private set; }
    public bool InStopRange { get; private set; }
    public float DistanceToTarget { get; private set; }
}
