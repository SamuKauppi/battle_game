using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiController : PlayerController
{
    UnitsInLane[] lanes = null;
    private int desiredLaneIndex = 4;
    [SerializeField] private int desiredUnitIndex = 0;
    private readonly int[] indexes = new int[4];

    private float shortestDistance = 0;
    private float biggestDifference = 0;
    private float leastAllies = 0;

    [SerializeField] private float emergencyDist = 3f;
    private bool emergencyMode;

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
    }

    public override void OnUnitSpawn()
    {
        FindNewPosition();
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
                if (unit.Alliance == alliance)
                {
                    allies++;
                    continue;
                }
                enemies++;
                float distance = Mathf.Abs((unit.transform.position - transform.position).z);
                if (distance < shortestDistance || shortestDistance == 0)
                {
                    indexes[0] = i;
                    shortestDistance = distance;
                }
            }

            if (shortestDistance < emergencyDist && enemies - allies > 0)
            {
                desiredLaneIndex = i;
                SelectNewUnit();
                emergencyMode = true;
                return;
            }

            if (biggestDifference < enemies - allies)
            {
                biggestDifference = enemies - allies;
                indexes[1] = i;
            }
            if (allies < leastAllies || leastAllies == 0f)
            {
                leastAllies = allies;
                indexes[2] = i;
            }
        }

        indexes[3] = Random.Range(0, lanes.Length);
        desiredLaneIndex = indexes[Random.Range(0, indexes.Length)];
        emergencyMode = false;
        SelectNewUnit();
    }

    private void SelectNewUnit()
    {
        int spear = 0;
        int sword = 0;

        foreach (UnitController u in lanes[desiredLaneIndex].units)
        {
            if (u.UnitName.Equals("spear"))
                spear++;
            else
                sword++;
        }
        if (spear > sword)
        {
            desiredUnitIndex = 1;
        }
        else if (spear < sword)
        {
            desiredUnitIndex = 2;
        }
        else
        {
            desiredUnitIndex = 0;
        }

        if (emergencyMode)
        {
            if(desiredUnitIndex == 0)
            {
                desiredUnitIndex = 1;
            }
            else
            {
                desiredUnitIndex = 0;
            }
        }
    }
}
