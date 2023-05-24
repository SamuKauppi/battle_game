using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerController : MonoBehaviour
{
    public Transform selector;
    public int selectedPos;
    [SerializeField] private string selectedUnit;
    public int alliance;
    [SerializeField] private float spawnTime = 2f;
    private float spawnTimer;
    private float moveTimer;
    public GameController manager;
    public int PosInput { get; set; }
    public int UnitInput { get; set; }

    private void Start()
    {
        manager = GameController.Instance;
        PosInput = 0;
    }

    public void CheckPosInput()
    {
        moveTimer += Time.deltaTime;
        if (moveTimer < 0.15f)
            return;

        if (PosInput < 0)
        {
            moveTimer = 0f;
            selectedPos -= 1;
            selectedPos = Mathf.Clamp(selectedPos, 0, 8);    
            PosInput = 0;
        }
        if (PosInput > 0)
        {
            moveTimer = 0f;
            selectedPos += 1;
            selectedPos = Mathf.Clamp(selectedPos, 0, 8);
            PosInput = 0;
        }
        selector.position = new Vector3(manager.GetXPos(selectedPos).x, 0, selector.transform.position.z);
    }

    public void CheckSpawn()
    {
        if (spawnTimer > spawnTime)
        {
            manager.AddUnit(alliance, selectedPos, selector.position, transform.rotation, selectedUnit);
            spawnTimer = 0f;
        }
        spawnTimer += Time.deltaTime;
    }

}
