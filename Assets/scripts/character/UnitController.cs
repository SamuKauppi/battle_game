using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    public string UnitName { get; private set; }        // To access the name
    private float attack_timer;                         // Timer for attacks
    [SerializeField] private Stats currentStats;

    // Upgrades
    private Stats baseStats;                                                        // Used to store the base stats for resetting
    [SerializeField] private string[] validUpgradeNames;                            // Upgrade names that affect this unit
    private readonly Dictionary<string, Upgrade> appliedUpgrades = new();           // Upgrades applied to this unit
    private readonly Dictionary<(string, string), float> statModifications = new(); // Stat modifications that are applied to this unit

    // Attaks
    [SerializeField] private int selectedAttackIndex;   // Current attack selected
    [SerializeField] private bool randomIndexAtStart;   // Should the unit start with random attack
    private int startingAttackIndex;                    // If the starting attack is not random, then save the starting index
    [SerializeField] private AttackType[] moveSet;      // Moves the unit uses
    private float totalAttackSelectWeight;              // Used for selecting moves based on it's weight value
    private bool disableTopLayer;                       // This prevents top animation layer to be turned off before an attack animation ends
    private bool shouldAttack;                          // Should the unit be attacking

    // Targeting
    [SerializeField] private TargetInRange[] targets;   // Targets. Both allies and enemies. Can be null
    private UnitController followTarget;                // Target this unit is matching it's speed
    private float distanceToClosestEnemy;               // Distance to the closest target
    private float targetUpdateTmr;                      // Timer for limiting how often units check for targets
    private const float targetUpdateThreshold = 0.05f;  // Threshold

    // Movement          
    [SerializeField] private float currentSpeed;            // Current speed of the unit
    private float alteredSpeed;                             // When this unit is faster than an ally in front of this, use their speed
    private bool isKnocked;                                 // Stops moving while being knocked back
    private float GetSpeed { get { return currentSpeed; } } // Return speed

    // Other
    private int deathIndex;                                 // Changes death animation based how much damage unit takes
    [SerializeField] private Transform deathPos;            // Transform of chect bone. Used to spawn explosion to correct pos
    public int Alliance { get; set; }                       // Alliance of this unit
    public int Xindex { get; set; }                         // On which lane this unit is
    public int Zindex { get; set; }                         // Each unit in lane gets a Zindex to identify it
    public bool IsAlive
    {
        get { return currentStats.hp > 0; }
        set { if (currentStats.MaxHp > 0) currentStats.hp = currentStats.MaxHp; }
    }  // Is this unit alive or set it alive



    #region Start/Update
    private void Start()
    {
        manager = GameController.Instance;
        pooler = ObjectPooler.Instance;
        UnitName = currentStats.unitName;
        // Get the total weight of all of the movesets
        for (int i = 0; i < moveSet.Length; i++)
        {
            totalAttackSelectWeight += moveSet[i].selectWeight;
            moveSet[i].minSelectWeightRange = totalAttackSelectWeight - moveSet[i].selectWeight;
            moveSet[i].maxSelectWeightRange = totalAttackSelectWeight;
        }
    }

    /// <summary>
    /// After Upgrades have been applied, save some stats and variables
    /// </summary>
    public void OnUnitSpawn()
    {
        // Apply attack speed
        anim.SetFloat("attack_speed", currentStats.attack_speed);

        // Set attack index to random index or to a specific index
        startingAttackIndex = selectedAttackIndex;
        SetAttack();

        // Save max hp
        currentStats.MaxHp = currentStats.hp;
        currentSpeed = currentStats.maxSpeed;

        // Stop moving at the beginning
        SetMoving(false);
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
            if (attack_timer > currentStats.attack_delay)
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
            currentSpeed = 0f;
            return;
        }

        // Set walk animation and it's speed based on the movement
        anim.SetBool("walk", value);
        if (value)
        {
            // Altered movement is based on the ally in front of this unit if it's slower
            if (alteredSpeed > 0)
                currentSpeed = alteredSpeed;
            else
                currentSpeed = currentStats.maxSpeed;

            anim.SetFloat("walking_speed", currentSpeed);
        }
        else
        {
            currentSpeed = 0f;
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

        transform.position += currentSpeed * Time.fixedDeltaTime * transform.forward;
    }

    /// <summary>
    /// Called when checking for targets
    /// </summary>
    private void CheckTargetsFront()
    {
        // Returns an array of units in front of this unit
        // This arrays length is maxtargets + 1 and the first value will be ally or null
        targets = manager.CheckForTargets(transform.position,
            transform.position + transform.forward * currentStats.attack_range,
            transform.position + transform.forward * currentStats.stop_range,
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

                if (target.Alliance == Alliance && target.Target.GetSpeed < currentSpeed)
                {
                    alteredSpeed = target.Target.GetSpeed;
                }
                else
                {
                    alteredSpeed = 0;
                }
            }
        }

        if (!foundStopRangeTarget && followTarget != null)
        {
            if (!followTarget.IsAlive)
            {
                alteredSpeed = 0f;
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
            distanceToClosestEnemy = currentStats.attack_range;
        }

        // Stop movement if the current attack stops movement
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
        float damage = attack.damage * currentStats.DamageMultiplier + currentStats.FlatDamage;
        foreach (TargetInRange target in targets)
        {
            if (target == null)
                continue;

            if (target.Alliance == Alliance)
            {
                continue;
            }

            if (currentStats.useProjectile) // Check if it's a ranged unit with a projectile. Create projectile and apply damage in delay
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
            distanceToClosestEnemy = currentStats.attack_range;
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

        // Define the armor type based on the damage type
        float armorValue = damageType switch
        {
            "thurst" => currentStats.thrustArmor,
            "blunt" => currentStats.bluntArmor,
            "special" => currentStats.specialArmor,
            _ => currentStats.slashArmor,
        };

        // Get the multiplier for this attack type
        float resistanceMultiplier = GetDamageModifier(armorValue);

        // Calculate damage and take damage
        float damage = amount * resistanceMultiplier * Random.Range(0.95f, 1.05f);
        currentStats.hp -= damage;

        pooler.GetPooledObject("sparks", transform.position);

        // Apply knockback
        StartCoroutine(ApplyKnockBack(damage * 0.017f));

        // Is this units hp falls below 0, it dies
        if (currentStats.hp <= 0)
        {
            float hp = currentStats.hp;
            float hpThreshold = currentStats.MaxHp * -0.2f;
            if (hp > hpThreshold)
                deathIndex = 0;
            else if (hp < hpThreshold && hp > hpThreshold * 2)
                deathIndex = 1;
            else
                deathIndex = 2;

            anim.SetLayerWeight(1, 0f);
            anim.SetFloat("deathIndex", deathIndex);
            anim.SetTrigger("die");
        }
    }
    /// <summary>
    /// Calculates damage multiplier
    /// </summary>
    /// <param name="armorValue"> armor value </param>
    /// <returns></returns>
    private float GetDamageModifier(float armorValue)
    {
        return 1 - (armorValue / (40 + Mathf.Abs(armorValue)));
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
        SpawnElectricity();
        Xindex = -1;
        Zindex = -1;
        alteredSpeed = 0;
        SetAttack();
        SetMoving(false);
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
    #region Upgrading
    /// <summary>
    /// Add multiple upgrades then apply them
    /// </summary>
    /// <param name="newUpgrades"></param>
    public void AddUpgrades(Upgrade[] newUpgrades)
    {
        if (newUpgrades.Length < 1)
        {
            return;
        }

        foreach (Upgrade upgrade in newUpgrades)
        {
            AddUpgrade(upgrade);
        }
        ApplyUpgrades();
    }

    /// <summary>
    /// Add single upgrade and then apply it if not adding multiple upgrades
    /// </summary>
    /// <param name="newUpgrade"></param>
    private void AddUpgrade(Upgrade newUpgrade)
    {
        if (!validUpgradeNames.Contains(newUpgrade.upgradeTag))
        {
            return; // Skip invalid upgrades
        }

        if (appliedUpgrades.ContainsKey(newUpgrade.upgradeTag))
        {
            appliedUpgrades[newUpgrade.upgradeTag] = newUpgrade; // Overwrite the existing upgrade
        }
        else
        {
            appliedUpgrades.Add(newUpgrade.upgradeTag, newUpgrade); // Add the new upgrade
        }
    }
    /// <summary>
    /// Apply upgrades to current stats
    /// </summary>
    private void ApplyUpgrades()
    {
        SaveOrLoadBaseStats();
        Dictionary<(string, string), float> statModifications = CalculateStatModifications();

        foreach (KeyValuePair<(string, string), float> statMod in statModifications)
        {
            switch (statMod.Key.Item1)
            {
                case "damage":
                    if (statMod.Key.Item2.Equals("multi"))
                    {
                        currentStats.DamageMultiplier = PerformOperation(currentStats.DamageMultiplier,
                            statMod.Value, statMod.Key.Item2);
                    }
                    else
                    {
                        currentStats.FlatDamage = PerformOperation(currentStats.FlatDamage,
                            statMod.Value, statMod.Key.Item2);
                    }
                    break;
                case "range":
                    currentStats.attack_range = PerformOperation(currentStats.attack_range,
                        statMod.Value, statMod.Key.Item2);
                    break;
                case "delay":
                    currentStats.attack_delay = PerformOperation(currentStats.attack_delay,
                        statMod.Value, statMod.Key.Item2);
                    break;
                case "as":
                    currentStats.attack_speed = PerformOperation(currentStats.attack_speed,
                        statMod.Value, statMod.Key.Item2);
                    break;
                case "speed":
                    currentStats.maxSpeed = PerformOperation(currentStats.maxSpeed,
                        statMod.Value, statMod.Key.Item2);
                    break;
                case "hp":
                    currentStats.hp = PerformOperation(currentStats.hp,
                        statMod.Value, statMod.Key.Item2);
                    break;
                default:
                    currentStats.slashArmor = PerformOperation(currentStats.slashArmor,
                        statMod.Value, statMod.Key.Item2);
                    currentStats.thrustArmor = PerformOperation(currentStats.thrustArmor,
                        statMod.Value, statMod.Key.Item2);
                    currentStats.bluntArmor = PerformOperation(currentStats.bluntArmor,
                        statMod.Value, statMod.Key.Item2);
                    currentStats.specialArmor = PerformOperation(currentStats.specialArmor,
                        statMod.Value, statMod.Key.Item2);
                    break;
            }
        }
    }

    private Dictionary<(string, string), float> CalculateStatModifications()
    {
        statModifications.Clear();
        foreach (KeyValuePair<string, Upgrade> upgrade in appliedUpgrades)
        {
            foreach (KeyValuePair<(string, string), StatModification> statMod in upgrade.Value.statsAffectedDict)
            {
                if (statModifications.TryGetValue(statMod.Key, out float amount))
                {
                    statModifications[statMod.Key] = amount + (statMod.Value.amount * upgrade.Value.upgradeLevel);
                }
                else
                {
                    statModifications.Add(statMod.Key, statMod.Value.amount * upgrade.Value.upgradeLevel);
                }
            }
        }
        // Return sorted dictionary in reverse order
        // This makes multiplications before flat changes
        return statModifications.OrderByDescending(kv => kv.Key.Item2).ToDictionary(kv => kv.Key, kv => kv.Value);
    }


    private float PerformOperation(float value1, float value2, string type)
    {
        switch (type)
        {
            case "multi":
                return value1 * (value2 + 1);

            case "flat":
                return value1 + value2;
            default:
                Debug.Log(type + " was not found!");
                return value1;
        }
    }

    /// <summary>
    /// If base stats have not been assinged, save them
    /// Otherwise load them
    /// </summary>
    private void SaveOrLoadBaseStats()
    {
        if (baseStats == null)
        {
            baseStats = currentStats;
            baseStats.MaxHp = baseStats.hp;
        }
        else
        {
            currentStats = baseStats;
        }
    }


    public void ResetUpgrades()
    {
        appliedUpgrades.Clear();
    }
    #endregion
    #region Other
    public void ConvertUnit()
    {
        Alliance *= -1;
        transform.Rotate(Vector3.up, 180f);
    }

    /// <summary>
    /// Spawn explosion. Called from animation
    /// </summary>
    public void SpawnExplosion()
    {
        if (deathPos == null)
            deathPos = transform;

        pooler.GetPooledObject("explosion", deathPos.position);
    }

    private void SpawnElectricity()
    {
        pooler.GetPooledObject("electric", transform.position);
    }
    #endregion
}
