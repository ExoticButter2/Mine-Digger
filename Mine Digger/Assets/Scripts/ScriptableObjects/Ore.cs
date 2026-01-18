using UnityEngine;

[CreateAssetMenu(fileName = "New Ore", menuName = "ScriptableObjects/Ore")]
public class Ore : ScriptableObject
{
    public GameObject prefab;
    public string oreName;
    public int value;
    public int health;
    public int rarity;

    public void CreateObject()
    {
        Instantiate(prefab);
    }
}