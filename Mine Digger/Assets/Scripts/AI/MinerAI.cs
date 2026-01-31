using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class MinerAI : MonoBehaviour
{
    private Transform _minerAiTransform;

    public NavMeshAgent aiAgent;
    public float maxCarryCapacity = 20f;
    public float currentCarryAmount = 0f;

    public MiningTool minerTool;

    [SerializeField]
    private MineGridGenerator _mineGrid;//mine ai should focus on
    private Dictionary<int, List<GameObject>> _gridDictionary;

    private MinerStates _currentState = MinerStates.Idle;

    private bool _miningOre;

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

    private void MoveToOre(GameObject targetOre, OreBehaviour oreBehaviour)
    {
        Debug.Log("Moving to ore");
        bool pathFound = aiAgent.SetDestination(targetOre.transform.position);

        if (!pathFound)
        {
            Debug.LogWarning("No path found for available ore");
            return;
        }

        oreBehaviour.currentlyMinedByAi = true;
    }

    private IEnumerator MineOre(OreBehaviour ore)
    {
        Debug.Log("Started mining ore");
        _miningOre = true;
        ore.currentlyMinedByAi = true;

        while (_miningOre)
        {
            if (ore.oreData.health <= 0)
            {
                _miningOre = false;
                ore.currentlyMinedByAi = false;
                break;
            }

            ore.TakeDamage(minerTool.damage);

            yield return new WaitForSeconds(minerTool.attackSpeed);
        }
    }

    private void Mine()
    {
        GameObject targetOre = _mineGrid.GetFirstAvailableOreInMine();
        OreBehaviour oreBehaviour = null;

        if (targetOre != null)
        {
            oreBehaviour = targetOre.GetComponent<OreBehaviour>();
        }

        if (_gridDictionary.Count == 0 || targetOre == null)//if no ores in mine left
        {
            if (currentCarryAmount > 0)//if carrying ore
            {
                if (oreBehaviour != null)
                {
                    oreBehaviour.currentlyMinedByAi = false;
                }
                _currentState = MinerStates.Return;//return to store
                return;
            }
            else//if not carrying ore
            {
                if (oreBehaviour != null)
                {
                    oreBehaviour.currentlyMinedByAi = false;
                }
                _currentState = MinerStates.Idle;
                return;
            }
        }

        //MINE LOGIC HERE
        if (targetOre == null)
        {
            Debug.LogWarning("No target ore found for miner ai");
            return;
        }

        if (minerTool.range >= Vector3.Distance(_minerAiTransform.position, targetOre.transform.position) && !_miningOre)//if ai is in range to mine ore
        {
            StartCoroutine(MineOre(oreBehaviour));//start mining ore until broken
            return;
        }

        if (!_miningOre)
        {
            MoveToOre(targetOre, oreBehaviour);
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
        _minerAiTransform = gameObject.transform;
        _gridDictionary = _mineGrid.gridDictionary;
    }
}