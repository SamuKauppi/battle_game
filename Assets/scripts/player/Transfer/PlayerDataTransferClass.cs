using UnityEngine;
[System.Serializable]
public abstract class PlayerDataTransferClass
{
    public int Alliance;
    public int slotIndex;
    public int logoIndex;
    public Color mainColor;
    public Color detailColor;
    public Color highlightColor;
    public string[] units;
    public UpgradeTransfer[] upgrades;
}
