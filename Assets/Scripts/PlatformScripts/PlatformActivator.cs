using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformActivator : MonoBehaviour, IActions
{
    [SerializeField] GameObject platform;
    public void Action()
    {
        platform.GetComponent<IPlatforms>().Activate();
    }
}
