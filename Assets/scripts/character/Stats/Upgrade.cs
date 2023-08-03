using System.Collections.Generic;

[System.Serializable]
public class Upgrade
{
    public string upgradeName;
    public StatModification[] statsAffected;
    public Dictionary<(string, string), StatModification> statsAffectedDict = new();
    public int upgradeLevel = 1;


    public void InitializeDictionary()
    {
        foreach (StatModification statModification in statsAffected)
        {
            statsAffectedDict.Add((statModification.statAffected, statModification.modificationType), statModification);
        }
    }
}
