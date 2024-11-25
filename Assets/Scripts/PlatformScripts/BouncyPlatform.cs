using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyPlatform : MonoBehaviour, IPlatforms
{
    [SerializeField, Tooltip("How hight the player bounces off of this"), Range(0,100)] float bounceHeight = 1f;
    bool active = true;
    public void Activate()
    {
        active = true;
    }
    public void DeActivate()
    {
        active = false;
    }
    private void OnTriggerEnter(Collider col)
    {
        Player player = col.GetComponent<Player>();
        if(player != null)
        {
            player.Bounce(bounceHeight);
        }
    }
}
