using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlling the movement of a projectile
/// </summary>
public class LaserPorjectile : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float shotDist;
    public void ShootTowards(Vector3 pos)
    {
        pos += new Vector3(0, 0.25f, 0);
        StartCoroutine(Move(pos));
    }

    IEnumerator Move(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > shotDist)
        {
            transform.LookAt(target);
            transform.position += speed * Time.deltaTime * transform.forward;
            yield return null;
        }
        gameObject.SetActive(false);
    }

    public float ReturnSpeed()
    {
        return speed;
    }
}
