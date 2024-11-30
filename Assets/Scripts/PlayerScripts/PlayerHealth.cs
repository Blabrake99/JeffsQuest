using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageble
{
    [SerializeField] int health;
    [SerializeField, Tooltip("Players IFrames")] protected float damageCooldown;
    [HideInInspector] public Vector3 RespawnPoint;
    protected int startHealth;
    public int Health { get { return health; } set { health = value; } }
    protected float damagedTimer;
    Vector3 _gravity;
    bool _bouncing;
    float _bounceHeight;
    HealthBar bar;
    private void Update()
    {
        if (damagedTimer > 0)
            damagedTimer -= Time.deltaTime;
    }
    public void Damage(int amount)
    {
        if (Time.time > damagedTimer)
        {
            damagedTimer = Time.time + damageCooldown;
            Health -= amount;
            bar.UpdateHealthBar(Health);
        }
        if (Health < 1)
        {
            Respawn();
        }
    }
    public void Respawn()
    {
        //_velocity = Vector3.zero;
        //longJumping = false;
        //isCrouchDeceleration = false;
        Health = startHealth;
        bar.UpdateHealthBar(Health);
        transform.position = RespawnPoint;
    }
    public bool MaxHealth()
    {
        if (health == startHealth)
            return true;
        else
        {
            return false;
        }
    }
    public void GainHealth(int amount)
    {
        if (health + amount > startHealth)
            health = startHealth;
        else
        {
            health += startHealth;
        }
    }
}
