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

        CheckPosInput();
        CheckUnitInput();
        CheckSpawn();
    }
}
