using UnityEngine;
using System;

public class SellManager : MonoBehaviour
{
    public static event Action<Ore> OnOreSold;

    public void SellOre(Ore oreData)
    {
        OnOreSold?.Invoke(oreData);
    }
}