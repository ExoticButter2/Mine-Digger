using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MiningTool", menuName = "Mining/Create new mining tool")]
public class MiningTool : ScriptableObject
{
    public string toolName = "DEFAULT_TOOL";

    public float damage = 5f;
    public float attackSpeed = 1f;
    public float range = 2f;

    public float price = 0f;
    public List<OreRequirement> oreRequirements;

    public GameObject prefab;
}