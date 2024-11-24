using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Fan : MonoBehaviour
{
    [SerializeField] float pushForce = 20f;
    List<Rigidbody> rigidbodies = new List<Rigidbody>();
    [SerializeField] Vector3 direction = new Vector3(1, 0, 0);
    [SerializeField] float dragforce = 3;
    private void Update()
    {
        if (rigidbodies.Count > 0)
        {
            for (int i = 0; i < rigidbodies.Count; i++)
            {
                if (rigidbodies[i] != null)
                {
                    Rigidbody rb = rigidbodies[i];
                    if(rb.drag < dragforce)
                    {
                        rb.drag += .01f;
                    }
                    else
                    {
                        rb.drag = dragforce;
                    }
                    if (rb.mass < 1)
                    {
                        rb.mass += .02f;
                    }
                    rb.AddForce(direction * pushForce, ForceMode.Force);
                }
            }
        }
    }
    private void OnTriggerStay(Collider col)
    {
        Rigidbody rb = col.GetComponent<Rigidbody>();
        if (rb != null && !rigidbodies.Contains(rb))
        {
            Add(rb);
        }
    }
    private void OnTriggerEnter(Collider col)
    {
        Rigidbody rb = col.GetComponent<Rigidbody>();
        if (rb != null && !rigidbodies.Contains(rb))
        {
            Add(rb);
            rb.mass = .1f;
        }
    }
    private void OnTriggerExit(Collider col)
    {
        Rigidbody rb = col.GetComponent<Collider>().GetComponent<Rigidbody>();
        if (rb != null)
        {
            Remove(rb);
            rb.drag = 0;
        }
    }
    private void OnCollisionEnter(Collision col)
    {
        Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();
        if (rb != null && !rigidbodies.Contains(rb))
        {
            Add(rb);
        }
    }
    private void OnCollisionStay(Collision col)
    {
        Rigidbody rb = col.collider.GetComponent<Rigidbody>();
        if (rb != null && !rigidbodies.Contains(rb))
        {
            Add(rb);
        }
    }
    private void OnCollisionExit(Collision col)
    {
        Rigidbody rb = col.collider.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Remove(rb);
        }
    }
    void Add(Rigidbody rb)
    {
        if (!rigidbodies.Contains(rb))
            rigidbodies.Add(rb);
    }
    void Remove(Rigidbody rb)
    {
        if (rigidbodies.Contains(rb))
            rigidbodies.Remove(rb);
    }
}
