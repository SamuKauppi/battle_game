using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Upgrade
{
    public string upgradeTag;
    public string upgradeName;
    public StatModification[] statsAffected;
    public Dictionary<(string, string), StatModification> statsAffectedDict;
    public int upgradeLevel = 1;


    public void InitializeDictionary()
    {
        statsAffectedDict = new Dictionary<(string, string), StatModification>();
        foreach (StatModification statModification in statsAffected)
        {
            statsAffectedDict.Add((statModification.statAffected, statModification.modificationType), statModification);
        }
    }

    public Upgrade(string name, StatModification[] stats, int level)
    {
        upgradeTag = name;
        statsAffected = stats;
        upgradeLevel = level;
        InitializeDictionary();
    }

    public Upgrade(Upgrade previousUpgrade)
    {
        upgradeTag = previousUpgrade.upgradeTag;
        statsAffected = previousUpgrade.statsAffected;
        upgradeLevel = previousUpgrade.upgradeLevel;
        InitializeDictionary();
    }
}
