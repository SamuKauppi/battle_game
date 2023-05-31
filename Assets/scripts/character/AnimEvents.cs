using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AnimEvents : MonoBehaviour
{
    [SerializeField] private UnitController controller;

    public void Attack(int i)
    {
        controller.Attack(i);
    }

    public void EndAttack()
    {
        controller.OnAttackEnd();
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
