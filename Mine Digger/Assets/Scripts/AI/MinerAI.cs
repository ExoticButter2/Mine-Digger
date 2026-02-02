using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class MinerAI : MonoBehaviour
{
    public NavMeshAgent aiAgent;
    public float maxCarryCapacity = 20f;
    public float currentCarryAmount = 0f;

    public MiningTool minerTool;

    [SerializeField]
    private MineGridGenerator _mineGrid;//mine ai should focus on
    private Dictionary<int, GameObject> _gridDictionary;

    private MinerStates _currentState = MinerStates.Idle;

    private bool _miningOre;

    private Transform _aiModelTransform;

    private GameObject _targetChunk;
    private GameObject _targetOre;
    private OreBehaviour _targetOreBehaviour;
    private bool _headingToTarget;

    private enum MinerStates
    {
        Idle,
        Mining,
        Return
    }

    #region State Methods
    private void Idle()
    {
        MineGridChunk selectedChunk = null;//MAKE MINER MINE NEAREST CHUNK
        if (currentCarryAmount >= maxCarryCapacity)
        {
            _currentState = MinerStates.Return;
            return;
        }
        else if (_gridDictionary.Count > 0 || selectedChunk.GetFirstAvailableOreInMine() != null)//if ore in mine
        {
            _currentState = MinerStates.Mining;//go mine
            return;
        }
        //IDLE LOGIC HERE
    }

    private bool MoveToOre(GameObject targetOre, OreBehaviour oreBehaviour)
    {
        Debug.Log("Moving to ore");
        bool pathFound = aiAgent.SetDestination(targetOre.transform.position);
        if (!pathFound)
        {
            Debug.LogWarning("No path found for available ore");
            return false;
        }
        _headingToTarget = true;
        return true;
    }

    private IEnumerator MineOre(OreBehaviour ore)
    {
        Debug.Log("Started mining ore");
        _miningOre = true;
        _headingToTarget = false;

        while (_miningOre)
        {
            yield return new WaitForSeconds(minerTool.attackSpeed);
            ore.TakeDamage(minerTool.damage);

            if (ore.oreData.health <= 0)
            {
                _miningOre = false;
                break;
            }
        }
    }

    private MineGridChunk GetNearestChunk(Vector3 position, MineGridGenerator mineGrid)
    {
        if (mineGrid.gridDictionary.Count == 0)
        {
            Debug.LogWarning("No chunks in mine grid");
            return null;
        }

        MineGridChunk nearestChunk = mineGrid.gridDictionary[0].GetComponent<MineGridChunk>();//old chunk
        float distanceToLastNearest = 0;//old distance

        for (int i = 1; i < mineGrid.gridDictionary.Count; i++)//i = 1 since we already got the first chunk
        {
            MineGridChunk chunk = mineGrid.gridDictionary[i].GetComponent<MineGridChunk>();//new chunk
            
            distanceToLastNearest = Vector3.Distance(position, nearestChunk.chunkPosition);//update old distance
            float distanceToNewChunk = Vector3.Distance(position, chunk.chunkPosition);//new distance
            if (distanceToNewChunk < distanceToLastNearest)//if new distance closer
            {
                nearestChunk = chunk;//closest chunk is new chunk
            }
        }
        return nearestChunk;
    }    

    private void Mine()
    {
        float distanceFromOre = 1000000f;
        MineGridChunk selectedChunk = null;

        if (_targetChunk == null)
        {
            selectedChunk = GetNearestChunk(transform.position, _mineGrid);
            Debug.Log("Assigned new chunk to miner");
            return;
        }
        //MINER LOGIC
        if (_targetOre == null)
        {
            _headingToTarget = false;
            _miningOre = false;
            _targetOreBehaviour = null;//reset ore behaviour
            _targetOre = selectedChunk.GetFirstAvailableOreInMine();//find new target ore

            if (_targetOre != null)
            {
                _targetOreBehaviour = _targetOre.GetComponent<OreBehaviour>();//get ore behaviour from target ore
                Debug.Log("Found ore behaviour");
            }

            distanceFromOre = Vector3.Distance(_aiModelTransform.position - Vector3.up * (_aiModelTransform.position.y / 2), _targetOre.transform.position);
        }

        if (_targetOre != null)
        {
            distanceFromOre = Vector3.Distance(_aiModelTransform.position - Vector3.up * (_aiModelTransform.position.y / 2), _targetOre.transform.position);
            Debug.Log($"Distance from ore: {distanceFromOre}");
        }
        Debug.Log($"Mining ore: {_miningOre}, Heading to target: {_headingToTarget}");

        if (!_miningOre && !_headingToTarget && _targetOre != null)//if not already moving to ore
        {
            if (!MoveToOre(_targetOre, _targetOreBehaviour))
            {
                _currentState = MinerStates.Idle;//if can't move to ore, go idle
                _targetOre = null;
                _headingToTarget = false;
            }

            _targetOreBehaviour.minerAIminingOre = this;
        }

        if (_targetOreBehaviour == null)
        {
            Debug.LogWarning("No ore behaviour found in ore"); //REMOVE AFTER DEBUGGING
        }

        if (minerTool.range >= distanceFromOre && _targetOreBehaviour != null && !_miningOre)//if ai is in range to mine ore
        {
            StartCoroutine(MineOre(_targetOreBehaviour));//start mining ore until broken
            return;
        }

        //STATE CHECK
        if (_gridDictionary.Count == 0 || _targetOre == null)//if no ores in mine left
        {
            if (_targetOreBehaviour != null)
            {
                _targetOreBehaviour.minerAIminingOre = null;
            }

            _targetOre = null;

            if (currentCarryAmount > 0)//if carrying ore
            {
                _targetOreBehaviour.minerAIminingOre = null;
                _currentState = MinerStates.Return;//return to store
                return;
            }
            else//if not carrying ore
            {
                _targetOreBehaviour.minerAIminingOre = null;
                _currentState = MinerStates.Idle;
                return;
            }
        }
    }

    private void Return()
    {
        if (currentCarryAmount == 0)//if returned all ore
        {
            if (_gridDictionary.Count > 0)//if ore is left in mine
            {
                _currentState = MinerStates.Mining;//go mine
                return;
            }
            else//if no ore left in mine
            {
                _currentState = MinerStates.Idle;
                return;
            }
        }
        //RETURN LOGIC HERE
    }
    #endregion

    private void Update()
    {
        switch(_currentState)
        {
            case MinerStates.Idle:
                Idle();
                break;
            case MinerStates.Mining:
                Mine();
                break;
            case MinerStates.Return:
                Return();
                break;
        }

        Debug.Log($"AI is on navmesh: {aiAgent.isOnNavMesh}, Current state: {_currentState}");

        if (!aiAgent.isOnNavMesh)
        {
            NavMeshHit hit; 
            if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas)) 
            { 
                aiAgent.Warp(hit.position); 
            }
        }
    }

    private void Start()
    {
        _gridDictionary = _mineGrid.gridDictionary;
        _aiModelTransform = gameObject.transform;
    }
}