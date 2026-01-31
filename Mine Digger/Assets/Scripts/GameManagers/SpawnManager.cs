using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;

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
    
    public GameObject SpawnOre(Ore ore, Vector3 position, Quaternion rotation, Transform parent, bool inWorldSpace)
    {
        Debug.Log("Spawning ore");
        GameObject oreObj = Instantiate(ore.prefab, Vector3.zero, rotation, parent);
        oreObj.name = ore.oreName;
        if (inWorldSpace)
        {
            oreObj.transform.position = position;
        }
        else
        {
            oreObj.transform.localPosition = position;
        }

        OreBehaviour oreObjBehaviour = oreObj.AddComponent<OreBehaviour>();
        oreObjBehaviour.oreData = Instantiate(ore);
        Debug.Log("Spawned ore");

        return oreObj;
    }
}