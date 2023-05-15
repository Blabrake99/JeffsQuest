using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyFragment : MonoBehaviour, ICollectibles
{
    [SerializeField] KeyFragDoor door;
    public void Collected()
    {
        door.GotKeyFragment(gameObject);
    }
}
