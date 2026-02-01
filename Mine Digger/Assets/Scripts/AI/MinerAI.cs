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
    private Dictionary<int, List<GameObject>> _gridDictionary;

    private MinerStates _currentState = MinerStates.Idle;

    private bool _miningOre;

    private Transform _aiModelTransform;

    private GameObject targetOre;
    private OreBehaviour targetOreBehaviour;
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
        if (currentCarryAmount >= maxCarryCapacity)
        {
            _currentState = MinerStates.Return;
            return;
        }
        else if (_gridDictionary.Count > 0 || _mineGrid.GetFirstAvailableOreInMine() != null)//if ore in mine
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
            if (ore.oreData.health <= 0)
            {
                _miningOre = false;
                break;
            }

            ore.TakeDamage(minerTool.damage);

            yield return new WaitForSeconds(minerTool.attackSpeed);
        }
    }

    private void Mine()
    {
        float distanceFromOre = 1000000f;

        //MINER LOGIC
        if (targetOre == null)
        {
            targetOreBehaviour = null;//reset ore behaviour
            targetOre = _mineGrid.GetFirstAvailableOreInMine();//find new target ore

            if (targetOre != null)
            {
                targetOreBehaviour = targetOre.GetComponent<OreBehaviour>();//get ore behaviour from target ore
                Debug.Log("Found ore behaviour");
            }

            distanceFromOre = Vector3.Distance(_aiModelTransform.position - Vector3.up * (_aiModelTransform.position.y / 2), targetOre.transform.position);
        }

        if (targetOre != null)
        {
            distanceFromOre = Vector3.Distance(_aiModelTransform.position - Vector3.up * (_aiModelTransform.position.y / 2), targetOre.transform.position);
            Debug.Log($"Distance from ore: {distanceFromOre}");
        }

        if (!_miningOre && !_headingToTarget)//if not already moving to ore
        {
            if (MoveToOre(targetOre, targetOreBehaviour))//move to ore
            {
                targetOreBehaviour.currentlyMinedByAi = true;
                return;
            }

            _currentState = MinerStates.Idle;//if can't move to ore, go idle
            targetOreBehaviour.currentlyMinedByAi = false;
            targetOre = null;
        }

        if (targetOreBehaviour == null)
        {
            Debug.LogWarning("No ore behaviour found in ore"); //REMOVE AFTER DEBUGGING
        }

        if (minerTool.range >= distanceFromOre && targetOreBehaviour != null && !_miningOre)//if ai is in range to mine ore
        {
            StartCoroutine(MineOre(targetOreBehaviour));//start mining ore until broken
            return;
        }

        //STATE CHECK
        if (_gridDictionary.Count == 0 || targetOre == null)//if no ores in mine left
        {
            if (targetOreBehaviour != null)
            {
                targetOreBehaviour.currentlyMinedByAi = false;
                targetOre = null;
            }

            if (currentCarryAmount > 0)//if carrying ore
            {
                _currentState = MinerStates.Return;//return to store
                return;
            }
            else//if not carrying ore
            {
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
    }

    private void Start()
    {
        _gridDictionary = _mineGrid.gridDictionary;
        _aiModelTransform = gameObject.transform;
    }
}