using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Rendering;

public class MineGridGenerator : MonoBehaviour
{
    public int height = 5;
    public int width = 5;

    private int _rowsGenerated = 0;

    private Transform _gridParent;

    public Dictionary<int, List<GameObject>> gridDictionary;

    public NavMeshSurface navMesh;

    private void Awake()
    {
        gridDictionary = new Dictionary<int, List<GameObject>>();
    }

    private void Start()
    {
        _gridParent = gameObject.transform;

        GenerateRows(250);
    }

    public void GenerateRows(int generateAmount)
    {
        for (int row = 0; row < generateAmount; row++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 cubePosition = new Vector3(-x, -y, _rowsGenerated);
                    Ore randomOre = RNGSelector.instance.SelectRandomObject(RNGSelector.ObjectType.Ore);
                    Vector3 forwardVector = _rowsGenerated * _gridParent.forward;
                    Vector3 upVector = -y * _gridParent.up;
                    Vector3 rightVector = -x * _gridParent.right;
                    Vector3 targetVector = forwardVector + upVector + rightVector;

                    Vector3 targetPosition = _gridParent.InverseTransformVector(targetVector);
                    GameObject negativeValueCube = SpawnManager.instance.SpawnOre(randomOre, targetPosition, _gridParent.rotation, _gridParent, false);//create prefab
                    negativeValueCube.transform.parent = _gridParent;

                    OreBehaviour oreBehaviour = negativeValueCube.GetComponent<OreBehaviour>();
                    oreBehaviour.row = row;

                    if (!gridDictionary.ContainsKey(_rowsGenerated))
                    {
                        gridDictionary[_rowsGenerated] = new List<GameObject>();
                    }

                    gridDictionary[_rowsGenerated].Add(negativeValueCube);
                    Debug.Log($"Generated mine grid cube for position: {negativeValueCube.transform.localPosition}");
                }
            }
            _rowsGenerated++;
        }
        Debug.Log($"Generated row {_rowsGenerated}");
        navMesh.BuildNavMesh();
    }

    public void RemoveRow(int row)
    {
        if (gridDictionary.ContainsKey(row))
        {
            List<GameObject> objectsInRow = gridDictionary[row];
            Debug.Log($"Objects in row: {objectsInRow.Count}");

            for (int i = objectsInRow.Count - 1; i >= 0; i--)
            {
                Destroy(objectsInRow[i]);
                objectsInRow.RemoveAt(i);
                Debug.Log($"Removed object in index: {i}");
            }
        }

        Debug.Log("Removed row");
        navMesh.BuildNavMesh();
    }

    public void RemovePartFromRow(Ore oreData, GameObject part, int row)
    {
        if (!gridDictionary.ContainsKey(row))//check if row exists
        {
            Debug.LogWarning("Row not valid (not created)");
            return;
        }

        List<GameObject> objectsInRow = gridDictionary[row];

        for (int i = 0; i < objectsInRow.Count; i++)//check each object in row
        {
            if (objectsInRow[i] == part)
            {
                Destroy(objectsInRow[i]);
                objectsInRow.RemoveAt(i);
                Debug.Log("Removed part from row");
                return;
            }
        }

        Debug.LogWarning("Part not found in row");
        navMesh.BuildNavMesh();
    }

    public GameObject GetFirstAvailableOreInMine()
    {
        for (int i = 0; i < _rowsGenerated; i++)
        {
            List<GameObject> ores = gridDictionary[i];

            if (ores == null)
            {
                continue;
            }

            foreach (GameObject ore in ores)
            {
                OreBehaviour oreBehaviour = ore.GetComponent<OreBehaviour>();
                if (!oreBehaviour.currentlyMinedByAi)
                {
                    Debug.Log($"First available ore found at row {i}, position {ore.transform.position}");
                    return ore;
                }
            }

            return ores.FirstOrDefault();
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