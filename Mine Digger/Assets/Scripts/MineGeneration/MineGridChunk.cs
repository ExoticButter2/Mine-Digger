using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class MineGridChunk : MonoBehaviour
{
    public NavMeshSurface chunkNavMesh;

    private Transform _chunkTransform;
    public Vector3 chunkPosition;

    public int chunkWidth = 3;
    public int chunkHeight = 3;

    private int _rowsGenerated = 0;

    public Dictionary<int, List<GameObject>> chunkDictionary;

    private void Awake()
    {
        chunkDictionary = new Dictionary<int, List<GameObject>>();
    }

    private void Start()
    {
        _chunkTransform = gameObject.transform;
        GenerateRow(2);
    }

    public void GenerateRow(int repeatCount)
    {
        Debug.Log($"Chunk width: {chunkWidth} Height: {chunkHeight}");
        for (int row = 0; row < repeatCount; row++)
        {
            for (int x = 0; x < chunkWidth; x++)
            {
                for (int y = 0; y < chunkHeight; y++)
                {
                    Ore randomOre = RNGSelector.instance.SelectRandomObject(RNGSelector.ObjectType.Ore);
                    Vector3 forwardVector = -y * _chunkTransform.forward;
                    Vector3 rightVector = -x * _chunkTransform.right;
                    Vector3 downVector = -_chunkTransform.up * _rowsGenerated;
                    Vector3 targetPosition = forwardVector + rightVector + downVector;

                    GameObject negativeValueCube = SpawnManager.instance.SpawnOre(randomOre, targetPosition, _chunkTransform.rotation, _chunkTransform, false);//create prefab

                    if (!chunkDictionary.ContainsKey(_rowsGenerated))//if row doesn't exist
                    {
                        chunkDictionary.Add(_rowsGenerated, new List<GameObject>());//add list for row
                    }

                    chunkDictionary[_rowsGenerated].Add(negativeValueCube);//add cube to row
                }
            }
            _rowsGenerated++;
        }

        chunkNavMesh.BuildNavMesh();
    }

    public void RemoveRow(int row)
    {
        if (chunkDictionary.ContainsKey(row))
        {
            List<GameObject> objectsInRow = chunkDictionary[row];
            Debug.Log($"Objects in row: {objectsInRow.Count}");

            for (int i = objectsInRow.Count - 1; i >= 0; i--)
            {
                Destroy(objectsInRow[i]);
                objectsInRow.RemoveAt(i);
                Debug.Log($"Removed object in index: {i}");
            }
        }

        Debug.Log("Removed row");
        chunkNavMesh.BuildNavMesh();
    }

    public void RemovePartFromRow(Ore oreData, GameObject part, int row)
    {
        if (!chunkDictionary.ContainsKey(row))//check if row exists
        {
            Debug.LogWarning("Row not valid (not created)");
            return;
        }

        List<GameObject> objectsInRow = chunkDictionary[row];

        for (int i = 0; i < objectsInRow.Count; i++)//check each object in row
        {
            if (objectsInRow[i] == part)
            {
                Destroy(objectsInRow[i]);
                objectsInRow.RemoveAt(i);
                Debug.Log("Removed part from row");
                chunkNavMesh.BuildNavMesh();
                return;
            }
        }

        Debug.LogWarning("Part not found in row");
    }

    public GameObject GetFirstAvailableOreInMine()
    {
        for (int i = 0; i < _rowsGenerated; i++)
        {
            List<GameObject> ores = chunkDictionary[i];

            if (ores == null)
            {
                continue;
            }

            if (!chunkDictionary.ContainsKey(i))
            {
                Debug.LogWarning($"Row {i} does not exist in grid dictionary");
                continue;
            }

            foreach (GameObject ore in ores)
            {
                OreBehaviour oreBehaviour = ore.GetComponent<OreBehaviour>();
                Debug.Log($"Ore {ore.name} minedByAi = {oreBehaviour.currentlyMinedByAi}");
                Debug.Log($"Row {i} has {ores.Count} ores");

                if (oreBehaviour.minerAIminingOre == null)
                {
                    Debug.Log($"First available ore found at row {i}, position {ore.transform.position}");
                    return ore;
                }
            }
        }

        Debug.LogWarning("No available ore found in mine");
        return null;
    }

    private void OnEnable()
    {
        OreBehaviour.BeforeOreDestroyed += RemovePartFromRow;//remove from mine grid dictionary
    }

    private void OnDisable()
    {
        OreBehaviour.BeforeOreDestroyed -= RemovePartFromRow;
    }
}