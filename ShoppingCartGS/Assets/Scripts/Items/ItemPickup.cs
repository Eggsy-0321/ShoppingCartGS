using UnityEngine;

/// <summary>
/// Handles trigger-based pickup for spawned items.
/// Logs item data now and keeps a simple entry point for future score/weight handling.
/// </summary>
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemDefinition itemDefinition;
    [SerializeField] private WeightManager weightManager;
    private bool _isCollected;

    /// <summary>
    /// Injects the item definition used for pickup logging and future gameplay hooks.
    /// </summary>
    public void Initialize(ItemDefinition itemDefinition)
    {
        this.itemDefinition = itemDefinition;
        ResolveWeightManager();
    }

    private void Awake()
    {
        ResolveWeightManager();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Unity trigger events require a Collider pair and at least one Rigidbody
        // on either the Player or the Item side.
        if (_isCollected)
        {
            return;
        }

        if (other == null || !other.CompareTag("Player"))
        {
            return;
        }

        if (itemDefinition == null)
        {
            Debug.LogWarning("[ItemPickup] ItemDefinition is missing during pickup.", this);
            return;
        }

        ResolveWeightManager();
        if (weightManager == null)
        {
            Debug.LogWarning("[ItemPickup] WeightManager is missing during pickup.", this);
            return;
        }

        _isCollected = true;
        weightManager.AddWeight(itemDefinition.weight);

        // Keep the pickup output simple for now so later systems can hook in here.
        Debug.Log(
            $"[ItemPickup] Collected itemName={itemDefinition.itemName}, weight={itemDefinition.weight}, point={itemDefinition.point}",
            this);

        Destroy(gameObject);
    }

    private void ResolveWeightManager()
    {
        if (weightManager == null)
        {
            weightManager = FindFirstObjectByType<WeightManager>();
        }
    }
}
