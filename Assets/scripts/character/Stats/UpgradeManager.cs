using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{

    private readonly Dictionary<StatType, string> statPropertyMap = new Dictionary<StatType, string>
{
    { StatType.Damage, "DamageMultiplier" },
    { StatType.Range, "attack_range" },
    { StatType.Delay, "attack_delay" },
    { StatType.AttackSpeed, "attack_speed" },
    { StatType.Speed, "maxSpeed" },
    { StatType.HP, "hp" }
    };

    private readonly Dictionary<ModificationType, Func<float, float, float>> modificationMap = new Dictionary<ModificationType, Func<float, float, float>>
{
    { ModificationType.Add, (currentValue, amount) => currentValue + amount },
    { ModificationType.Multiply, (currentValue, amount) => currentValue * amount }
    };

    private void ApplyUpgrades(Upgrade upgrade, Stats currentStats)
    {
        foreach (StatModification statMod in upgrade.statsAffected)
        {
            if (statPropertyMap.TryGetValue(statMod.statAffected, out string propertyName)
                && modificationMap.TryGetValue(GetModificationType(statMod.modificationType), out var modificationFunc))
            {
                PropertyInfo propInfo = typeof(Stats).GetProperty(propertyName);
                if (propInfo != null)
                {
                    float currentValue = (float)propInfo.GetValue(currentStats);
                    float newValue = modificationFunc(currentValue, statMod.amount * upgrade.upgradeLevel);
                    propInfo.SetValue(currentStats, newValue);
                }
            }
        }
    }

    private ModificationType GetModificationType(string modificationType)
    {
        return Enum.TryParse(modificationType, out ModificationType result) ? result : ModificationType.Add;
    }


}
public enum StatType
{
    Damage,
    Range,
    Delay,
    AttackSpeed,
    Speed,
    HP
}

public enum ModificationType
{
    Add,
    Multiply
}

