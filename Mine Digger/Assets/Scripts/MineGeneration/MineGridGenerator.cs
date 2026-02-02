using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Rendering;

public class MineGridGenerator : MonoBehaviour
{
    [SerializeField]
    private int _width = 5;

    [SerializeField]
    private int _height = 0;

    private Transform _gridTransform;

    public Dictionary<int, GameObject> gridDictionary;

    private void Awake()
    {
        gridDictionary = new Dictionary<int, GameObject>();
    }

    private void Start()
    {
        _gridTransform = gameObject.transform;

        GenerateChunks();
    }

    public void CreateLinksBetweenChunks(int chunksPerColumn, int chunksPerRow)
    {
        List<Vector2> chunkPositions = new List<Vector2>();

        foreach (GameObject chunkParent in gridDictionary.Values)
        {
            Vector2 chunkPos = new Vector2(chunkParent.transform.position.x, chunkParent.transform.position.z);
            chunkPositions.Add(chunkPos);
        }

        //for (int y = 0; y < chunksPerColumn; y++)
        //{
        //    for (int x = 0; x < chunksPerRow; x++)
        //    {
        //        var current = chunks[(x, y)];
        //        if (x + 1 < chunksPerRow) 
        //        { 
        //            var right = chunks[(x + 1, y)]; 
        //            CreateLinkBetween(current, right); 
        //        }
        //        if (y + 1 < chunksPerColumn) 
        //        { 
        //            var top = chunks[(x, y + 1)]; 
        //            CreateLinkBetween(current, top); 
        //        } 
        //    } 
        //}
    }

    private void GenerateChunks()
    {
        int chunkIndex = 0;

        for (int y = 0; y < _width; y++)
        {
            for (int x = 0; x < _height; x++)
            {
                if (chunkIndex >= _height)
                {
                    return;
                }
                
                Vector3 heightVector = y * _gridTransform.forward;
                Vector3 rightVector = x * _gridTransform.right;
                Vector3 position = Vector3.zero + rightVector + heightVector;

                GenerateChunk(position, 1, _width);

                chunkIndex++;
            }
        }
    }

    private void GenerateChunk(Vector3 position, int newChunkWidth, int newChunkHeight)
    {
        int chunkId = gridDictionary.Count;

        GameObject chunkParentObject = new GameObject();//CREATE CHUNK PARENT OBJECT
        chunkParentObject.transform.SetParent(_gridTransform, false);

        Transform chunkParentTransform = chunkParentObject.transform;
        chunkParentTransform.position = position;
        chunkParentObject.name = $"Chunk {chunkId}";

        NavMeshSurface chunkNavMeshSurface = chunkParentObject.AddComponent<NavMeshSurface>();//SET UP NAVMESH SURFACE
        chunkNavMeshSurface.collectObjects = CollectObjects.Children;

        MineGridChunk chunkComponent = chunkParentObject.AddComponent<MineGridChunk>();//SET UP CHUNK COMPONENT
        chunkComponent.chunkPosition = position;
        chunkComponent.chunkWidth = newChunkWidth;
        chunkComponent.chunkHeight = newChunkHeight;
        chunkComponent.chunkNavMesh = chunkNavMeshSurface;

        Debug.Log($"Generated chunk at position: {position} with width: {newChunkWidth} and height: {newChunkHeight}");

        gridDictionary.Add(chunkId, chunkParentObject);
    }

    public void RemoveChunk(int chunk)
    {
        if (gridDictionary.ContainsKey(chunk))
        {
            Destroy(gridDictionary[chunk]);
            gridDictionary.Remove(chunk);
            Debug.Log($"Removed chunk {chunk} from mine grid");
        }
        else
        {
            Debug.LogWarning($"Chunk {chunk} does not exist in mine grid");
        }
    }
}