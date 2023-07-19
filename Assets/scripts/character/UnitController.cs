using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Controls an unit
/// </summary>
public class UnitController : MonoBehaviour
{
    // References
    private GameController manager;
    private ObjectPooler pooler;
    [SerializeField] private Animator anim;
    public Renderer myRend;
    [SerializeField] private LazerGun gun;

    // Stats
    public string UnitName;                                 // Used for spawning
    public float hp;                                        // Units hp
    [SerializeField] private float attack_range;            // How far can this unit attack
    [SerializeField] private bool useProjectile;            // Does this unit shoot projectiles
    [SerializeField] private float attack_speed;            // Delay time between attacks
    private float attack_timer;
    [SerializeField] private float stop_range;          // When the unit stops moving
    [SerializeField] private float speed;               // How fast does this unit move

    // Upgrades
    [SerializeField] private string[] validUpgrades; // Upgrade names that affect this unit
    private HashSet<string> validUpgradeNames = new HashSet<string>();

    // Damage
    private float damageMultiplier = 1;    // Damage multiplier for effects
    private float flatDamage = 0f;         // Damage increase for effects
    private float upgradeMultiplier = 1;   // Damage multiplier through upgrades
    private float flatUpgrade = 0f;        // Damage increase through upgrades

    // Attaks
    [SerializeField] private int selectedAttackIndex;   // Current attack selected
    [SerializeField] private bool randomIndexAtStart;   // Should the unit start with random attack
    private int startingAttackIndex;                    // If the starting attack is not random, then save the starting index
    [SerializeField] private AttackType[] moveSet;      // Moves the unit uses
    private float totalAttackSelectWeight;              // Used for selecting moves based on it's weight value
    private bool disableTopLayer;                       // This prevents top animation layer to be turned off before an attack animation ends
    private bool shouldAttack;                          // Should the unit be attacking

    // Armor
    private float maxHp;                                // Hp value used for resetting when dead
    [SerializeField] private float slashArmor = 10;
    [SerializeField] private float thrustArmor = 10;
    [SerializeField] private float bluntArmor = 10;
    [SerializeField] private float specialArmor = 10;
    private float armorMultiplier = 1f;
    private float flatArmor = 0f;

    // Targeting
    [SerializeField] private TargetInRange[] targets;   // Targets. Both allies and enemies. Can be null
    private UnitController followTarget;                // Target this unit is matching it's speed
    private float distanceToClosestEnemy;               // Distance to the closest target
    private float targetUpdateTmr;                      // Timer for limiting how often units check for targets
    private const float targetUpdateThreshold = 0.1f;   // Threshold

    // Movement          
    private float movement;                             // Normal movement speed of this unit
    private float alteredMovement;                      // When this unit is faster than an ally in front of this, use their speed
    private bool isKnocked;                             // Stops moving while being knocked back
    public float GetSpeed { get { return speed; } }     // Return speed
    public float GetMovement { get { return movement; } }
    public int Alliance { get; set; }                   // Alliance of this unit
    public int Xindex { get; set; }                     // On which lane this unit is
    public int Zindex { get; set; }                     // Each unit in lane gets a Zindex to identify it
    public bool IsAlive { get { return hp > 0; } set { if (maxHp > 0) { hp = maxHp; } } } // Is this unit alive or set it alive

    #region Start/Update
    private void Start()
    {
        manager = GameController.Instance;
        pooler = ObjectPooler.Instance;
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
        foreach (string upgrade in validUpgrades)
        {
            validUpgradeNames.Add(upgrade);
        }
    }

