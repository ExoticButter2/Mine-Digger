using UnityEngine;

public class RNGSelector : MonoBehaviour
{
    public static RNGSelector instance;

    public enum ObjectType
    {
        Ore
    }

    public Ore defaultOre;

    public Ore[] ores;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Ore SelectRandomOre()
    {
        Ore selectedOre;

        int totalWeight = 0;

        foreach (Ore ore in ores)
        {
            totalWeight += ore.rarity;
        }

        int randomValue = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        foreach (Ore ore in ores)
        {
            cumulativeWeight += ore.rarity;
            if (randomValue < cumulativeWeight)
            {
                selectedOre = ore;
                return selectedOre;
            }
        }

        return defaultOre;
    }

    public Ore SelectRandomObject(ObjectType objectType)
    {
        switch(objectType)
        {
            case ObjectType.Ore:
                return SelectRandomOre();
            default:
                return null;
        }
    }
}