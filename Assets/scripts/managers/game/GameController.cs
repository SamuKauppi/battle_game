using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    [SerializeField] private UnitsInLane[] lanes;
    [SerializeField] private Transform p1Start;
    [SerializeField] private Transform p2Start;
    private int playerScores = 0;
    private ObjectPooler pooler;
    [SerializeField] private Slider scoreSlider;

    private void Awake()
    {
        Instance = this;
    }

    public void AddUnit(int alliance, int x, string unitType)
    {
        Transform target = p1Start;
        if (alliance != 1)
        {
            target = p2Start;
        }

        if (!pooler)
            pooler = ObjectPooler.Instance;

        UnitController unit = pooler.GetPooledObject(unitType).GetComponent<UnitController>();

        unit.transform.SetPositionAndRotation(new Vector3(lanes[x].xPos.transform.position.x, 0, target.position.z), target.rotation);
        unit.Xindex = x;
        unit.Zindex = lanes[x].indexCounter;
        lanes[x].indexCounter += 1;
        unit.Alliance = alliance;
        unit.IsAlive = true;
        lanes[x].units.Add(unit);
    }

    public TargetInRange[] CheckEnemy(Vector3 thisPos, Vector3 attackRange, Vector3 stopRange, int alliance, int Xindex, int Zindex, int maxTargets)
    {
        List<TargetInRange> allyInRange = new();
        List<TargetInRange> enemiesInRange = new();

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
                target.TakeDamage(target.hp * 10, "");
                target.gameObject.SetActive(false);
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
                continue; // Opponent unit is behind
            else if (dot == 0)
            {
                target.transform.position += forward * 0.05f;
            }


            bool inAttackRange = Vector3.Distance(thisPos, target.transform.position) < Vector3.Distance(thisPos, attackRange);
            bool inStopRange = Vector3.Distance(thisPos, target.transform.position) < Vector3.Distance(thisPos, stopRange);

            if (alliance != target.Alliance)
            {
                if (!inAttackRange)
                    continue; // enemy is too far from attacking range

                enemiesInRange.Add(new TargetInRange(target, target.Alliance, inStopRange));

                if (enemiesInRange.Count > maxTargets)
                    break;
            }
            else
            {
                if (!inStopRange || allyInRange.Count > 0)
                    continue; // ally is too far from stopping range

                allyInRange.Add(new TargetInRange(target, alliance, inStopRange));
            }


        }

        if (enemiesInRange.Count > 0)
        {
            return enemiesInRange.ToArray();
        }
        else
        {
            return allyInRange.ToArray();
        }
    }

    public Vector3 GetXPos(int index)
    {
        return lanes[index].xPos.position;
    }

    public UnitsInLane[] GetUnits()
    {
        return lanes;
    }

}
