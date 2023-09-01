using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanController : PlayerController
{
    public override void OnStart()
    {
        if (PlayerIndex % 2 == 0)
        {
            playerType = PlayerType.p1;
        }
        else
        {
            playerType = PlayerType.p2;
        }
    }
    public enum PlayerType
    {
        p1,
        p2,
        main
    }

    public PlayerType playerType;

    private void Update()
    {
        if (playerType == PlayerType.p1)
        {
            HandleP1Input();
        }
        else if (playerType == PlayerType.p2)
        {
            HandleP2Input();
        }
        else if (playerType == PlayerType.main)
        {
            HandleMainInput();
        }

        CheckPosInput();
        CheckUnitInput();
        CheckSpawn();
        CheckAbilities();
    }

    private void HandleP1Input()
    {
        HandlePositionInput(KeyCode.W, KeyCode.S);
        HandleUnitInput(KeyCode.A, KeyCode.D);
        CheckAbilityInput(KeyCode.Alpha1, 0);
        CheckAbilityInput(KeyCode.Alpha2, 1);
        CheckAbilityInput(KeyCode.Alpha3, 2);
        CheckAbilityInput(KeyCode.Alpha4, 3);
        CheckAbilityInput(KeyCode.Alpha5, 4);
    }

    private void HandleP2Input()
    {
        HandlePositionInput(KeyCode.UpArrow, KeyCode.DownArrow);
        HandleUnitInput(KeyCode.LeftArrow, KeyCode.RightArrow);
        CheckAbilityInput(KeyCode.Keypad1, 0);
        CheckAbilityInput(KeyCode.Keypad2, 1);
        CheckAbilityInput(KeyCode.Keypad3, 2);
        CheckAbilityInput(KeyCode.Keypad4, 3);
        CheckAbilityInput(KeyCode.Keypad5, 4);
    }

    private void HandleMainInput()
    {
        HandleP1Input();
        HandleP2Input();
    }

    private void HandlePositionInput(KeyCode positiveKey, KeyCode negativeKey)
    {
        if (Input.GetKey(positiveKey))
        {
            PosInput = -1;
        }
        else if (Input.GetKey(negativeKey))
        {
            PosInput = 1;
        }
        else
        {
            PosInput = 0;
        }
    }

    private void HandleUnitInput(KeyCode leftKey, KeyCode rightKey)
    {
        if (Input.GetKeyDown(leftKey))
        {
            UnitInput = -1;
        }
        else if (Input.GetKeyDown(rightKey))
        {
            UnitInput = 1;
        }
    }
    private void CheckAbilityInput(KeyCode key, int abilityIndex)
    {
        if (Input.GetKeyDown(key))
        {
            ActivateAbility(abilityIndex);
        }
    }
}
