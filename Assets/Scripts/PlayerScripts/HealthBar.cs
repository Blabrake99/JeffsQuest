using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Photon.Pun;
public class HealthBar : MonoBehaviour
{
    [SerializeField] GameObject[] HealthBars = new GameObject[4];
    GameObject player;
    //private void Update()
    //{
    //    if (player == null)
    //        player = PhotonFindCurrentClient();
    //}

    //public void FindNewHealthBar()
    //{
    //    if (player.GetComponent<PlayerHealth>().currentHealth > 0)
    //    {
    //        HealthBars[player.GetComponent<PlayerHealth>().currentHealth].SetActive(false);
    //        HealthBars[player.GetComponent<PlayerHealth>().currentHealth - 1].SetActive(true);
    //    }
    //    else
    //    {
    //        HealthBars[player.GetComponent<PlayerHealth>().currentHealth].SetActive(false);
    //        HealthBars[3].SetActive(true);
    //    }
    //}
    //GameObject PhotonFindCurrentClient()
    //{
    //    GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

    //    foreach (GameObject g in players)
    //    {
    //        if (g.GetComponent<PhotonView>().IsMine)
    //            return g;
    //    }
    //    return null;
    //}
}