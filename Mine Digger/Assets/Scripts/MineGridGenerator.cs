using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MineGridGenerator : MonoBehaviour
{
    public int height = 5;
    public int width = 5;

    private int _rowsGenerated = 0;

    private Transform _gridParent;

    private Dictionary<int, List<GameObject>> _gridDictionary;

    private void Start()
    {
        _gridParent = gameObject.transform;
        _gridDictionary = new Dictionary<int, List<GameObject>>();

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
                    GameObject negativeValueCube = SpawnManager.instance.SpawnOre(randomOre, _gridParent.position + cubePosition, _gridParent.rotation, _gridParent, true);//create prefab
                    negativeValueCube.transform.parent = _gridParent;

                    if (!_gridDictionary.ContainsKey(_rowsGenerated))
                    {
                        _gridDictionary[_rowsGenerated] = new List<GameObject>();
                    }

                    _gridDictionary[_rowsGenerated].Add(negativeValueCube);
                    Debug.Log($"Generated mine grid cube for position: {negativeValueCube.transform.localPosition}");
                }
            }
            _rowsGenerated++;
        }
        Debug.Log($"Generated row {_rowsGenerated}");
    }

    public void RemoveRow(int row)
    {
        if (_gridDictionary.ContainsKey(row))
        {
            List<GameObject> objectsInRow = _gridDictionary[row];
            Debug.Log($"Objects in row: {objectsInRow.Count}");

            for (int i = objectsInRow.Count - 1; i >= 0; i--)
            {
                Destroy(objectsInRow[i]);
                objectsInRow.RemoveAt(i);
                Debug.Log($"Removed object in index: {i}");
            }
        }

        Debug.Log("Removed row");
    }

    public void RemovePartFromRow(GameObject part, int row)
    {
        if (!_gridDictionary.ContainsKey(row))//check if row exists
        {
            Debug.LogWarning("Row not valid (not created)");
            return;
        }

        List<GameObject> objectsInRow = _gridDictionary[row];

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
    }
}