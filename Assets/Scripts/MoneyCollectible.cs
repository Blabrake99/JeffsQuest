using UnityEngine;

public class MoneyCollectible : MonoBehaviour, ICollectibles
{
    public int currencyGained = 100;
    public void Collected()
    {
        FindObjectOfType<GameManager>().GainMoney(currencyGained);
        Destroy(gameObject);
    }
}