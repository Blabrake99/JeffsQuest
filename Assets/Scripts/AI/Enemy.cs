using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour, IDamageble
{
    [SerializeField, Tooltip("Players health")] protected int health = 6;
    public int Health { get { return health; } set { health = value; } }
    protected Transform target;
    public STATE current_state = STATE.IDLE;

    public void Damage(int amount)
    {
        Health -= amount;
        if (health <= 0)
            Die();
    }
    void Start()
    {
        
    }
    void Update()
    {
        
    }
    void Die()
    {
        gameObject.SetActive(false);
    }
    public enum STATE
    {
        IDLE,
        CHASE,
        ATTACK,
        FLEE,
        DEAD,
        STUN

    }
}
