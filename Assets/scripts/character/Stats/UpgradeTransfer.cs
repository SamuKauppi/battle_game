using UnityEngine;

[System.Serializable]
public class UpgradeTransfer
{
    public string upgradeName;
    [Range(1f, 10f)]
    public int upgradeLevel = 1;
}
