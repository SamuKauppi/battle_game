using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LazerGun : MonoBehaviour
{
    [SerializeField] private Transform pivotPoint;
    public float lazer_speed;
    public void ShootGun(Vector3 pos)
    {
        GameObject project = ObjectPooler.Instance.GetPooledObject("lazer");
        project.transform.position = pivotPoint.position;
        LaserPorjectile lazer = project.GetComponent<LaserPorjectile>();
        lazer.ShootTowards(pos);
        lazer_speed = lazer.ReturnSpeed();
    }
}
