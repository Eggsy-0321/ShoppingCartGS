using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns one item at each distance threshold.
/// Designed for the fixed-player world-scroll setup.
/// </summary>
public class ItemSpawnManager : MonoBehaviour
{
    private const string LightCategoryName = "Light";
    private const string MediumCategoryName = "Medium";
    private const string HeavyCategoryName = "Heavy";

    [Header("References")]
    [SerializeField] private SegmentLoopScroller segmentLoopScroller;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform spawnRoot;

    [Header("Spawn Settings")]
    [SerializeField] private List<ItemDefinition> itemDefinitions = new List<ItemDefinition>();
    [SerializeField] private ItemDefinition lightItemDefinition;
    [SerializeField] private ItemDefinition mediumItemDefinition;
    [SerializeField] private ItemDefinition heavyItemDefinition;
    [SerializeField] private GameObject[] lightItemPrefabs;
    [SerializeField] private GameObject[] mediumItemPrefabs;
    [SerializeField] private GameObject[] heavyItemPrefabs;
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
        ResolveCategoryDefinitionReferences();
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

        ResolveCategoryDefinitionReferences();
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

    /// <summary>
    /// Clears all currently spawned items and resets spawn progress for a new run.
    /// </summary>
    public void ResetForNewGame()
    {
        ClearSpawnedItems();
        ResetSpawnDistance();
    }

    /// <summary>
    /// Removes spawned item instances under the spawn root.
    /// </summary>
    public void ClearSpawnedItems()
    {
        if (spawnRoot == null)
        {
            return;
        }

        for (int i = spawnRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(spawnRoot.GetChild(i).gameObject);
        }
    }

    private void TrySpawnItem()
    {
        ItemDefinition selectedDefinition = SelectItemDefinition();
        if (selectedDefinition == null)
        {
            return;
        }

        GameObject selectedPrefab = SelectSpawnPrefab(selectedDefinition);
        if (selectedPrefab == null)
        {
            Debug.LogError(
                $"[ItemSpawnManager] Spawn skipped because no valid prefab could be resolved for '{selectedDefinition.itemName}'.");
            return;
        }

        Vector3 worldSpawnPosition = GetWorldSpawnPosition();
        GameObject spawnedItem = Instantiate(
            selectedPrefab,
            worldSpawnPosition,
            selectedPrefab.transform.rotation,
            spawnRoot);

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

    private GameObject SelectSpawnPrefab(ItemDefinition selectedDefinition)
    {
        GameObject[] categoryPrefabs = null;
        string categoryName = string.Empty;

        if (selectedDefinition == lightItemDefinition)
        {
            categoryPrefabs = lightItemPrefabs;
            categoryName = LightCategoryName;
        }
        else if (selectedDefinition == mediumItemDefinition)
        {
            categoryPrefabs = mediumItemPrefabs;
            categoryName = MediumCategoryName;
        }
        else if (selectedDefinition == heavyItemDefinition)
        {
            categoryPrefabs = heavyItemPrefabs;
            categoryName = HeavyCategoryName;
        }
        else
        {
            Debug.LogWarning(
                $"[ItemSpawnManager] ItemDefinition '{selectedDefinition.itemName}' is not mapped to a category prefab array. Falling back to the prefab on the definition.");
            return selectedDefinition.prefab;
        }

        if (TryGetRandomPrefab(categoryPrefabs, categoryName, out GameObject selectedPrefab))
        {
            return selectedPrefab;
        }

        if (selectedDefinition.prefab != null)
        {
            Debug.LogWarning(
                $"[ItemSpawnManager] {categoryName} prefab array is not usable. Falling back to the prefab assigned on '{selectedDefinition.itemName}'.");
            return selectedDefinition.prefab;
        }

        Debug.LogError(
            $"[ItemSpawnManager] {categoryName} prefab array is not usable and fallback prefab is missing on '{selectedDefinition.itemName}'.");
        return null;
    }

    private void ResolveCategoryDefinitionReferences()
    {
        if (itemDefinitions == null)
        {
            return;
        }

        ResolveCategoryDefinitionReference(lightItemDefinition);
        ResolveCategoryDefinitionReference(mediumItemDefinition);
        ResolveCategoryDefinitionReference(heavyItemDefinition);
    }

    private void ResolveCategoryDefinitionReference(ItemDefinition categoryDefinition)
    {
        if (categoryDefinition == null)
        {
            return;
        }

        for (int i = 0; i < itemDefinitions.Count; i++)
        {
            if (ReferenceEquals(itemDefinitions[i], categoryDefinition))
            {
                return;
            }
        }

        for (int i = 0; i < itemDefinitions.Count; i++)
        {
            ItemDefinition listedDefinition = itemDefinitions[i];
            if (!HasSameRepresentativePrefab(listedDefinition, categoryDefinition))
            {
                continue;
            }

            itemDefinitions[i] = categoryDefinition;
            return;
        }
    }

    private bool HasSameRepresentativePrefab(ItemDefinition firstDefinition, ItemDefinition secondDefinition)
    {
        return firstDefinition != null
            && secondDefinition != null
            && firstDefinition.prefab != null
            && firstDefinition.prefab == secondDefinition.prefab;
    }

    private bool TryGetRandomPrefab(GameObject[] prefabCandidates, string categoryName, out GameObject selectedPrefab)
    {
        selectedPrefab = null;

        if (prefabCandidates == null || prefabCandidates.Length == 0)
        {
            Debug.LogWarning($"[ItemSpawnManager] {categoryName} prefab array is null or empty.");
            return false;
        }

        int validPrefabCount = 0;
        for (int i = 0; i < prefabCandidates.Length; i++)
        {
            if (prefabCandidates[i] != null)
            {
                validPrefabCount++;
            }
        }

        if (validPrefabCount == 0)
        {
            Debug.LogWarning($"[ItemSpawnManager] {categoryName} prefab array does not contain any valid prefab references.");
            return false;
        }

        int targetValidIndex = Random.Range(0, validPrefabCount);
        int currentValidIndex = 0;

        for (int i = 0; i < prefabCandidates.Length; i++)
        {
            GameObject prefabCandidate = prefabCandidates[i];
            if (prefabCandidate == null)
            {
                continue;
            }

            if (currentValidIndex == targetValidIndex)
            {
                selectedPrefab = prefabCandidate;
                return true;
            }

            currentValidIndex++;
        }

        Debug.LogError($"[ItemSpawnManager] Failed to select a valid prefab from the {categoryName} prefab array.");
        return false;
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

        ValidateCategoryDefinition(lightItemDefinition, LightCategoryName);
        ValidateCategoryDefinition(mediumItemDefinition, MediumCategoryName);
        ValidateCategoryDefinition(heavyItemDefinition, HeavyCategoryName);
    }

    private void ValidateLaneSettings()
    {
        if (lanePositionsX == null || lanePositionsX.Length != 3)
        {
            Debug.LogWarning("[ItemSpawnManager] Lane settings were invalid. Reverting to default 3-lane positions.");
            lanePositionsX = new float[] { -1.4f, 0f, 1.4f };
        }
    }

    private void ValidateCategoryDefinition(ItemDefinition definition, string categoryName)
    {
        if (definition == null)
        {
            Debug.LogWarning($"[ItemSpawnManager] {categoryName} ItemDefinition is not assigned. Category prefab array fallback will be unavailable.");
            return;
        }

        if (itemDefinitions != null && !itemDefinitions.Contains(definition))
        {
            Debug.LogWarning($"[ItemSpawnManager] {categoryName} ItemDefinition is not included in ItemDefinitions.");
        }
    }
}
