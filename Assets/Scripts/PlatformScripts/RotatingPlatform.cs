using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class RotatingPlatform : MonoBehaviour
{
    [SerializeField] float rotateSpeed = 15f;
    [SerializeField] Vector3 rotateDirection = new Vector3 (0, 1, 0);
    
    void FixedUpdate()
    {
        transform.Rotate(rotateDirection * rotateSpeed * Time.fixedDeltaTime); 
    }
    private void OnTriggerEnter(Collider col)
    {
        Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();
        if (rb != null )
        {
            col.gameObject.transform.SetParent( transform,true);
        }
    }
    private void OnTriggerExit(Collider col)
    {
        Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            col.gameObject.transform.parent = null;
        }
    }
}
