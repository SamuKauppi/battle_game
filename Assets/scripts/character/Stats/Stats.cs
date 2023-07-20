using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stats
{
    // Stats
    public float maxHp;                 // Hp value used for resetting when dead
    public float hp;                    // Unit's hp
    public float attack_range;          // How far can this unit attack
    public bool useProjectile;          // Does this unit shoot projectiles
    public float attack_speed;          // Delay time between attacks
    public string unitName;             // Identify unit type
    public float stop_range;            // When the unit stops moving
    public float movement;              // Max speed of this unit

    // Armor
    public float slashArmor = 10;
    public float thrustArmor = 10;
    public float bluntArmor = 10;
    public float specialArmor = 10;

    public bool IsAlive { get { return hp > 0; } set { if (maxHp > 0) { hp = maxHp; } } } // Is this unit alive or set it alive
}
