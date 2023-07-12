using System.Collections;
using System.Collections.Generic;
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
        teamColors[0].color = playerData.mainColor;
        teamColors[1].color = playerData.detailColor;
        teamColors[2].color = playerData.highlightColor;

        unitAdder.AddUnits(playerData.units);
    }

}
