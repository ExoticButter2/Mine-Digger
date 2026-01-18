using UnityEngine;

public class OreBehaviour : MonoBehaviour
{
    public Ore oreData;

    public void TakeDamage(int damage)
    {
        oreData.health -= damage;
        Debug.Log($"{oreData.oreName} took {damage} damage, remaining health: {oreData.health}");
        if (oreData.health <= 0)
        {
            DestroyOre();
        }
    }

    private void DestroyOre()
    {
        Debug.Log($"{oreData.oreName} has been destroyed!");
        Destroy(gameObject);
    }
}