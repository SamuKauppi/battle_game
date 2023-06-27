using TMPro;
using UnityEngine;

public abstract class PlayerController : MonoBehaviour
{
    public Transform selector;
    public int selectedPos;

    int lanesLength;
    [SerializeField] private string[] unitNames;
    [SerializeField] private UnitSpawn[] units;
    public int selectedUnit;

    public int alliance;
    private float spawnTimer;
    private float moveTimer;

    public int PosInput { get; set; }
    public int UnitInput { get; set; }

    public GameController gameManager;
    public SpawnHudController spawnManager;
    [SerializeField] private int playerIndex;
    
    private bool toggleSpawn = false;

    private void Start()
    {
        gameManager = GameController.Instance;
        spawnManager = SpawnHudController.instance;
        units = spawnManager.GetUnits(unitNames, playerIndex);
        PosInput = 0;
        lanesLength = gameManager.GetLaneData().Length - 1;
    }

    public void CheckPosInput()
    {
        moveTimer += Time.deltaTime;
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (moveTimer < 0.15f)
            return;

        if (PosInput < 0)
        {
            moveTimer = 0f;
            selectedPos -= 1;
            selectedPos = Mathf.Clamp(selectedPos, 0, lanesLength);
            OnInput(PosInput);
            PosInput = 0;
        }
        if (PosInput > 0)
        {
            moveTimer = 0f;
            selectedPos += 1;
            selectedPos = Mathf.Clamp(selectedPos, 0, lanesLength);
            OnInput(PosInput);
            PosInput = 0;
        }

        selector.position = new Vector3(gameManager.GetXPos(selectedPos).x, 0, selector.transform.position.z);
    }

    public void CheckUnitInput()
    {
        if (UnitInput < 0)
        {
            selectedUnit -= 1;
            if (selectedUnit < 0)
            {
                selectedUnit = units.Length - 1;
            }
            UnitInput = 0;
        }
        if (UnitInput > 0)
        {
            selectedUnit += 1;
            if (selectedUnit >= units.Length)
            {
                selectedUnit = 0;
            }
            UnitInput = 0;
        }

        spawnManager.MoveSelector(playerIndex, units[selectedUnit].rectPosition);
    }

    public void CheckSpawn()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            toggleSpawn = !toggleSpawn;
        }

        if (toggleSpawn)
            return;

        if (spawnTimer > units[selectedUnit].spawnTime)
        {
            gameManager.AddUnit(alliance, selectedPos, selector.position, transform.rotation, units[selectedUnit].unitName);
            spawnTimer = 0f;
            OnUnitSpawn();
        }
        spawnTimer += Time.deltaTime;

        for (int i = 0; i < units.Length; i++)
        {
            units[i].pickSlider.value = spawnTimer;
        }
    }

    public virtual void OnUnitSpawn()
    {

    }
    public virtual void OnInput(int index)
    {

    }

}
