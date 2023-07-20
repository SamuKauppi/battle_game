[System.Serializable]
public class Upgrade
{
    public Upgrade(string name, float flat = 0f, float multiplier = 1f)
    {
        upgradeName = name;
        upgradeFlat = flat;
        upgradeMultiplier = multiplier;
    }

    public string upgradeName;
    public float upgradeFlat;
    public float upgradeMultiplier;
    public int level = 1;
}
