using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    private PersistentManager manager;
    public static UpgradeManager Instance { get; private set; }

    private void Start()
    {
        manager = PersistentManager.Instance;
        Instance = this;

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
            Upgrade upgrade = FindMatchingUpgrade(transferInfo.upgradeName);
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
            if (upgrade.upgradeName.Equals(upgradeName))
            {
                return upgrade;
            }
        }
        return null;
    }
}
