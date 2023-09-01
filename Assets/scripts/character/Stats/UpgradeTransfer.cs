using UnityEngine;

[System.Serializable]
public class UpgradeTransfer
{
    public string upgradeTag;
    [Range(1f, 10f)]
    public int upgradeLevel = 1;

    public UpgradeTransfer(string upgradeTag, int level = 1)
    {
        this.upgradeTag = upgradeTag;
        upgradeLevel = level;
    }
}
