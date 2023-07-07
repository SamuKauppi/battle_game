using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the game
/// </summary>
public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    // Player prefabs
    [SerializeField] private HumanController humanPlayerPrefab;
    [SerializeField] private AiController aiPlayerPrefab;

    // Player positons
    [SerializeField] private Transform playerParent;
    [SerializeField] private PlayerTransform[] playerTransforms;

    // References
    private ObjectPooler pooler;
    private PersistentManager pManager;         // Reference to a manager that transfers data between scenes

    // Every lane and their data including position and units
    [SerializeField] private UnitsInLane[] lanes;
    // Game score: the one who gets 10 or more points wins
    private int playerScores = 0;
    // Game time: the one who has more points at end of the time wins
    private float timer;

    // UI elements
    [SerializeField] private Slider scoreSlider;    // Score
    [SerializeField] private Slider clockSlider;    // Clock


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        pManager = PersistentManager.Instance;

        StartGame();
    }

    private void StartGame()
    {
        // Spawn map
        Instantiate(pManager.gameProperties.map, Vector3.zero, Quaternion.identity, transform);

        // Initilize round timer
        timer = pManager.gameProperties.roundTime;
        clockSlider.maxValue = timer;
        clockSlider.value = timer;

        // Initlize Players
        foreach (HumanPlayerTransfer playerData in pManager.gameProperties.humanPlayers)
        {
            SpawnPlayer(playerData, false);
        }
        foreach (AiTransferData playerData in pManager.gameProperties.aiPlayers)
        {
            SpawnPlayer(playerData, true);
        }
    }

    /// <summary>
    /// Spawns an player
    /// </summary>
    /// <param name="player"></param>
    /// <param name="isAi"></param>
    /// <returns></returns>
    private PlayerController SpawnPlayer(PlayerDataTransferClass player, bool isAi)
    {
        PlayerController p;
        if (!isAi)
        {
            p = Instantiate(humanPlayerPrefab);
        }
        else
        {
            AiController aiPlayer = Instantiate(aiPlayerPrefab);
            AiTransferData aiTransfer = (AiTransferData)player;
            aiPlayer.Combos = aiTransfer.aiPlayerCombos;
            aiPlayer.UnitData = pManager.aiUnitCountering;
            p = aiPlayer;
        }

        p.SetPlayerProperties(player.Alliance, 0, player.logoIndex, player.BaseColor, player.detailColor, player.highlightColor, player.units);

        p.transform.parent = playerParent;

        for (int i = 0; i < playerTransforms.Length; i++)
        {
            if (playerTransforms[i].Alliance == p.Alliance)
            {
                p.transform.position = playerTransforms[i].Pos;
                p.transform.rotation = playerTransforms[i].Rot;
                break;
            }
        }
        return p;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Debug.Log("That is game");
        }
        else
        {
            clockSlider.value = timer;
        }
    }

    /// <summary>
    /// Create an unit
    /// </summary>
    /// <param name="alliance"> Unit alliance </param>
    /// <param name="xIndex"> Lane index to be spawned </param>
    /// <param name="pos"> Postiton of the unit </param>
    /// <param name="rot"> Rotation of the unit </param>
    /// <param name="unitType"> Unit type </param>
    /// <param name="mat"> Material of the team </param>
    public void AddUnit(int alliance, int xIndex, Vector3 pos, Quaternion rot, string unitType, Material mat)
    {
        if (!pooler)
            pooler = ObjectPooler.Instance;

        UnitController unit = pooler.GetPooledObject(unitType).GetComponent<UnitController>();
        unit.myRend.material = mat;
        unit.transform.SetPositionAndRotation(pos, rot);
        unit.Xindex = xIndex;
        unit.Zindex = lanes[xIndex].indexCounter;
        lanes[xIndex].indexCounter += 1;
        unit.Alliance = alliance;
        unit.IsAlive = true;
        lanes[xIndex].units.Add(unit);
    }

    /// <summary>
    /// Returns all valid targets in range
    /// </summary>
    /// <param name="thisPos"> Unit position </param>
    /// <param name="attackRange"> How far can this unit attack </param>
    /// <param name="stopRange"> How close until this unit should stop </param>
    /// <param name="alliance"> This units alliance </param>
    /// <param name="Xindex"> Lane index </param>
    /// <param name="Zindex"> Index inside the lane (unique for unit in lane) </param>
    /// <param name="maxTargets"> How many targets can unit find (atleast one enemy and one ally) </param>
    /// <returns> Array of targets </returns>
    public TargetInRange[] CheckEnemy(Vector3 thisPos, Vector3 attackRange, Vector3 stopRange, int alliance, int Xindex, int Zindex, int maxTargets)
    {
        // Create an array
        TargetInRange[] targets = new TargetInRange[maxTargets + 1];
        bool allyFound = false;
        int enemyIndex = 0;

        for (int i = lanes[Xindex].units.Count - 1; i >= 0; i--)
        {
            UnitController target = lanes[Xindex].units[i];
            if (!target.IsAlive)
            {
                lanes[Xindex].units.RemoveAt(i);
                continue; // target is dead and will be removed
            }

            if (Mathf.Abs(target.transform.position.z) > 12f)
            {
                target.TakeDamage(1000000, "");
                lanes[Xindex].units.RemoveAt(i);
                playerScores += target.Alliance;
                scoreSlider.value = playerScores;
                if (Mathf.Abs(playerScores) >= 10)
                    Debug.Log("Allicance: " + target.Alliance + " won!");
                continue; // target reached the end
            }

            if (target.Zindex == Zindex)
                continue; // target is the same as this

            Vector3 relativePos = target.transform.position - thisPos;
            Vector3 forward = (attackRange - thisPos).normalized;
            float dot = Vector3.Dot(relativePos.normalized, forward);
            if (dot < 0)
                continue; // target unit is behind
            else if (dot == 0)
            {
                target.transform.position += forward * 0.2f;
                continue; // target is on top of this and will be moved forward a bit
            }

            // Relevant distances
            float distanceToTarget = Vector3.Distance(thisPos, target.transform.position);  // Distance to target
            bool inAttackRange = distanceToTarget < Vector3.Distance(thisPos, attackRange); // Is the target in attack range
            bool inStopRange = distanceToTarget < Vector3.Distance(thisPos, stopRange);     // Is the target in stop range

            if (alliance != target.Alliance)
            {
                if (!inAttackRange)
                    continue; // enemy is too far from attacking range

                // If there are too many enemies found then take only the closests
                if (enemyIndex >= maxTargets)
                {
                    // Iterate enemies found
                    int shortestDistIdex = 0;
                    float dist = distanceToTarget;
                    for (int x = 1; x < targets.Length; x++)
                    {
                        if (targets[x].DistanceToTarget > dist)
                        {
                            shortestDistIdex = x;
                            dist = targets[x].DistanceToTarget;
                        }
                    }
                    // If an enemy closer was found, replace it
                    if (shortestDistIdex != 0)
                    {
                        targets[shortestDistIdex] = new TargetInRange(target, target.Alliance, inStopRange, distanceToTarget);
                    }
                }
                else
                {
                    // Add enemy to array
                    enemyIndex++;
                    targets[enemyIndex] = new TargetInRange(target, target.Alliance, inStopRange, distanceToTarget);
                }
            }
            // Only one ally can be found
            else
            {
                if (!inStopRange || allyFound)
                    continue; // ally is too far from stopping range or an ally has already been found
                // Add ally
                targets[0] = new TargetInRange(target, alliance, inStopRange, distanceToTarget);
                allyFound = true;
            }
        }
        return targets;
    }

    // Return lane position
    public Vector3 GetXPos(int index)
    {
        return lanes[index].xPos.position;
    }

    // Return every lane
    public UnitsInLane[] GetLaneData()
    {
        return lanes;
    }

}
