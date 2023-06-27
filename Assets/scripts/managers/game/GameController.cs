using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    [SerializeField] private UnitsInLane[] lanes;
    [SerializeField] private Vector3 distance;
    private int playerScores = 0;
    private ObjectPooler pooler;
    [SerializeField] private Slider scoreSlider;

    private void Awake()
    {
        Instance = this;
    }

    public void AddUnit(int alliance, int x, Vector3 pos, Quaternion rot, string unitType)
    {
        if (!pooler)
            pooler = ObjectPooler.Instance;

        UnitController unit = pooler.GetPooledObject(unitType).GetComponent<UnitController>();

        unit.transform.SetPositionAndRotation(pos, rot);
        unit.Xindex = x;
        unit.Zindex = lanes[x].indexCounter;
        lanes[x].indexCounter += 1;
        unit.Alliance = alliance;
        unit.IsAlive = true;
        lanes[x].units.Add(unit);
    }

    public TargetInRange[] CheckEnemy(Vector3 thisPos, Vector3 attackRange, Vector3 stopRange, int alliance, int Xindex, int Zindex, int maxTargets)
    {
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
                target.transform.position += forward * 0.05f;
                continue; // target is on top of this
            }

            float distanceToTarget = Vector3.Distance(thisPos, target.transform.position);
            bool inAttackRange = distanceToTarget < Vector3.Distance(thisPos, attackRange);
            bool inStopRange = distanceToTarget < Vector3.Distance(thisPos, stopRange);

            if (alliance != target.Alliance)
            {
                if (!inAttackRange)
                    continue; // enemy is too far from attacking range


                if (enemyIndex >= maxTargets)
                {
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
                    if (shortestDistIdex != 0)
                    {
                        targets[shortestDistIdex] = new TargetInRange(target, target.Alliance, inStopRange, distanceToTarget);
                    }
                }
                else
                {
                    enemyIndex++;
                    targets[enemyIndex] = new TargetInRange(target, target.Alliance, inStopRange, distanceToTarget);
                }
            }
            else
            {
                if (!inStopRange || allyFound)
                    continue; // ally is too far from stopping range or an ally has already been found
                targets[0] = new TargetInRange(target, alliance, inStopRange, distanceToTarget);
                allyFound = true;
            }
        }
        return targets;
    }

    public Vector3 GetXPos(int index)
    {
        return lanes[index].xPos.position;
    }

    public UnitsInLane[] GetLaneData()
    {
        return lanes;
    }

}
