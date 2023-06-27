using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class UnitController : MonoBehaviour
{
    // References
    private GameController manager;
    [SerializeField] private Animator anim;

    // Stats
    public string UnitName;                             // Used for spawning
    public float hp;                                    // Units hp
    [SerializeField] private float damageMultiplier = 1;// Damage increase
    [SerializeField] private float attack_range;        // How far can this unit attack
    [SerializeField] private float attack_speed;        // Delay time between attacks
    private float attack_timer;
    [SerializeField] private float stop_range;          // When the unit stops moving
    [SerializeField] private float speed;               // How fast does this unit move

    // Attaks
    [SerializeField] private int selectedAttackIndex;   // Current attack selected
    [SerializeField] private bool randomIndexAtStart;   // Should the unit start with random attack
    private int startingAttackIndex;                    // If the starting attack is not random, then save the starting index
    [SerializeField] private AttackType[] moveSet;      // Moves the unit uses
    private float totalAttackSelectWeight;              // Used for selecting moves based on it's weight value
    private bool animLayrWeight;                        // Used for tracking the weight of the top animation layer. When attacking = 1, otherwise = 0
    private bool shouldAttack;                          // Should the unit be attacking

    // Armor
    private float maxHp;                                // Hp value used for resetting when dead
    [SerializeField] private float slashArmor = 10;
    [SerializeField] private float thrustArmor = 10;
    [SerializeField] private float bluntArmor = 10;
    [SerializeField] private float specialArmor = 10;

    // Targeting
    [SerializeField] private TargetInRange[] targets;   // Targets. Both allies and enemies. Can be null
    private float distanceToClosestEnemy;               // Distance to the closest target
    private float targetUpdateTmr;                      // Timer for limiting how often units check for targets
    private const float targetUpdateThreshold = 0.1f;   // Threshold

    // Movement          
    private float movement;                             // Normal movement speed of this unit
    private float alteredMovement;                      // When this unit is faster than an ally in front of this, use their speed
    private bool isKnocked;                             // Stops moving while being knocked back
    public float GetSpeed { get { return speed; } }     // Return speed
    public int Alliance { get; set; }                   // Alliance of this unit
    public int Xindex { get; set; }                     // On which lane this unit is
    public int Zindex { get; set; }                     // Each unit in lane gets a Zindex to identify it
    public bool IsAlive { get { return hp > 0; } set { if (maxHp > 0) { hp = maxHp; } } } // Is this unit alive or set it alive

    private void Start()
    {
        manager = GameController.Instance;
        maxHp = hp;
        movement = speed;
        // Stop moving at the beginning
        SetMoving(false);
        // Set attack index to random index or to a specific index
        startingAttackIndex = selectedAttackIndex;
        SetAttack();
        // Get the total weight of all of the movesets
        for (int i = 0; i < moveSet.Length; i++)
        {
            totalAttackSelectWeight += moveSet[i].selectWeight;
            moveSet[i].minSelectWeightRange = totalAttackSelectWeight - moveSet[i].selectWeight;
            moveSet[i].maxSelectWeightRange = totalAttackSelectWeight;
        }
    }

    private void FixedUpdate()
    {
        if (!IsAlive)
            return;

        // Check targets
        targetUpdateTmr += Time.fixedDeltaTime;
        if (targetUpdateTmr > targetUpdateThreshold)
        {
            targetUpdateTmr = 0;
            CheckTargetsFront();
        }

        attack_timer += Time.fixedDeltaTime;
        // Attack if possible
        if (shouldAttack)
        {
            if (attack_timer > attack_speed)
            {
                anim.SetBool("attack", true);
                attack_timer = 0f;
            }
            else
            {
                anim.SetBool("attack", false);
            }
        }


        // Apply movement
        MoveForward();
    }

    public void SetMoving(bool value)
    {
        if (isKnocked)
        {
            anim.SetBool("walk", false);
            speed = 0f;
            return;
        }


        anim.SetBool("walk", value);
        if (value)
        {
            if (alteredMovement > 0)
                speed = alteredMovement;
            else
                speed = movement;

            anim.SetFloat("walking_speed", speed);
        }
        else
        {
            speed = 0f;
        }
    }
    private void MoveForward()
    {
        if (isKnocked)
            return;

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
            moveSet[selectedAttackIndex].maxTargets);

        bool foundStopRangeTarget = false;
        bool foundEnemy = false;
        distanceToClosestEnemy = 0;

        foreach (TargetInRange target in targets)
        {
            if (target == null)
                continue;

            if (target.InStopRange)
            {
                foundStopRangeTarget = true;
                if (target.Alliance == Alliance && target.Target.GetSpeed < movement)
                {
                    alteredMovement = target.Target.GetSpeed;
                }
                else
                {
                    alteredMovement = 0;
                }
            }

            if (target.Alliance != Alliance)
            {
                foundEnemy = true;
                if (distanceToClosestEnemy > target.DistanceToTarget || distanceToClosestEnemy == 0)
                {
                    distanceToClosestEnemy = target.DistanceToTarget;
                }
            }
        }

        if (foundEnemy)
            anim.SetLayerWeight(1, 1f);
        else
        {
            animLayrWeight = true;
            distanceToClosestEnemy = attack_range;
        }


        if (!moveSet[selectedAttackIndex].stopMovement)
        {
            SetMoving(!foundStopRangeTarget);
        }
        else
        {
            if (!foundEnemy)
            {
                SetMoving(!foundStopRangeTarget);
            }
            else
            {
                SetMoving(false);
            }
        }

        shouldAttack = foundEnemy;
    }
    //public void OnAttackStart()
    //{
    //    if (moveSet[selectedAttackIndex].stopMovement)
    //    {
    //        SetMoving(false);
    //    }
    //}
    public void Attack()
    {
        AttackType attack = moveSet[selectedAttackIndex];
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
        attack_timer = 0f;
        anim.SetBool("attack", false);
        // Generate a random value between 0 and the total weight
        float randomValue = Random.Range(0f, totalAttackSelectWeight);

        bool hasSelected = false;
        for (int i = 0; i < moveSet.Length; i++)
        {
            if (distanceToClosestEnemy > moveSet[i].maxRange || distanceToClosestEnemy < moveSet[i].minRange)
            {
                continue;
            }

            if (!hasSelected)
            {
                hasSelected = true;
                selectedAttackIndex = i;
                continue;
            }

            if (randomValue <= moveSet[i].maxSelectWeightRange && randomValue >= moveSet[i].minSelectWeightRange)
            {
                selectedAttackIndex = i;
                break;
            }
        }

        //if (UnitName.Equals("spear") && Alliance == 1)
        //{
        //    Debug.Log(selectedAttackIndex + ", " + distanceToClosestEnemy + ", " + hasSelected);
        //}

        // Set the selected attack index in the animator
        anim.SetFloat("attackType", selectedAttackIndex);

        if (animLayrWeight)
        {
            animLayrWeight = false;
            anim.SetLayerWeight(1, 0f);
        }

        if (moveSet[selectedAttackIndex].stopMovement)
        {
            SetMoving(true);
        }
    }

    private void SetAttack()
    {
        if (randomIndexAtStart)
        {
            OnAttackEnd();
        }
        else
        {
            selectedAttackIndex = startingAttackIndex;
            anim.SetFloat("attackType", selectedAttackIndex);
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
        float damage = amount * Random.Range(0.85f, 1.16f) * multiplier;
        hp -= damage;

        StartCoroutine(ApplyKnockBack(damage * 0.017f));
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
        Xindex = -1;
        Zindex = -1;
        anim.SetLayerWeight(1, 0f);
        alteredMovement = 0;
        SetAttack();
    }

    public void OnDespawn()
    {
        anim.SetTrigger("live");
        gameObject.SetActive(false);
    }
}
