using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanController : PlayerController
{
    private void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow))
        {
            PosInput = -1;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            PosInput = 1;
        }
        else
        {
            PosInput = 0;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            UnitInput = -1;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            UnitInput = 1;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ActivateAbility(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ActivateAbility(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ActivateAbility(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ActivateAbility(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ActivateAbility(4);
        }

        CheckPosInput();
        CheckUnitInput();
        CheckSpawn();
        CheckAbilities();
    }
}