    private void FixedUpdate()
    {
        // Do only if this unit is alive
        if (!IsAlive)
            return;

        // Check targets
        targetUpdateTmr += Time.fixedDeltaTime;
        if (targetUpdateTmr > targetUpdateThreshold)
        {
            targetUpdateTmr = 0;
            CheckTargetsFront();
        }


        // Increase attack timer
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
    #endregion
    #region Movement/Targeting

    /// <summary>
    ///  Sets unit's movement
    /// </summary>
    /// <param name="value"> Start or Stop moving </param>
    private void SetMoving(bool value)
    {
        // If this unit is being knocked back, stop walk animation
        if (isKnocked)
        {
            anim.SetBool("walk", false);
            speed = 0f;
            return;
        }

        // Set walk animation and it's speed based on the movement
        anim.SetBool("walk", value);
        if (value)
        {
            // Altered movement is based on the ally in front of this unit if it's slower
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

    /// <summary>
    ///  Moves unit forward
    /// </summary>
    private void MoveForward()
    {
        // move forward if it's not being knocked back
        if (isKnocked)
            return;

        transform.position += speed * Time.fixedDeltaTime * transform.forward;
    }

    /// <summary>
    /// Called when checking for targets
    /// </summary>
    private void CheckTargetsFront()
    {
        // Returns an array of units in front of this unit
        // This arrays length is maxtargets + 1 and the first value will be ally or null
        targets = manager.CheckEnemy(transform.position,
            transform.position + transform.forward * attack_range,
            transform.position + transform.forward * stop_range,
            Alliance,
            Xindex,
            Zindex,
            moveSet[selectedAttackIndex].maxTargets);

        // Iterate through targets found
        bool foundStopRangeTarget = false;
        bool foundEnemy = false;
        distanceToClosestEnemy = 0;
        foreach (TargetInRange target in targets)
        {
            if (target == null)
                continue;


            // If it's an enemy save it as closest enemy for other functions
            if (target.Alliance != Alliance)
            {
                foundEnemy = true;
                if (distanceToClosestEnemy > target.DistanceToTarget || distanceToClosestEnemy == 0)
                {
                    distanceToClosestEnemy = target.DistanceToTarget;
                }
            }

            // If the target is in stop range and is an ally that is slower, slow this unit to macth that unit
            if (target.InStopRange)
            {
                foundStopRangeTarget = true;
                followTarget = target.Target;

                if (target.Alliance == Alliance && target.Target.GetSpeed < movement)
                {
                    alteredMovement = target.Target.GetSpeed;
                }
                else
                {
                    alteredMovement = 0;
                }
            }
        }

        if (!foundStopRangeTarget && followTarget != null)
        {
            if (!followTarget.IsAlive)
            {
                alteredMovement = 0f;
                followTarget = null;
            }
        }

        // If an enemy was found, set the layer weight in top layer to 1. This let's the top part of the rig to attack
        if (foundEnemy)
            anim.SetLayerWeight(1, 1f);
        else
        {
            // If no enemies where found, then set disableTopLayer true to disable top layer after next attack
            disableTopLayer = true;
            // Distance to closest enemy will be changed to max range to pick correct attack
            distanceToClosestEnemy = attack_range;
        }

        // Stop movement if the current attack is set so
        if (!moveSet[selectedAttackIndex].stopMovement)
        {
            SetMoving(!foundStopRangeTarget);
        }
        else
        {
            // Otherwise stop movement when there is an target in stop range
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
    #endregion
    #region Attacking/Taking Damage
    /// <summary>
    ///  Called when an attack connects
    /// </summary>
    public void Attack()
    {
        // Iterate through targets with the current attack and apply damage
        AttackType attack = moveSet[selectedAttackIndex];
        float damage = attack.damage * upgradeMultiplier * damageMultiplier + flatDamage + flatUpgrade;
        foreach (TargetInRange target in targets)
        {
            if (target == null)
                continue;

            if (target.Alliance != Alliance)
            {
                if (useProjectile) // Check if it's a ranged unit with a projectile. Create projectile and apply damage in delay
                {
                    gun.ShootGun(target.Target.transform.position);
                    StartCoroutine(AttackWithDelay(target, damage, attack.attackType, target.DistanceToTarget / gun.Lazer_speed));
                }
                else
                {
                    target.Target.TakeDamage(damage, attack.attackType);
                }
            }
        }
    }

    /// <summary>
    /// Projectile attack delay
    /// </summary>
    /// <param name="target"> Who takes damage </param>
    /// <param name="attack"> The attack </param>
    /// <param name="delay"> How long the attack will be delayed </param>
    /// <returns></returns>
    private IEnumerator AttackWithDelay(TargetInRange target, float damage, string attackType, float delay)
    {
        yield return new WaitForSeconds(delay); // Delay based on the projectile travel time
        target.Target.TakeDamage(damage, attackType);
    }

    /// <summary>
    /// Called after an attack ends
    /// </summary>
    public void OnAttackEnd()
    {
        // Reset attack timer
        attack_timer = 0f;
        anim.SetBool("attack", false);

        // Generate a random value between 0 and the total weight
        float randomValue = Random.Range(0f, totalAttackSelectWeight);

        // Iterate through attacks to pick a new attack
        // Pick a random attack that has valid range (inside min and max range), picking the first valid target as default
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

        // Now disable top layer after attack ends and there are no targets in front of this unit
        if (disableTopLayer)
        {
            disableTopLayer = false;
            anim.SetLayerWeight(1, 0f);
            SetAttack();
        }

        // Stop movement if nessesary
        if (moveSet[selectedAttackIndex].stopMovement)
        {
            SetMoving(true);
        }
    }

    /// <summary>
    /// Sets the next attack to a random attack or to a specific if this unit will attack first with some attack
    /// </summary>
    private void SetAttack()
    {
        if (randomIndexAtStart)
        {
            distanceToClosestEnemy = attack_range;
            OnAttackEnd();
        }
        else
        {
            selectedAttackIndex = startingAttackIndex;
            anim.SetFloat("attackType", selectedAttackIndex);
        }
    }

    /// <summary>
    /// This unit takes damage
    /// </summary>
    /// <param name="amount"> damage </param>
    /// <param name="damageType"> type </param>
    public void TakeDamage(float amount, string damageType)
    {
        if (!IsAlive)
            return;

        // Get the multiplier for this attack type
        var multiplier = damageType switch
        {
            "thurst" => GetDamageModifier(thrustArmor),
            "blunt" => GetDamageModifier(bluntArmor),
            "special" => GetDamageModifier(specialArmor),
            _ => GetDamageModifier(slashArmor),
        };
        // Calculate damage and take damage
        float damage = amount * Random.Range(0.85f, 1.16f) * multiplier;
        hp -= damage;

        pooler.GetPooledObject("sparks", transform.position);

        // Apply knockback
        StartCoroutine(ApplyKnockBack(damage * 0.017f));

        // Is this units hp falls below 0, it dies
        if (hp <= 0)
        {
            anim.SetTrigger("die");
            SetMoving(false);
            pooler.GetPooledObject("explosion", transform.position);
        }
    }
    /// <summary>
    /// Calculates damage multiplier
    /// </summary>
    /// <param name="armorValue"> armor value </param>
    /// <returns></returns>
    private float GetDamageModifier(float armorValue)
    {
        float changedArmor = armorValue * armorMultiplier + flatArmor;
        return 1 - (changedArmor / (40 + Mathf.Abs(changedArmor)));
    }

    /// <summary>
    /// Applies knockback
    /// </summary>
    /// <param name="amount"> Knockback strength </param>
    /// <returns></returns>
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


    /// <summary>
    /// On the first frame of death animation
    /// </summary>
    public void OnDeath()
    {
        Xindex = -1;
        Zindex = -1;
        anim.SetLayerWeight(1, 0f);
        alteredMovement = 0;
        SetAttack();
    }

    /// <summary>
    /// On death animation end
    /// </summary>
    public void OnDespawn()
    {
        anim.SetTrigger("live");
        gameObject.SetActive(false);
    }
    #endregion
    public void ApplyUpgrades(Upgrade[] upgrades)
    {
        foreach (Upgrade upgrade in upgrades)
        {
            if (validUpgradeNames.Contains(upgrade.upgradeName))
            {
                switch (upgrade.upgradeName)
                {

                    default:
                        break;
                }
            }
        }
    }
}
