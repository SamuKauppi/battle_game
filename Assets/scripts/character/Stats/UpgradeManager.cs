using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    private PersistentManager manager;

    private void Start()
    {
        manager = PersistentManager.Instance;

        foreach (Upgrade upgrade in manager.availableUpgrades)
        {
            upgrade.InitializeDictionary();
        }
    }

    public Upgrade[] GetUpgrades(UpgradeTransfer[] transferInfos)
    {
        HashSet<Upgrade> upgradeSet = new();

        foreach (UpgradeTransfer transferInfo in transferInfos)
        {
            Upgrade upgrade = FindMatchingUpgrade(transferInfo.upgradeTag);
            if (upgrade != null)
            {
                upgrade.upgradeLevel = transferInfo.upgradeLevel;
                upgradeSet.Add(upgrade);
            }
        }

        return upgradeSet.ToArray();
    }

    private Upgrade FindMatchingUpgrade(string upgradeName)
    {
        foreach (Upgrade upgrade in manager.availableUpgrades)
        {
            if (upgrade.upgradeTag.Equals(upgradeName))
            {
                return new Upgrade(upgrade);
            }
        }
        return null;
    }
}
