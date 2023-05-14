using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanController : PlayerController
{
    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            PosInput = -1;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            PosInput = 1;
        }
        else
        {
            PosInput = 0;
        }

        CheckPosInput();
        CheckSpawn();
    }
}
