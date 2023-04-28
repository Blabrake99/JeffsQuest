using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jeff : Player
{
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();

    }
    void FixedUpdate()
    {
        base.FixedUpdate();
    }
    private void OnTriggerStay(Collider col)
    {
        base.OnTriggerStay(col);
    }
    private void OnTriggerExit(Collider col)
    {
        base.OnTriggerExit(col); 
    }
    private void OnCollisionStay(Collision col)
    {
        base.OnCollisionStay(col);
    }
    private void OnCollisionExit(Collision col)
    {
        base.OnCollisionExit(col);
    }
}
