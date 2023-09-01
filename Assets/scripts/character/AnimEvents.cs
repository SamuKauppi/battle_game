using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AnimEvents : MonoBehaviour
{
    [SerializeField] private UnitController controller;

    public void Attack()
    {
        controller.Attack();
    }
    public void Explode()
    {
        controller.SpawnExplosion();
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
