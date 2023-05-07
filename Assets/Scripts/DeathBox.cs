using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBox : MonoBehaviour
{
    [SerializeField] int damageAmount = 20;
    private void OnTriggerEnter(Collider col)
    {
        IDamageble hit = col.GetComponent<IDamageble>();
        if (hit != null)
        {
            hit.Damage(damageAmount);
        }
    }
}
