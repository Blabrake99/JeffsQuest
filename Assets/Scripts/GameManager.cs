using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static int money = 0;
    static int collectibles = 0;
    static Player _player;
    private void Start()
    {
        _player = FindObjectOfType<Player>();
    }
    public static void GainMoney(int gainedCurrancy)
    {
        money += gainedCurrancy;
    }
    public static void GotCollectible(int GainedCollectibles)
    {
        collectibles += collectibles;
        _player.GotCollectible();
    }
    public void LoseMoney(int lostCurrancy)
    {
        if (money - lostCurrancy > 0)
            money -= lostCurrancy;
    }
    public bool CanBuyItem(int itemCost)
    {
        if (money >= itemCost)
            return true;
        else
            return false;
    }
    #region Cursor methods
    public static void lockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public static void unlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    #endregion
}