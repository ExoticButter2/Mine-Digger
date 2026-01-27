using UnityEngine;
using System;

public class OreBehaviour : MonoBehaviour
{
    public Ore oreData;
    public static event Action<Ore> OnOreDestroyed;

    public void TakeDamage(int damage)//make ore take damage
    {
        oreData.health -= damage;
        Debug.Log($"{oreData.oreName} took {damage} damage, remaining health: {oreData.health}");
        if (oreData.health <= 0)//if ore is dead
        {
            OnOreDestroyed?.Invoke(oreData);//fire ore destroy event to listeners
            DestroyOre();
        }
    }

    private void DestroyOre()
    {
        Destroy(gameObject);
    }
}