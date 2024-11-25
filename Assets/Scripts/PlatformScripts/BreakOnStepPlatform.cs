using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakOnStepPlatform : MonoBehaviour, IPlatforms
{
    [SerializeField] float turnOffTime = 3;
    [SerializeField] bool breakRegaurdlessOfPlayer = true;
    [SerializeField] float StayTurnedOffFor = 3;
    bool active;
    float timer;
    [SerializeField] Collider collider;
    bool turnedOff;
    Renderer rend;
    public void Activate()
    {
        throw new System.NotImplementedException();
    }

    public void DeActivate()
    {
        throw new System.NotImplementedException();
    }
    private void Start()
    {
        rend = GetComponent<Renderer>();
    }
    void Update()
    {
        if(active)
        {
            timer -= Time.deltaTime;
            if(timer < 0 && !turnedOff)
            {
                collider.enabled = false;
                rend.enabled = false;
                timer = StayTurnedOffFor;
                turnedOff = true;
            }
            if (timer < 0 && turnedOff)
            {
                collider.enabled = true;
                rend.enabled = true;
                timer = StayTurnedOffFor;
                turnedOff = false;
                active = false;
            }
        }
    }
    private void OnTriggerEnter(Collider col)
    {
        Player player = col.GetComponent<Player>();
        if (player != null)
        {
            timer = turnOffTime;
            active = true;
        }
    }
    private void OnTriggerExit(Collider col)
    {
        Player player = col.GetComponent<Player>();
        if (player != null && !breakRegaurdlessOfPlayer)
        {
            active = false;
        }
    }
}
