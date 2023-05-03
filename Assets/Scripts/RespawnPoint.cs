using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider col)
    {
        if (col.TryGetComponent<Player>(out var player))
        {
            // anim.SetBool("Spin", true);
            player.RespawnPoint = transform.position + new Vector3(0, .5f, 0);
        }
    }
}
