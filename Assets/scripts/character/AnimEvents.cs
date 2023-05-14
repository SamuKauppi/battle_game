using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEvents : MonoBehaviour
{
    [SerializeField] private UnitController controller;


    public void Attack_a()
    {
        controller.Attack(1);
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
