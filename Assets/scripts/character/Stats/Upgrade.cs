[System.Serializable]
public class Upgrade
{
    public Upgrade(string name, float flat = 0f, float multiplier = 1f, int level = 1)
    {
        upgradeName = name;
        upgradeFlat = flat * level;
        upgradeMultiplier = multiplier * level;
        upgradeLevel = level;
    }

    public string upgradeName;
    public float upgradeFlat;
    public float upgradeMultiplier;
    public int upgradeLevel = 1;
}
