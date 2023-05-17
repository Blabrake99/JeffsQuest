using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCollectible : MonoBehaviour, ICollectibles
{
    int gainedCollectibles = 1;
    public void Collected()
    {
        GameManager.GotCollectible(gainedCollectibles);
        Destroy(gameObject);
    }
}
