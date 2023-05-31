using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class AttackType
{
    public string attackType;
    public float damage;
    public float selectWeight;
    public string[] effects;
}
