using System.Linq;
using UnityEngine;

/// <summary>
/// Controls the AI player
/// </summary>
public class AiController : PlayerController
{
    // Lane-related variables
    private UnitsInLane[] lanes = null;                 // Data of lanes

    private int desiredLaneIndex = 4;                   // Where the AI wants to spawn the next unit
    private float shortestDistance = 0;                 // Lane index with the shortest distance between enemy and reaching the end
    private float biggestDifference = 0;                // Lane index with the biggest difference between allies and enemies count
    private float leastAllies = 0;                      // Lane index with the least amount of allies

    [SerializeField] private float emergencyDist = 3f;  // Distance that determines if it's an emergency
    private bool emergencyMode;                         // Is the emergency mode on

    private readonly int[] laneSelectIndexes = new int[4];  // 4 options for which lane to use. Selected randomly:
                                                            // 0 = Lane that has the unit closest to AI. Picked always if it's an emergency
                                                            // 1 = Biggest difference between allies and enemies
                                                            // 2 = Has the least allies
                                                            // 3 = Random

    // Unit-related variables
    private int desiredUnitIndex = 0;                   // What Unit the AI wants to spawn next

    [SerializeField] private bool isComboing;                            // Is the AI currently performing a combo
    private int comboIndex;                             // Which combo is the AI performing
    private int comboCount;                             // How far is the AI in the combo

    public StringArray[] Combos { get; set; }           // All of the combos available for AI
    public StringArray[] UnitData { get; set; }         // Data about which unit counters which (copied from GameManager)
    private string closestUnit;                         // Closest unit's name (used to find a counter for it)

    // Ability related
    private bool unitWasSpawned;

    public override void OnStart()
    {
        FindNewPosition();
    }

    private void Update()
    {

        if (selectedPos < desiredLaneIndex)
        {
            PosInput = 1;
        }
        else if (selectedPos > desiredLaneIndex)
        {
            PosInput = -1;
        }
        else
        {
            PosInput = 0;
        }

        if (selectedUnit < desiredUnitIndex)
        {
            UnitInput = 1;
        }
        else if (selectedUnit > desiredUnitIndex)
        {
            UnitInput = -1;
        }
        else
        {
            UnitInput = 0;
        }

        CheckPosInput();
        CheckUnitInput();
        CheckSpawn();
        CheckAbilities();
        AiActivateAbility();
    }

    public override void OnUnitSpawn()
    {
        FindNewPosition();
        unitWasSpawned = true;
    }
    private void FindNewPosition()
    {
        shortestDistance = 0f;
        biggestDifference = 0f;
        shortestDistance = 0f;
        leastAllies = 0f;
        lanes = gameManager.GetLaneData();

        for (int i = 0; i < lanes.Length; i++)
        {
            int allies = 0;
            int enemies = 0;
            foreach (UnitController unit in lanes[i].units)
            {
                if (unit.Alliance == Alliance)
                {
                    allies++;
                    continue;
                }
                enemies++;
                float distance = Mathf.Abs((unit.transform.position - transform.position).z);
                if ((distance < shortestDistance || shortestDistance == 0))
                {
                    laneSelectIndexes[0] = i;
                    shortestDistance = distance;
                    closestUnit = unit.UnitName;
                }
            }

            if (shortestDistance < emergencyDist && enemies - allies > 0)
            {
                desiredLaneIndex = i;
                SelectNewUnit();
                emergencyMode = true;
                return;
            }

            if (biggestDifference < enemies - allies && lanes[i].units.Count < 50)
            {
                biggestDifference = enemies - allies;
                laneSelectIndexes[1] = i;
            }
            if (allies < leastAllies || leastAllies == 0f)
            {
                leastAllies = allies;
                laneSelectIndexes[2] = i;
            }
        }

        if (!isComboing)
        {
            laneSelectIndexes[3] = Random.Range(0, lanes.Length);
            desiredLaneIndex = laneSelectIndexes[Random.Range(0, laneSelectIndexes.Length)];
        }
        emergencyMode = false;
        SelectNewUnit();
    }

    private void SelectNewUnit()
    {
        int funtionality = Random.Range(0, 5);

        string nextUnit;
        if (emergencyMode)
        {
            nextUnit = GetCounterUnit(closestUnit);
            isComboing = false;
        }
        else if (isComboing)
        {
            nextUnit = Combo();
        }
        else
        {
            nextUnit = funtionality switch
            {
                0 => GetCounterUnit(closestUnit),
                _ => Combo(),
            };
        }


        for (int i = 0; i < units.Length; i++)
        {
            if (units[i].unitName == nextUnit)
            {
                desiredUnitIndex = i;
                break;
            }
        }
    }

    private string GetCounterUnit(string unitAgainst)
    {
        for (int i = 0; i < UnitData.Length; i++)
        {
            if (UnitData[i].array[0].Equals(unitAgainst))
            {
                for (int y = 1; y < UnitData[i].array.Length; y++)
                {
                    for (int x = 0; x < units.Length; x++)
                    {
                        if (UnitData[i].array[y].Equals(units[x].unitName))
                        {
                            return UnitData[i].array[y];
                        }
                    }

                }
            }
        }
        return "";
    }

    private string Combo()
    {
        if (!isComboing)
        {
            comboIndex = Random.Range(0, Combos.Length);
            comboCount = 0;
        }
        isComboing = true;


        string nextUnit = Combos[comboIndex].array[comboCount];
        comboCount++;
        if (comboCount >= Combos[comboIndex].array.Length)
        {
            isComboing = false;
        }

        return nextUnit;
    }

    private void AiActivateAbility()
    {
        for (int i = 0; i < Abilities.Length; i++)
        {
            Ability ability = Abilities[i];

            if (!ability.IsTheAbilityReady())
            {
                continue;
            }
            
            switch (ability.abilityTag)
            {
                case "double":
                    if (AnUnitWasJustSpawned())
                    {
                        // Activate the "double" ability
                        ActivateAbility(i);
                    }
                    break;

                case "convert":
                    if (ThereIsAnEnemyUnitInSelectedLane())
                    {
                        // Activate the "convert" ability
                        ActivateAbility(i);
                    }
                    break;

                case "column":
                case "production":
                    // Activate the "column" and "production" abilities since they don't require additional conditions
                    ActivateAbility(i);
                    break;

                // Add more cases for other abilities as needed

                default:
                    break;
            }
        }
        unitWasSpawned = false;
    }

    private bool AnUnitWasJustSpawned()
    {
        return unitWasSpawned;
    }

    private bool ThereIsAnEnemyUnitInSelectedLane()
    {
        return lanes[desiredLaneIndex].units.Any(unit => unit.Alliance != Alliance);
    }
}
