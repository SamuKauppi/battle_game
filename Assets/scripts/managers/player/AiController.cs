using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiController : PlayerController
{
    UnitsInLane[] lanes = null;
    private float checkTimer;
    [SerializeField] private int desiredIndex;
    private float shortestDistance = 0;

    private void Update()
    {
        checkTimer += Time.deltaTime;

        if (checkTimer > 1f)
        {
            lanes = manager.GetUnits();
            for (int i = 0; i < lanes.Length; i++)
            {
                foreach (UnitController unit in lanes[i].units)
                {
                    if (unit.Alliance == alliance)
                        continue;

                    float distance = Mathf.Abs(transform.position.z - unit.transform.position.z);
                    if (distance < shortestDistance)
                    {
                        desiredIndex = i;
                        shortestDistance = distance;
                    }
                    Debug.Log(distance);
                }
            }
            if (shortestDistance == 0)
            {
                desiredIndex = 4;
            }
            shortestDistance = 0f;
            checkTimer = 0;
        }

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

}
