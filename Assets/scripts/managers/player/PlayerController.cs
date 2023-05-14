using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform selector;
    [SerializeField] private int selectedPos;
    [SerializeField] private string selectedUnit;
    [SerializeField] private int alliance;
    private float spawnTimer;
    private float moveTimer;
    private GameController manager;
    public int PosInput { get; set; }
    public int UnitInput { get; set; }

    private void Start()
    {
        manager = GameController.Instance;
        PosInput = 0;
    }

    public void CheckPosInput()
    {
        if (PosInput < 0 && moveTimer > 0.15f)
        {
            moveTimer = 0f;
            selectedPos -= 1;
            selectedPos = Mathf.Clamp(selectedPos, 0, 8);
            selector.position = new Vector3(manager.GetXPos(selectedPos).x, 0, selector.transform.position.z);
            PosInput = 0;
        }
        if (PosInput > 0 && moveTimer > 0.15f)
        {
            moveTimer = 0f;
            selectedPos += 1;
            selectedPos = Mathf.Clamp(selectedPos, 0, 8);
            selector.position = new Vector3(manager.GetXPos(selectedPos).x, 0, selector.transform.position.z);
            PosInput = 0;
        }
        moveTimer += Time.deltaTime;
    }

    public void CheckSpawn()
    {
        if (spawnTimer > 2)
        {
            manager.AddUnit(alliance, selectedPos, selectedUnit);
            spawnTimer = 0;
        }
        spawnTimer += Time.deltaTime;
    }

}
