using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthCollectible : MonoBehaviour, ICollectibles
{
    public int gainedHealth = 1;
    public void Collected()
    {
        Player player = FindObjectOfType<Player>();
        if (player.MaxHealth())
            return;
        else
        {
            player.GainHealth(gainedHealth);
            Destroy(gameObject);
        }
    }
}