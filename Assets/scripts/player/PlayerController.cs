using UnityEngine;

/// <summary>
/// Controls the player. Abstract class for both AI and human
/// </summary>
public abstract class PlayerController : MonoBehaviour
{
    // References
    public GameController gameManager;
    public SpawnHudController spawnManager;
    public MaterialManager MatManager { get; set; }

    // Lane Selection
    public Transform selector;
    public int selectedPos;
    private int lanesLength;
    public int PosInput { get; set; }

    // Unit Selection
    public UnitSpawn[] units;           // Units in game
    public int selectedUnit;
    public int UnitInput { get; set; }

    // Timers
    private float spawnTimer;
    private float moveTimer;

    // Player qualities (inherited)
    private Material roboMaterial;
    private string[] UnitNames { get; set; }    // Units player has access to
    public int Alliance { get; set; }           // Alliance tells units who to attack
    private int PlayerIndex { get; set; }       // Player indentifier (needs to be unique for the scene)
    private int LogoIndex { get; set; }         // Logo index 
    private Color BaseColor { get; set; }       // Base color of the team
    private Color DetailColor { get; set; }     // Detail color of the team
    private Color HighlightColor { get; set; }  // Highlight color of the team

    public void SetPlayerProperties(int alliance, int playerindex, int logoIndex, Color baseC, Color detailC, Color highC, string[] units, MaterialManager matManager)
    {
        Alliance = alliance;
        PlayerIndex = playerindex;
        LogoIndex = logoIndex;
        BaseColor = baseC;
        DetailColor = detailC;
        HighlightColor = highC;
        UnitNames = units;
        MatManager = matManager;
    }

    private void Start()
    {
        gameManager = GameController.Instance;
        spawnManager = SpawnHudController.instance;

        roboMaterial = MatManager.CreateTeamMaterial(BaseColor, DetailColor, HighlightColor, LogoIndex);
        spawnManager.SetPlayerLogo(PlayerIndex, LogoIndex);
        units = spawnManager.GetUnits(UnitNames, PlayerIndex);
        PosInput = 0;
        lanesLength = gameManager.GetLaneData().Length - 1;
    }

    /// <summary>
    /// Checks the lane selectors position
    /// </summary>
    public void CheckPosInput()
    {
        moveTimer += Time.deltaTime;
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

    /// <summary>
    /// Check unit selection input
    /// </summary>
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

        spawnManager.MoveSelector(PlayerIndex, units[selectedUnit].rectPosition);
    }

    /// <summary>
    /// Check unit spawn
    /// </summary>
    public void CheckSpawn()
    {
        if (spawnTimer > units[selectedUnit].spawnTime)
        {
            gameManager.SpawnUnit(Alliance, selectedPos, selector.position, transform.rotation, units[selectedUnit].unitName, roboMaterial);
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
