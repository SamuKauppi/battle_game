using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazerGun : MonoBehaviour
{
    [SerializeField] private Transform pivotPoint;
    [SerializeField] private LaserPorjectile projectilePrefab;
    public float Lazer_speed { get; private set; }

    private void Start()
    {
        Lazer_speed = projectilePrefab.ReturnSpeed();
    }
    public void ShootGun(Vector3 pos)
    {
        GameObject project = ObjectPooler.Instance.GetPooledObject("lazer");
        project.transform.position = pivotPoint.position;
        project.GetComponent<LaserPorjectile>().ShootTowards(pos);
    }
}
