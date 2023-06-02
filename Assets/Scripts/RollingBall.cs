using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollingBall : MonoBehaviour
{
    Rigidbody body;
    Vector3 StartingPos;
    private void Start()
    {
        StartingPos = transform.position;
        body = GetComponent<Rigidbody>();
    }
    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.tag == "ballTrigger")
        {
            body.constraints = RigidbodyConstraints.FreezeAll;
        }
        if(col.gameObject.tag == "DeathBox")
        {
            transform.position = StartingPos;
            body.velocity = Vector3.zero;
        }
    }
}
