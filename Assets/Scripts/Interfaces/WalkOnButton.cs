using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WalkOnButton : MonoBehaviour
{
    List<Rigidbody> bodies = new List<Rigidbody>();
    bool active;
    [SerializeField] GameObject[] ObjectsToActive;
    [SerializeField] bool StepOnOnce = true;
    private void OnTriggerEnter(Collider col)
    {
        Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();
        if (rb != null && !bodies.Contains(rb))
        {
            Add(rb);
        }
    }
    private void OnTriggerExit(Collider col)
    {
        Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Remove(rb);
        }
    }
    private void OnTriggerStay(Collider col)
    {
        Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();
        if (rb != null && !bodies.Contains(rb))
        {
            Add(rb);
        }
    }
    void CheckIfActive()
    {
        foreach (Rigidbody body in bodies)
        {
            if (body != null)
            {
                return;
            }
        }
        print("working");
        DeActive();

    }
    void Active()
    {
        foreach (GameObject g in ObjectsToActive)
        {
            g.GetComponent<IPlatforms>().Activate();
        }
        active = true;
    }
    void DeActive()
    {
        foreach (GameObject g in ObjectsToActive)
        {
            g.GetComponent<IPlatforms>().DeActivate();
        }
        active = false;
    }
    void Add(Rigidbody rb)
    {
        if (!bodies.Contains(rb))
        {
            bodies.Add(rb);
            Active();
        }
    }
    void Remove(Rigidbody rb)
    {
        if (bodies.Contains(rb))
        {
            bodies.Remove(rb);
            if(!StepOnOnce)
                CheckIfActive();
        }
    }
}
