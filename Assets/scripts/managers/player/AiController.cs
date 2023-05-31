using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiController : PlayerController
{
    UnitsInLane[] lanes = null;
    private int desiredIndex = 4;
    private readonly int[] indexes = new int[4];

    private float shortestDistance = 0;
    private float biggestDifference = 0;
    private float leastAllies = 0;

    [SerializeField] private float emergencyDist = 3f;
    private void Update()
    {
        if (selectedPos < desiredIndex)
        {
            PosInput = 1;
        }
        else if (selectedPos > desiredIndex)
        {
            PosInput = -1;
        }
        else
        {
            PosInput = 0;
        }

        CheckPosInput();
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
        lanes = manager.GetUnits();

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
                desiredIndex = i;
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
        desiredIndex = indexes[Random.Range(0, indexes.Length)];
    }
}
