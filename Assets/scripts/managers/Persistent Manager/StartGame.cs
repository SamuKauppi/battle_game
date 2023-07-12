using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    private GameTransferClass gameData;
    // Maps
    [SerializeField] private GameObject[] gameMaps;
    [SerializeField] private TMP_Dropdown mapsSelector;
    // Time
    private int roundTime;
    [SerializeField] private int minTime;
    [SerializeField] private int maxTime;
    [SerializeField] private TMP_Text timeText;
    // Players
    [SerializeField] private PlayerSettings[] players;

    private void Start()
    {
        gameData = PersistentManager.Instance.gameProperties;
        roundTime = 210;
    }
    public void LaunchGame()
    {
        gameData.map = gameMaps[mapsSelector.value];
        gameData.roundTime = roundTime;
        gameData.aiPlayers = new List<AiTransferData>();
        gameData.humanPlayers = new List<HumanPlayerTransfer>();

        foreach (var player in players)
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
                    units = units.ToArray()
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

                int numberOfCombos = units.Count * 2;
                StringArray[] combos = new StringArray[numberOfCombos];

                for (int x = 0; x < combos.Length; x++)
                {
                    int numberOfUnitsInCombo = Random.Range(2, 6);
                    int maxUnitIndex = Mathf.RoundToInt(numberOfUnitsInCombo / 1.1f);
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
                        unitIndex = Mathf.Clamp(unitIndex, 0, reOrderedUnits.Count - 1);
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



                playerData = new AiTransferData
                {
                    Alliance = player.Alliance,
                    slotIndex = player.playerSlotIndex,
                    logoIndex = player.logo.value,
                    mainColor = player.teamColors[0].color,
                    detailColor = player.teamColors[1].color,
                    highlightColor = player.teamColors[2].color,
                    units = units.ToArray(),
                    aiPlayerCombos = combos
                };

                gameData.aiPlayers.Add((AiTransferData)playerData);
            }
        }

        PersistentManager.Instance.loader.LoadScene(1);
    }

    public void AlterTime(int value)
    {
        roundTime = Mathf.Clamp(roundTime + value, minTime, maxTime);
        timeText.text = roundTime + " s";
    }
}
