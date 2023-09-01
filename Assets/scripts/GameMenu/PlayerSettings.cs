using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSettings : MonoBehaviour
{
    // Slot index
    public int playerSlotIndex;

    // For storing player settings in scene
    public int Alliance;
    public Toggle isHumanPlayer;
    public TMP_Dropdown logo;
    public Image[] teamColors;
    public TMP_Dropdown[] units;
    public List<UpgradeTransfer> Upgrades { get; private set; }     // For storing Upgrades
    public List<string> Abilities { get; private set; }             // For stroing abilities

    // For loading player settings at Start
    private PersistentManager manager;
    [SerializeField] private AddUnit unitAdder;

    private void Start()
    {
        manager = PersistentManager.Instance;

        PlayerDataTransferClass playerData = null;
        bool foundExistingSlot = false;

        foreach (PlayerDataTransferClass humanPlayer in manager.gameProperties.humanPlayers)
        {
            if (humanPlayer.slotIndex == playerSlotIndex)
            {
                foundExistingSlot = true;
                isHumanPlayer.isOn = true;
                playerData = humanPlayer;
                break;
            }
        }

        if (!foundExistingSlot)
        {
            foreach (PlayerDataTransferClass aiPlayer in manager.gameProperties.aiPlayers)
            {
                if (aiPlayer.slotIndex == playerSlotIndex)
                {
                    foundExistingSlot = true;
                    isHumanPlayer.isOn = false;
                    playerData = aiPlayer;
                    break;
                }
            }
        }

        if (!foundExistingSlot || playerData == null)
            return;

        logo.value = playerData.logoIndex;

        teamColors[0].color = new Color(playerData.mainColor.r, playerData.mainColor.g, playerData.mainColor.b, 1f);
        teamColors[1].color = new Color(playerData.detailColor.r, playerData.detailColor.g, playerData.detailColor.b, 1f);
        teamColors[2].color = new Color(playerData.highlightColor.r, playerData.highlightColor.g, playerData.highlightColor.b, 1f);

        Upgrades = playerData.upgrades.ToList();
        Abilities = playerData.ablilites.ToList();

        unitAdder.AddUnits(playerData.units);
    }

    public void ToggleItem(string optionData, bool isOn, string tag, int level = 0)
    {
        switch (optionData)
        {
            case "upgrade":
                foreach (UpgradeTransfer upgrade in Upgrades)
                {
                    if (upgrade.upgradeTag.Equals(tag))
                    {
                        if (!isOn)
                        {
                            Upgrades.Remove(upgrade);
                        }
                        else
                        {
                            upgrade.upgradeLevel = level;
                        }
                        return;
                    }
                }
                if (isOn)
                    Upgrades.Add(new(tag, level));
                break;

            case "ability":
                if (Abilities.Contains(tag))
                {
                    if (!isOn)
                    {
                        Abilities.Remove(tag);
                    }
                }
                else if (isOn)
                    Abilities.Add(tag);

                break;

            default:
                break;
        }
    }
}
