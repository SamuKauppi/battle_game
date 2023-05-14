using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AnimEvents : MonoBehaviour
{
    [SerializeField] private UnitController controller;


    public void Attack(string attackParameter)
    {
        string damage = "";
        string type = "";
        foreach (char s in attackParameter)
        {
            if (Char.IsDigit(s))
            {
                damage += s;
            }
            else
            {
                type += s;
            }
        }
        controller.Attack(int.Parse(damage), type);
    }

    public void Death()
    {
        controller.OnDeath();
    }

    public void Despawn()
    {
        controller.OnDespawn();
    }
}
