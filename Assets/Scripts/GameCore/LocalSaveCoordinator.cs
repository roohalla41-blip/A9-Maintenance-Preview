using UnityEngine;

/// <summary>
/// Coordinates local-only persistence at mobile lifecycle boundaries.
/// Resource state and empire progression remain on this device; no network is used.
/// </summary>
[DefaultExecutionOrder(100)]
public sealed class LocalSaveCoordinator : MonoBehaviour
{
    [SerializeField, Min(5f)] private float autosaveIntervalSeconds = 30f;

    private ResourceInventory inventory;
    private EmpireProgressionSystem progression;
    private float autosaveTimer;
    private bool initialized;

    private void Start()
    {
        // VillageSimulation owns the initial resource restore. This coordinator
        // starts after scene initialization and only handles subsequent saves.
        ResolveSystems();
        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;
        autosaveTimer += Time.unscaledDeltaTime;
        if (autosaveTimer < autosaveIntervalSeconds) return;
        autosaveTimer = 0f;
        SaveAll();
    }

    private void ResolveSystems()
    {
        VillageSimulation village = VillageSimulation.Instance;
        if (village != null) inventory = village.Inventory;
        if (inventory == null) inventory = FindObjectOfType<ResourceInventory>();
        progression = EmpireProgressionSystem.Instance;
        if (progression == null) progression = FindObjectOfType<EmpireProgressionSystem>();
    }

    public void SaveAll()
    {
        if (inventory == null || progression == null)
            ResolveSystems();

        if (inventory != null && !GameSaveSystem.Save(inventory))
            Debug.LogWarning("Local resource save did not complete.");
        if (progression != null)
            progression.Save();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused && initialized) SaveAll();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && initialized) SaveAll();
    }

    private void OnApplicationQuit()
    {
        if (initialized) SaveAll();
    }
}
