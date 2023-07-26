[System.Serializable]
public class Stats
{
    // Stats
    public float MaxHp { get; set; }                    // Hp value used for resetting when dead
    public float hp;                                    // Unit's hp
    public float FlatDamage { get; set; } = 0f;         // Flat damage added per hit  
    public float DamageMultiplier { get; set; } = 1f;   // Multiplier damage added
    public float attack_range;                          // How far can this unit attack
    public bool useProjectile;                          // Does this unit shoot projectiles
    public float attack_delay = 0.1f;                   // Delay time between attacks
    public float attack_speed = 1f;                     // Speed multiplier of attack animations
    public string unitName;                             // Identify unit type
    public float stop_range;                            // When the unit stops moving
    public float maxSpeed;                              // Max speed of this unit

    // Armor
    public float slashArmor = 10f;
    public float thrustArmor = 10f;
    public float bluntArmor = 10f;
    public float specialArmor = 10f;
}
