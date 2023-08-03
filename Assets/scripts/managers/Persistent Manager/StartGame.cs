using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    private GameTransferClass gameData;
    // Maps
    [SerializeField] private GameObject[] gameMapPrefabs;
    private int mapSelectorIndex;
    // Time
    private int roundTime = 210;
    [SerializeField] private int minTime;
    [SerializeField] private int maxTime;
    [SerializeField] private TMP_Text timeText;
    // Players
    [SerializeField] private PlayerSettings[] players;
    [SerializeField] private AddPlayerSlot alliance1PlayerAdder;
    [SerializeField] private AddPlayerSlot alliance2PlayerAdder;


    /// <summary>
    /// Load data from PersistentManager at Start and activate player slots for them
    /// </summary>
    private void Start()
    {
        gameData = PersistentManager.Instance.gameProperties;
        bool alliance1CountEdited = false;
        bool alliance2CountEdited = false;
        foreach (PlayerSettings player in players)
        {
            int playerSlotIndex = player.playerSlotIndex;
            foreach (PlayerDataTransferClass transferData in gameData.humanPlayers.Concat<PlayerDataTransferClass>(gameData.aiPlayers))
            {
                if (playerSlotIndex == transferData.slotIndex)
                {
                    // Slots jump from left to right
                    if (playerSlotIndex % 2 != 0)
                    {
                        if (alliance1CountEdited)
                        {
                            alliance1PlayerAdder.EditPlayerCount(1);
                        }
                        alliance1CountEdited = true;
                    }
                    else
                    {
                        if (alliance2CountEdited)
                        {
                            alliance2PlayerAdder.EditPlayerCount(1);
                        }
                        alliance2CountEdited = true;
                    }

                }
            }
        }
    }
    /// <summary>
    /// Edits the round time
    /// </summary>
    /// <param name="value"></param>
    public void AlterTime(int value)
    {
        roundTime = Mathf.Clamp(roundTime + value, minTime, maxTime);
        if (timeText != null)
            timeText.text = roundTime + " s";
    }
    /// <summary>
    /// Selects the map that will be used
    /// </summary>
    /// <param name="dropdownValue"></param>
    public void SelectMap(TMP_Dropdown dropdownValue)
    {
        mapSelectorIndex = dropdownValue.value;
    }
    /// <summary>
    /// Loads all the data from player settings
    /// </summary>
    public void LaunchGame()
    {
        gameData.map = gameMapPrefabs[mapSelectorIndex];
        gameData.roundTime = roundTime;
        gameData.aiPlayers = new List<AiTransferData>();
        gameData.humanPlayers = new List<HumanPlayerTransfer>();

        foreach (PlayerSettings player in players)
        {
            if (!player.gameObject.activeInHierarchy)
                continue;

            PlayerDataTransferClass playerData;
            List<string> units = new();

            for (int i = 0; i < player.units.Length; i++)
            {
                if (player.units[i].gameObject.activeInHierarchy)
                {
                    units.Add(player.units[i].options[player.units[i].value].text);
                }
            }

            if (player.isHumanPlayer.isOn)
            {
                playerData = new HumanPlayerTransfer
                {
                    Alliance = player.Alliance,
                    slotIndex = player.playerSlotIndex,
                    logoIndex = player.logo.value,
                    mainColor = player.teamColors[0].color,
                    detailColor = player.teamColors[1].color,
                    highlightColor = player.teamColors[2].color,
                    units = units.ToArray(),
                    upgrades = player.Upgrades,
                    ablilites = player.Abilities
                };
                gameData.humanPlayers.Add((HumanPlayerTransfer)playerData);
            }
            else
            {
                List<string> reOrderedUnits = new(units);
                UnitSpawn[] templateUnits = PersistentManager.Instance.avaiableUnits;

                reOrderedUnits = reOrderedUnits.OrderBy(unit =>
                {
                    UnitSpawn template = templateUnits.FirstOrDefault(t => t.unitName == unit);
                    return template?.aiComboSortLayer ?? int.MaxValue;
                }).ToList();

                StringArray[] combos = CreateAiCombos(reOrderedUnits.ToArray(), units.Count);

                playerData = new AiTransferData
                {
                    Alliance = player.Alliance,
                    slotIndex = player.playerSlotIndex,
                    logoIndex = player.logo.value,
                    mainColor = player.teamColors[0].color,
                    detailColor = player.teamColors[1].color,
                    highlightColor = player.teamColors[2].color,
                    units = units.ToArray(),
                    upgrades = player.Upgrades,
                    ablilites = player.Abilities,
                    aiPlayerCombos = combos
                };

                gameData.aiPlayers.Add((AiTransferData)playerData);
            }
        }

        PersistentManager.Instance.loader.LoadScene(1);
    }

    /// <summary>
    /// Creates combos for Ai player
    /// </summary>
    /// <param name="reOrderedUnits"></param>
    /// <param name="units"></param>
    /// <returns></returns>
    private StringArray[] CreateAiCombos(string[] reOrderedUnits, float numberOfUnits) 
    {
        // How many combos will the ai have is based on the number of units * x
        StringArray[] combos = new StringArray[Mathf.RoundToInt(numberOfUnits * 1.5f)];

        for (int x = 0; x < combos.Length; x++)
        {
            int numberOfUnitsInCombo = Random.Range(2, 5);
            int maxUnitIndex = Mathf.RoundToInt(numberOfUnitsInCombo / 1.15f);
            int prevUnitIndex = 0;
            int unitIndex = 0;
            int unitRepeats = 0;
            string[] combo = new string[numberOfUnitsInCombo];
            for (int i = 0; i < combo.Length; i++)
            {
                unitIndex += Random.Range(0, maxUnitIndex);
                if (unitRepeats > 1)
                {
                    unitIndex = Random.Range(0, maxUnitIndex);
                }
                unitIndex = Mathf.Clamp(unitIndex, 0, reOrderedUnits.Length - 1);
                if (prevUnitIndex == unitIndex)
                {
                    unitRepeats++;
                }
                else
                {
                    unitRepeats = 0;
                }
                prevUnitIndex = unitIndex;
                combo[i] = reOrderedUnits[unitIndex];
            }
            combos[x] = new StringArray { array = combo };
        }

        return combos;
    }
}
