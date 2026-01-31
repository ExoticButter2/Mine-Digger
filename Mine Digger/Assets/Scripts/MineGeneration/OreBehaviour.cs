using UnityEngine;
using System;

public class OreBehaviour : MonoBehaviour
{
    public Ore oreData;
    public static event Action<Ore, GameObject, int> BeforeOreDestroyed;
    public int row;

    public bool currentlyMinedByAi;

    public void TakeDamage(float damage)//make ore take damage
    {
        oreData.health -= damage;
        Debug.Log($"{oreData.oreName} took {damage} damage, remaining health: {oreData.health}");
        if (oreData.health <= 0)//if ore is dead
        {
            DestroyOre();
        }
    }

    private void DestroyOre()
    {
        BeforeOreDestroyed?.Invoke(oreData, gameObject, row);//fire ore destroy event to listeners
    }
}