using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    private void Start()
    {
        PersistentManager manager = PersistentManager.Instance;
        foreach (Upgrade upgrade in manager.availabeUpgrades)
        {
            upgrade.InitilizeDict();
        }
    }
}
