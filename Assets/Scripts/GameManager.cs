using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    int money = 0;
    public void GainMoney(int gainedCurrancy)
    {
        money += gainedCurrancy;
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