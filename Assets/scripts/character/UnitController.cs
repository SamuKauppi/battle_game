using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitController : MonoBehaviour
{
    public float hp;
    private float maxHp;
    [SerializeField] private float attack_range;
    [SerializeField] private float stop_range;
    // Armor
    [SerializeField] private float slashArmor;
    [SerializeField] private float thrustArmor;
    [SerializeField] private float bluntArmor;
    [SerializeField] private float specialArmor;

    [SerializeField] private int maxAttackTargets;

    [SerializeField] private Animator anim;

    [SerializeField] private float speed;
    private float movement;

    [SerializeField] private TargetInRange[] targets;
    private GameController manager;

    private float timer;

    public int Alliance { get; set; }
    public int Xindex { get; set; }
    public int Zindex { get; set; }
    public bool IsAlive { get { return hp > 0; } set { if (maxHp > 0) hp = maxHp; } }

    private void Start()
    {
        manager = GameController.Instance;
        maxHp = hp;
        movement = speed;
        SetMoving(false);
    }

    private void FixedUpdate()
    {
        if (!IsAlive)
            return;

        timer += Time.fixedDeltaTime;
        if (timer > 0.25f || targets.Length < 1)
        {
            timer = 0;
            CheckTargetsFront();
        }
        MoveForward();
    }

    private void SetMoving(bool value)
    {
        anim.SetBool("walk", value);
        if (value)
        {
            speed = movement;
        }
        else
        {
            speed = 0;
        }
    }
    private void MoveForward()
    {
        transform.position += speed * Time.fixedDeltaTime * transform.forward;
    }

    private void CheckTargetsFront()
    {
        targets = manager.CheckEnemy(transform.position,
            transform.position + transform.forward * attack_range,
            transform.position + transform.forward * stop_range,
            Alliance,
            Xindex,
            Zindex,
            maxAttackTargets);

        if (targets.Length < 1)
        {
            SetMoving(true);
            anim.SetBool("attack", false);
            return;
        }
        foreach (TargetInRange target in targets)
        {
            if (target.InStopRange)
            {
                SetMoving(false);
            }
            else
            {
                SetMoving(true);
            }

            if (target.Alliance == Alliance)
            {
                anim.SetBool("attack", false);
            }
            else
            {
                anim.SetBool("attack", true);
            }
        }
    }


    public void Attack(float damage, string damageType)
    {
        foreach (TargetInRange target in targets)
        {
            target.Target.TakeDamage(damage, damageType);
        }
    }

    public void TakeDamage(float amount, string damageType)
    {
        hp -= amount * Random.Range(0.85f, 1.15f);
        StartCoroutine(ApplyKnockBack(amount * 0.025f));
        if (hp <= 0)
        {
            anim.SetTrigger("die");
            SetMoving(false);
        }

    }
    IEnumerator ApplyKnockBack(float amount)
    {
        while (amount > 0)
        {
            yield return new WaitForFixedUpdate();
            transform.position -= amount * Time.fixedDeltaTime * transform.forward;
            amount *= 0.75f;
            amount -= 0.01f;
        }
    }

    public void OnDeath()
    {
        targets = new TargetInRange[0];
        Xindex = -1;
        Zindex = -1;
    }

    public void OnDespawn()
    {
        anim.SetTrigger("live");
        gameObject.SetActive(false);
    }
}
