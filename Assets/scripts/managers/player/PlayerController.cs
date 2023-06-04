using TMPro;
using UnityEngine;

public abstract class PlayerController : MonoBehaviour
{
    public Transform selector;
    public int selectedPos;

    [SerializeField] private UnitSpawn[] units;
    public int selectedUnit;

    public int alliance;
    private float spawnTimer;
    private float moveTimer;

    public int PosInput { get; set; }
    public int UnitInput { get; set; }

    public GameController manager;
    [SerializeField] private TMP_Text unitText;

    private void Start()
    {
        manager = GameController.Instance;
        PosInput = 0;
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
            selectedPos = Mathf.Clamp(selectedPos, 0, 8);
            OnInput(PosInput);
            PosInput = 0;
        }
        if (PosInput > 0)
        {
            moveTimer = 0f;
            selectedPos += 1;
            selectedPos = Mathf.Clamp(selectedPos, 0, 8);
            OnInput(PosInput);
            PosInput = 0;
        }

        selector.position = new Vector3(manager.GetXPos(selectedPos).x, 0, selector.transform.position.z);
    }
    public virtual void OnInput(int index)
    {

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
            unitText.text = selectedUnit.ToString();
            UnitInput = 0;
        }
        if (UnitInput > 0)
        {
            selectedUnit += 1;
            if (selectedUnit >= units.Length)
            {
                selectedUnit = 0;
            }
            unitText.text = selectedUnit.ToString();
            UnitInput = 0;
        }

    }

    public void CheckSpawn()
    {
        if (spawnTimer > units[selectedUnit].spawnTime)
        {
            manager.AddUnit(alliance, selectedPos, selector.position, transform.rotation, units[selectedUnit].unitName);
            spawnTimer = 0f;
            OnUnitSpawn();
        }
        spawnTimer += Time.deltaTime;
    }

    public virtual void OnUnitSpawn()
    {

    }
}
