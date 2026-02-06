using System.Collections.Generic;
using System.Linq;
using System.Net;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.U2D.ScriptablePacker;

public class MineGridChunk : MonoBehaviour
{
    public int chunkId = 0;
    public NavMeshSurface chunkNavMesh;

    private Transform _chunkTransform;
    public Vector3 chunkPosition;

    public int chunkWidth = 3;
    public int chunkHeight = 3;

    private int _rowsGenerated = 0;

    public MineGridGenerator mineGrid;

    public Dictionary<int, List<GameObject>> chunkDepthDictionary;//int for depth, list of gameobjects in chunk

    private void Start()
    {
        chunkDepthDictionary = new Dictionary<int, List<GameObject>>();
        _chunkTransform = gameObject.transform;
        GenerateRow(2);
        GenerateLinks();
    }

    private NavMeshLink CreateLinkInstance(GameObject targetObject, Transform partLinkTransformA, Transform partLinkTransformB)
    {
        NavMeshLink link = targetObject.AddComponent<NavMeshLink>();
        link.bidirectional = true;
        link.startPoint = new Vector3(partLinkTransformA.position.x, partLinkTransformA.position.y + partLinkTransformA.localScale.y / 2, partLinkTransformA.position.z);
        link.endPoint = new Vector3(partLinkTransformB.position.x, partLinkTransformB.position.y + partLinkTransformB.localScale.y / 2, partLinkTransformB.position.z);
        link.enabled = true;
        link.UpdateLink();

        return link;
    }

    private void UpdateBlockLink(NavMeshLink link)
    {
        Transform linkObjectTransform = link.gameObject.transform;

        Transform partLinkTransformA = linkObjectTransform;
        link.startPoint = new Vector3(partLinkTransformA.position.x, partLinkTransformA.position.y + partLinkTransformA.localScale.y / 2, partLinkTransformA.position.z);
        link.enabled = true;
        link.UpdateLink();
    }

    private void GenerateLinksForChunk(int chunkIndex)
    {
        Dictionary<int, GameObject> gridChunkDictionary = mineGrid.gridDictionary;//dictionary of all chunks in grid
        MineGridChunk currentChunk = gridChunkDictionary[chunkIndex].GetComponent<MineGridChunk>();

        List<GameObject> highestCurrentChunkParts = currentChunk.chunkDepthDictionary.FirstOrDefault().Value;//list of parts in current chunk

        for (int i = 0; i < highestCurrentChunkParts.Count - 1; i++)//link every part in selected chunk
        {
            Transform startPartTransform = highestCurrentChunkParts[i].transform;
            Transform endPartTransform = highestCurrentChunkParts[i + 1].transform;

            NavMeshLink link = CreateLinkInstance(highestCurrentChunkParts[i], startPartTransform, endPartTransform);
        }

        if (!gridChunkDictionary.ContainsKey(chunkIndex + 1))//if no neighboring chunk
        {
            Debug.LogWarning("No neighboring chunk to link");
            return;
        }

        MineGridChunk neighborChunk = gridChunkDictionary[chunkIndex + 1].GetComponent<MineGridChunk>();
        if (neighborChunk == null)
        {
            Debug.LogWarning("No neighbor chunk found");
            return;
        }

        List<GameObject> highestNeighborChunkParts = neighborChunk.chunkDepthDictionary[neighborChunk.chunkDepthDictionary.Keys.Max()];

        for (int i = 0; i < highestCurrentChunkParts.Count - 1; i++)
        {
            Transform startPartTransform = highestCurrentChunkParts[i].transform;
            Transform endPartTransform = highestNeighborChunkParts[i].transform;

            NavMeshLink link = CreateLinkInstance(highestCurrentChunkParts[i], startPartTransform, endPartTransform);
        }
    }

    private void GenerateLinks()
    {
        for (int chunkIndex = 0; chunkIndex < mineGrid.gridDictionary.Count; chunkIndex++)
        {
            GameObject chunkParent = mineGrid.gridDictionary[chunkIndex];
            MineGridChunk chunkComponent = chunkParent.GetComponent<MineGridChunk>();

            Dictionary<int, List<GameObject>> currentChunkDictionary = chunkComponent.chunkDepthDictionary;//dictionary of all parts in chunk

            GenerateLinksForChunk(chunkIndex);
        }
    }

    //private void GenerateLinkOnNeighbors(int index)
    //{
    //    Transform startTransform = chunkDepthDictionary.FirstOrDefault().Value[index].transform;
    //    if (mineGrid.gridDictionary.ContainsKey(chunkId + 1))
    //    {
    //        MineGridChunk neighborChunk = mineGrid.gridDictionary[index + 1].GetComponent<MineGridChunk>();
    //        Transform neighborTransform = neighborChunk.chunkDepthDictionary.FirstOrDefault().Value[index].transform;
    //    }

    //    if (chunkDepthDictionary.FirstOrDefault().Value[index + 1] != null)
    //    {
    //        Transform nextInChunkTransform = chunkDepthDictionary.FirstOrDefault().Value[index + 1].transform;
    //    }
    //}

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

                    if (!chunkDepthDictionary.ContainsKey(_rowsGenerated))//if row doesn't exist
                    {
                        chunkDepthDictionary.Add(_rowsGenerated, new List<GameObject>());//add list for row
                    }

                    chunkDepthDictionary[_rowsGenerated].Add(negativeValueCube);//add cube to row
                }
            }
            _rowsGenerated++;
        }

        chunkNavMesh.BuildNavMesh();
    }

    public void RemoveRow(int row)
    {
        if (!chunkDepthDictionary.ContainsKey(row))
        {
            Debug.LogWarning("Row doesn't exist in grid");
            return;
        }

        List<GameObject> objectsInRow = chunkDepthDictionary[row];
        Debug.Log($"Objects in row: {objectsInRow.Count}");

        for (int i = objectsInRow.Count - 1; i >= 0; i--)
        {
            Destroy(objectsInRow[i]);
            objectsInRow.RemoveAt(i);
            Debug.Log($"Removed object in index: {i}");
        }

        chunkDepthDictionary.Remove(row); Debug.Log($"Removed empty row {row} from dictionary");
    }

    public void RemovePartFromRow(Ore oreData, GameObject part, int row)
    {
        if (!chunkDepthDictionary.ContainsKey(row))//check if row exists
        {
            Debug.LogWarning("Row not valid (not created)");
            return;
        }

        List<GameObject> objectsInRow = chunkDepthDictionary[row];

        for (int i = 0; i < objectsInRow.Count; i++)//check each object in row
        {
            if (objectsInRow[i] == part)
            {
                Destroy(objectsInRow[i]);
                objectsInRow.RemoveAt(i);

                Debug.Log("Removed part from row");
                chunkNavMesh.BuildNavMesh();
                break;
            }
        }

        if (objectsInRow.Count == 0)
        {
            RemoveRow(row);
            return;
        }
        Debug.LogWarning("Part not found in row");
    }

    public GameObject GetFirstAvailableOreInChunk()
    {
        for (int i = 0; i < _rowsGenerated; i++)
        {
            if (!chunkDepthDictionary.ContainsKey(i))
            {
                Debug.LogWarning($"Row {i} does not exist in grid dictionary");
                continue;
            }

            List<GameObject> ores = chunkDepthDictionary[i];

            if (ores == null)
            {
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