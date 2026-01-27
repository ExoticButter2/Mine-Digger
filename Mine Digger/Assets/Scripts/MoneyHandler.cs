using System.Runtime.Serialization;
using UnityEngine;

public class MoneyHandler : MonoBehaviour
{
    private int _money = 0;
    public int Money { get { return _money; } private set { _money = value; } }

    private void AddMoney(int amount)
    {
        _money += amount;
    }

    private void OnOreSell(Ore oreData)
    {
        if (oreData == null)
        {
            Debug.LogWarning("No data assigned to ore");
            return;
        }

        AddMoney(oreData.value);
    }

    private void OnEnable()
    {
        SellManager.OnOreSold += OnOreSell;
    }

    private void OnDisable()
    {
        SellManager.OnOreSold -= OnOreSell;
    }
}