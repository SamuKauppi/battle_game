using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitController : MonoBehaviour
{
    // References
    private GameController manager;

    // Animations
    [SerializeField] private Animator anim;

    // Stats
    public float hp;
    private float maxHp;
    [SerializeField] private float damageMultiplier = 1;
    [SerializeField] private float attack_range;
    [SerializeField] private float stop_range;

    // Attaks
    [SerializeField] private AttackType[] moveSet;
    private float totalAttackSelect;
    private bool animLayrWeight;

    // Armor
    [SerializeField] private float slashArmor = 10;
    [SerializeField] private float thrustArmor = 10;
    [SerializeField] private float bluntArmor = 10;
    [SerializeField] private float specialArmor = 10;

    // Targeting
    [SerializeField] private TargetInRange[] targets;
    [SerializeField] private int maxAttackTargets;
    private float targetUpdateTmr;

    // Movement
    [SerializeField] private float speed;
    private float movement;
    private bool isKnocked;
    public int Alliance { get; set; }
    public int Xindex { get; set; }
    public int Zindex { get; set; }
    public bool IsAlive { get { return hp > 0; } set { if (maxHp > 0) { hp = maxHp; anim.SetFloat("walking_speed", movement); } } }

    private void Start()
    {
        manager = GameController.Instance;
        maxHp = hp;
        movement = speed;  
        SetMoving(false);
        OnAttackEnd();

        for (int i = 0; i < moveSet.Length; i++)
        {
            totalAttackSelect += moveSet[i].selectWeight;
        }
        IsAlive = true;
    }

    private void FixedUpdate()
    {
        if (!IsAlive)
            return;

        targetUpdateTmr += Time.fixedDeltaTime;
        if (targetUpdateTmr > 0.1f || targets.Length < 1)
        {
            targetUpdateTmr = 0;
            CheckTargetsFront();
        }
        MoveForward();
    }
    public void SetMoving(bool value)
    {
        if (isKnocked)
            return;

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

        bool foundStopRangeTarget = false;
        bool foundEnemy = false;

        foreach (TargetInRange target in targets)
        {
            if (target == null)
                continue;

            if (target.InStopRange)
            {
                foundStopRangeTarget = true;
            }

            if (target.Alliance != Alliance)
            {
                foundEnemy = true;
            }
        }

        if (foundEnemy)
            anim.SetLayerWeight(1, 1f);
        else
            animLayrWeight = true;

        SetMoving(!foundStopRangeTarget);
        anim.SetBool("attack", foundEnemy);
    }

    public void Attack(int attackIndex)
    {
        if (moveSet.Length <= attackIndex)
            return;

        AttackType attack = moveSet[attackIndex];
        foreach (TargetInRange target in targets)
        {
            if (target == null)
                continue;

            if (target.Alliance != Alliance)
            {
                target.Target.TakeDamage(attack.damage * damageMultiplier, attack.attackType);
            }
        }
    }

    public void OnAttackEnd()
    {

        // Generate a random value between 0 and the total weight
        float randomValue = Random.Range(0f, totalAttackSelect);

        // Iterate through the attacks and find the one corresponding to the random value
        float cumulativeWeight = 0f;
        int selectedAttackIndex = 0;
        foreach (var damageType in moveSet)
        {
            cumulativeWeight += damageType.selectWeight;
            if (randomValue <= cumulativeWeight)
            {
                break;
            }
            selectedAttackIndex++;
        }

        // Set the selected attack index in the animator
        anim.SetFloat("attackType", selectedAttackIndex);

        if (animLayrWeight)
        {
            animLayrWeight = false;
            anim.SetLayerWeight(1, 0f);
        }
    }

    public void TakeDamage(float amount, string damageType)
    {
        var multiplier = damageType switch
        {
            "thurst" => GetDamageModifier(thrustArmor),
            "blunt" => GetDamageModifier(bluntArmor),
            "special" => GetDamageModifier(specialArmor),
            _ => GetDamageModifier(slashArmor),
        };
        hp -= amount * Random.Range(0.80f, 1.21f) * multiplier;

        StartCoroutine(ApplyKnockBack(amount * 0.015f));
        if (hp <= 0)
        {
            anim.SetTrigger("die");
            SetMoving(false);
        }
    }
    private float GetDamageModifier(float armorValue)
    {
        return 1 - (armorValue / (40 + Mathf.Abs(armorValue)));
    }

    IEnumerator ApplyKnockBack(float amount)
    {
        SetMoving(false);
        isKnocked = true;
        while (amount > 0)
        {
            yield return null;
            transform.position -= amount * Time.fixedDeltaTime * transform.forward;
            amount *= 0.75f;
            amount -= 0.01f;
        }
        isKnocked = false;
    }

    public void OnDeath()
    {
        targets = new TargetInRange[0];
        Xindex = -1;
        Zindex = -1;
        anim.SetLayerWeight(1, 0f);
    }

    public void OnDespawn()
    {
        anim.SetTrigger("live");
        gameObject.SetActive(false);
    }
}
