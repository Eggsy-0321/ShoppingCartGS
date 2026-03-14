using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns one item at each distance threshold.
/// Designed for the fixed-player world-scroll setup.
/// </summary>
public class ItemSpawnManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SegmentLoopScroller segmentLoopScroller;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform spawnRoot;

    [Header("Spawn Settings")]
    [SerializeField] private List<ItemDefinition> itemDefinitions = new List<ItemDefinition>();
    [SerializeField] private float spawnIntervalDistance = 10f;
    [SerializeField] private float spawnForwardOffset = 10f;
    [SerializeField] private float spawnY = 0.1f;
    [SerializeField] private float[] lanePositionsX = new float[] { -1.4f, 0f, 1.4f };

    [Header("Debug")]
    [SerializeField] private bool logSpawnEvents = false;

    private float _nextSpawnDistance = 10f;
    private float _lastDistance;

    private void Awake()
    {
        ValidateReferences();
        ValidateLaneSettings();
        ResetSpawnDistance();
    }

    private void OnValidate()
    {
        if (spawnIntervalDistance <= 0f)
        {
            spawnIntervalDistance = 10f;
        }

        if (spawnForwardOffset < 0f)
        {
            spawnForwardOffset = 0f;
        }

        if (spawnY < 0f)
        {
            spawnY = 0f;
        }

        if (lanePositionsX == null || lanePositionsX.Length != 3)
        {
            lanePositionsX = new float[] { -1.4f, 0f, 1.4f };
        }
    }

    private void Update()
    {
        if (segmentLoopScroller == null || spawnRoot == null)
        {
            return;
        }

        float currentDistance = segmentLoopScroller.Distance;

        if (currentDistance < _lastDistance)
        {
            ResetSpawnDistance();
        }

        // Catch up if distance advanced across multiple thresholds in one frame.
        while (currentDistance >= _nextSpawnDistance)
        {
            TrySpawnItem();
            _nextSpawnDistance += spawnIntervalDistance;
        }

        _lastDistance = currentDistance;
    }

    /// <summary>
    /// Resets the next spawn threshold to the first interval.
    /// </summary>
    public void ResetSpawnDistance()
    {
        _nextSpawnDistance = Mathf.Max(0.01f, spawnIntervalDistance);
        _lastDistance = 0f;
    }

    private void TrySpawnItem()
    {
        ItemDefinition selectedDefinition = SelectItemDefinition();
        if (selectedDefinition == null)
        {
            return;
        }

        if (selectedDefinition.prefab == null)
        {
            Debug.LogWarning($"[ItemSpawnManager] Prefab is missing for item '{selectedDefinition.itemName}'.");
            return;
        }

        Vector3 worldSpawnPosition = GetWorldSpawnPosition();
        GameObject spawnedItem = Instantiate(selectedDefinition.prefab, worldSpawnPosition, Quaternion.identity);
        Transform spawnedTransform = spawnedItem.transform;
        spawnedTransform.SetParent(spawnRoot, true);

        // Each spawned item moves itself using the same speed as the world scroll.
        ItemMover itemMover = spawnedItem.GetComponent<ItemMover>();
        if (itemMover == null)
        {
            itemMover = spawnedItem.AddComponent<ItemMover>();
        }

        if (segmentLoopScroller == null || playerTransform == null)
        {
            Debug.LogWarning("[ItemSpawnManager] ItemMover initialization skipped because references are missing.", spawnedItem);
        }

        itemMover.Initialize(segmentLoopScroller, playerTransform);

        // Ensure pickup logic is present even if the prefab does not carry it yet.
        ItemPickup itemPickup = spawnedItem.GetComponent<ItemPickup>();
        if (itemPickup == null)
        {
            itemPickup = spawnedItem.AddComponent<ItemPickup>();
        }

        if (selectedDefinition == null)
        {
            Debug.LogWarning("[ItemSpawnManager] ItemPickup initialization skipped because ItemDefinition is missing.", spawnedItem);
        }

        itemPickup.Initialize(selectedDefinition);

        if (logSpawnEvents)
        {
            Debug.Log(
                $"[ItemSpawnManager] Spawned '{selectedDefinition.itemName}' at distance {_nextSpawnDistance:0.0}m " +
                $"(worldX: {worldSpawnPosition.x:0.0}, worldY: {worldSpawnPosition.y:0.00}, worldZ: {worldSpawnPosition.z:0.0}).",
                spawnedItem);
        }
    }

    private Vector3 GetWorldSpawnPosition()
    {
        float laneX = lanePositionsX[Random.Range(0, lanePositionsX.Length)];
        float spawnZ = playerTransform != null
            ? playerTransform.position.z + spawnForwardOffset
            : spawnForwardOffset;

        return new Vector3(laneX, spawnY, spawnZ);
    }

    private ItemDefinition SelectItemDefinition()
    {
        int totalWeight = 0;

        for (int i = 0; i < itemDefinitions.Count; i++)
        {
            ItemDefinition definition = itemDefinitions[i];
            if (definition == null || definition.spawnWeight <= 0)
            {
                continue;
            }

            totalWeight += definition.spawnWeight;
        }

        if (totalWeight <= 0)
        {
            Debug.LogWarning("[ItemSpawnManager] No spawnable item definitions were found.");
            return null;
        }

        int roll = Random.Range(0, totalWeight);
        int cumulativeWeight = 0;

        for (int i = 0; i < itemDefinitions.Count; i++)
        {
            ItemDefinition definition = itemDefinitions[i];
            if (definition == null || definition.spawnWeight <= 0)
            {
                continue;
            }

            cumulativeWeight += definition.spawnWeight;
            if (roll < cumulativeWeight)
            {
                return definition;
            }
        }

        Debug.LogWarning("[ItemSpawnManager] Weighted selection failed unexpectedly.");
        return null;
    }

    private void ValidateReferences()
    {
        if (segmentLoopScroller == null)
        {
            Debug.LogError("[ItemSpawnManager] SegmentLoopScroller is not assigned.");
        }

        if (spawnRoot == null)
        {
            Debug.LogError("[ItemSpawnManager] SpawnRoot is not assigned.");
        }

        if (playerTransform == null)
        {
            Debug.LogError("[ItemSpawnManager] Player Transform is not assigned.");
        }

        if (segmentLoopScroller != null && playerTransform == null)
        {
            Debug.LogWarning("[ItemSpawnManager] Spawned items will not move until Player Transform is assigned.");
        }

        if (itemDefinitions == null || itemDefinitions.Count == 0)
        {
            Debug.LogWarning("[ItemSpawnManager] ItemDefinitions list is empty.");
        }
    }

    private void ValidateLaneSettings()
    {
        if (lanePositionsX == null || lanePositionsX.Length != 3)
        {
            Debug.LogWarning("[ItemSpawnManager] Lane settings were invalid. Reverting to default 3-lane positions.");
            lanePositionsX = new float[] { -1.4f, 0f, 1.4f };
        }
    }
}
