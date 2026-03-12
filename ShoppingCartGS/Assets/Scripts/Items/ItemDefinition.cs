using System;
using UnityEngine;

[Serializable]
public class ItemDefinition
{
    [Header("Basic")]
    public string itemName;
    public int weight;
    public int point;

    [Header("Spawn")]
    [Min(0)]
    public int spawnWeight = 1;
    public GameObject prefab;
}
