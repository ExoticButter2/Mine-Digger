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

        GenerateRows(20);
        RemoveRow(3);
    }

    public void GenerateRows(int generateAmount)
    {
        for (int row = 0; row < generateAmount; row++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    //GameObject positiveValueCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //positiveValueCube.transform.parent = _gridParent;
                    //positiveValueCube.transform.localPosition = new Vector3(x, y, row);

                    GameObject negativeValueCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    negativeValueCube.transform.parent = _gridParent;
                    negativeValueCube.transform.localPosition = new Vector3(-x, -y, row);

                    List<GameObject> objectsInRow = new List<GameObject>();

                    if (_gridDictionary.ContainsKey(row))
                    {
                        objectsInRow = _gridDictionary[row];//get current gameobjects in row
                        //objectsInRow.Add(positiveValueCube);
                        objectsInRow.Add(negativeValueCube);
                    }
                    else
                    {
                        objectsInRow = new List<GameObject>();
                        //objectsInRow.Add(positiveValueCube);
                        objectsInRow.Add(negativeValueCube);
                    }

                        _gridDictionary[row] = objectsInRow;
                    Debug.Log($"Generated mine grid cube for position: {negativeValueCube.transform.localPosition}");
                }
            }
        }
        Debug.Log($"Generated row {_rowsGenerated}");
    }

    public void RemoveRow(int row)
    {
        List<GameObject> objectsInRow = _gridDictionary[row];
        Debug.Log($"Objects in row: {objectsInRow.Count}");

        if (objectsInRow == null)
        {
            Debug.LogWarning("No objects in row");
            return;
        }

        for (int i = objectsInRow.Count - 1; i >= 0; i--)
        {
            Destroy(objectsInRow[i]);
            objectsInRow.RemoveAt(i);
            Debug.Log($"Removed object in index: {i}");
        }

        Debug.Log("Removed row");
    }

    public void RemovePartFromRow(GameObject part, int row)
    {
        if (!_gridDictionary.ContainsKey(row))
        {
            Debug.LogWarning("Row not valid (not created)");
            return;
        }

        List<GameObject> objectsInRow = _gridDictionary[row];

        for (int i = 0; i < objectsInRow.Count; i++)
        {
            if (objectsInRow[i] == part)
            {
                Destroy(objectsInRow[i]);
                objectsInRow.RemoveAt(i);
                Debug.Log("Removed part from row");
            }
        }
    }
}