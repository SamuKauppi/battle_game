using System.Collections;
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
    private Upgrade[] Upgrades { get; set; }
    private Ability[] Abilities { get; set; }
    private string[] UnitNames { get; set; }    // Units player has access to
    public int Alliance { get; set; }           // Alliance tells units who to attack
    private int PlayerIndex { get; set; }       // Player indentifier (needs to be unique for the scene)
    private int LogoIndex { get; set; }         // Logo index 
    private Color BaseColor { get; set; }       // Base color of the team
    private Color DetailColor { get; set; }     // Detail color of the team
    private Color HighlightColor { get; set; }  // Highlight color of the team

    // Ability Related
    private bool IsFasterProduction { get; set; } = false;
    public bool RandomUpgrades { get; private set; }

    public void SetPlayerProperties(int alliance, int playerindex, int logoIndex,
        Color baseC, Color detailC, Color highC, string[] units, MaterialManager matManager,
        Upgrade[] upgrades, Ability[] abilities)
    {
        Alliance = alliance;
        PlayerIndex = playerindex;
        LogoIndex = logoIndex;
        BaseColor = baseC;
        DetailColor = detailC;
        HighlightColor = highC;
        UnitNames = units;
        MatManager = matManager;
        Upgrades = upgrades;
        Abilities = abilities;
        Debug.Log("spawn");
    }

    private void Start()
    {
        gameManager = GameController.Instance;
        spawnManager = SpawnHudController.instance;

        roboMaterial = MatManager.CreateTeamMaterial(BaseColor, DetailColor, HighlightColor, LogoIndex);
        spawnManager.SetPlayerLogo(PlayerIndex, LogoIndex);
        units = spawnManager.GetUnitSlots(UnitNames, PlayerIndex);
        Abilities = spawnManager.SetAbilitySliders(PlayerIndex, Abilities);

        PosInput = 0;
        lanesLength = gameManager.GetLaneData().Length - 1;

        for (int i = 0; i < Abilities.Length; i++)
        {
            if (Abilities[i].ablityTag.Equals("upgrade"))
            {
                RandomUpgrades = true;
                break;
            }
        }
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
            gameManager.SpawnUnit(Alliance, selectedPos, selector.position, transform.rotation, 
                units[selectedUnit].unitName, roboMaterial, Upgrades, RandomUpgrades);
            spawnTimer = 0f;
            OnUnitSpawn();
        }
        spawnTimer += Time.deltaTime;
        if (IsFasterProduction)
        {
            spawnTimer += Time.deltaTime;
        }

        for (int i = 0; i < units.Length; i++)
        {
            units[i].pickSlider.value = spawnTimer;
        }
    }

    public void CheckAbilities()
    {
        foreach (Ability ability in Abilities)
        {
            ability.CooldownTimer += Time.deltaTime;
            ability.AbilitySlider.value = ability.CooldownTimer;
        }
    }

    public virtual void OnUnitSpawn()
    {

    }
    public virtual void OnInput(int index)
    {

    }

    public void ActivateAbility(int abilityIndex)
    {
        foreach (Ability ability in Abilities)
        {
            if (Abilities.Length <= abilityIndex)
            {
                Debug.Log("dont have ability");
                return;
            }

            if (!ability.ablityTag.Equals(Abilities[abilityIndex].ablityTag) || !ability.IsTheAbilityReady())
            {
                continue;
            }

            Debug.Log("Activate ability = " + Abilities[abilityIndex].abilityName);
            ability.CooldownTimer = 0f;

            switch (ability.ablityTag)
            {
                case "double":
                    spawnTimer += 10000;
                    return;
                case "convert":
                    UnitsInLane[] laneData = gameManager.GetLaneData();
                    UnitController closestUnit = null;
                    float distance = 0f;
                    foreach (UnitController unit in laneData[selectedPos].units)
                    {
                        if (unit.Alliance == Alliance)
                            continue;

                        if (distance > Mathf.Abs(selector.transform.position.z - unit.transform.position.z) || closestUnit == null)
                        {
                            closestUnit = unit;
                            distance = Mathf.Abs(selector.transform.position.z - closestUnit.transform.position.z);
                        }
                    }
                    if (closestUnit != null)
                        closestUnit.ConvertUnit();
                    return;
                case "column":
                    StartCoroutine(ColumnSpawn());
                    return;
                case "production":
                    StartCoroutine(FasterProduction(ability.duration));
                    return;
                case "upgrade":
                    RandomUpgrades = true;
                    return;
                default:
                    Debug.Log(ability.ablityTag + " does not exist");
                    return;
            }
        }
    }

    private IEnumerator FasterProduction(float time)
    {
        IsFasterProduction = true;
        yield return new WaitForSecondsRealtime(time);
        IsFasterProduction = false;
    }

    private IEnumerator ColumnSpawn()
    { 
        while(spawnTimer < units[selectedUnit].spawnTime)
        {
            yield return null;
        }

        spawnTimer = 0f;
        int numberOfLanes = gameManager.GetLaneData().Length;
        for (int i = 0; i < numberOfLanes; i++)
        {
            Vector3 xPos = gameManager.GetXPos(i);
            gameManager.SpawnUnit(Alliance, i, new Vector3(xPos.x, xPos.y, selector.transform.position.z),
                transform.rotation, units[selectedUnit].unitName, roboMaterial, Upgrades);
        }
    }

}
