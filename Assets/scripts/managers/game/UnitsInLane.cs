using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitsInLane
{
    public List< UnitController> units = new();
    public Transform xPos;
    public int indexCounter = 0;
}
