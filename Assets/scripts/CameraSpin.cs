using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSpin : MonoBehaviour
{
    private float maxXDistance;
    private Vector3 direction;


    private void Start()
    {
        maxXDistance = Mathf.Abs(transform.position.x);
        direction = new Vector3(1f, 0, 0);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(Vector3.up, 0.1f);
        transform.position += Time.fixedDeltaTime * direction;
        if (Mathf.Abs(transform.position.x) > maxXDistance)
            direction *= -1;
    }
}
