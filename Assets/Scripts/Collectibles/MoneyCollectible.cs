using UnityEngine;

public class MoneyCollectible : MonoBehaviour, ICollectibles
{
    public int currencyGained = 1;
    [SerializeField, Range(0, 100)] float RotationSpeed = 45;
    private void Update()
    {
        transform.Rotate(-Vector3.right * RotationSpeed * Time.deltaTime);
    }
    public void Collected()
    {
        GameManager.GainMoney(currencyGained);
        Destroy(gameObject);
    }
}