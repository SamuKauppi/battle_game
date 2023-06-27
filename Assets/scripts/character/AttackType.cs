using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class AttackType
{
    public string attackType;
    public float damage;
    public int maxTargets = 1;
    public float selectWeight;
    [HideInInspector]
    public float minSelectWeightRange;
    [HideInInspector]
    public float maxSelectWeightRange;
    public string[] effects;
    public float maxRange = 1f;
    public float minRange;
    public bool stopMovement;
}
