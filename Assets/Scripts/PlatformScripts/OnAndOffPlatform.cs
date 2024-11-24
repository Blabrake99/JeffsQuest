using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnAndOffPlatform : MonoBehaviour
{
    Collider collider;
    Renderer renderer;
    bool turnOn, wait;
    [SerializeField] float turnOnSpeed = .1f;
    [SerializeField] float turnOffTimer = 1f;
    [SerializeField] float turnOnTimer = 1f;
    float colorAlpha = 1;
    void Start()
    {
        collider = GetComponent<Collider>();
        renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!wait)
        {
            if (turnOn)
            {
                colorAlpha += turnOnSpeed * Time.fixedDeltaTime;
                renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, colorAlpha);
                if (renderer.material.color.a >= 1)
                {
                    wait = true;
                    Invoke("SwitchPlatfromState", turnOnTimer);
                }
            }
            else
            {
                colorAlpha -= turnOnSpeed * Time.fixedDeltaTime;
                renderer.material.color = new Color(renderer.material.color.r, renderer.material.color.g, renderer.material.color.b, colorAlpha);
                if (renderer.material.color.a <= 0)
                {
                    wait = true;
                    Invoke("SwitchPlatfromState", turnOffTimer);
                    collider.enabled = false;
                }
            }
        }
    }
    void SwitchPlatfromState()
    {
        turnOn = !turnOn;
        wait = false;
        collider.enabled = true;
    }
}
