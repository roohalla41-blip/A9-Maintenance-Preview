using UnityEngine;

public class GridBuildingSystem : MonoBehaviour
{
    [Min(.25f)] public float gridSize = 1f;
    [Min(1f)] public float buildRadius = 17f;

    private bool buildMode;
    public bool IsBuildMode { get { return buildMode; } }
    private int selectedBuilding = 1;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (TechnologyResearchPanel.IsOpen) return;
        if (Input.GetKeyDown(KeyCode.B))
            buildMode = !buildMode;

        for (int i = 1; i <= BuildingCatalog.Count; i++)
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i - 1)))
                selectedBuilding = i;

        if (!buildMode) return;

        // Touch input is handled separately so a tap is never placed twice.
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended && !IsBuildUiPosition(touch.position))
                PlaceBuilding(touch.position);
        }
        else if (Input.GetMouseButtonDown(0) && !IsBuildUiPosition(Input.mousePosition))
        {
            PlaceBuilding(Input.mousePosition);
        }
    }

    private bool IsBuildUiPosition(Vector2 screenPosition)
    {
        return MobileUiLayout.IsHudPosition(screenPosition);
    }

    private void PlaceBuilding(Vector2 screenPosition)
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        BuildingDefinition definition = BuildingCatalog.Get(selectedBuilding);
        VillageSimulation economy = VillageSimulation.Instance;
        if (definition == null || economy == null || economy.Inventory == null) return;
        EmpireProgressionSystem progression = EmpireProgressionSystem.Instance;
        if (progression != null && !progression.IsBuildingUnlocked(definition.Id)) return;
        if (!economy.Inventory.HasEnough(definition.Cost)) return;

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        if (!ground.Raycast(ray, out float enter)) return;

        Vector3 point = ray.GetPoint(enter);
        point.x = Mathf.Round(point.x / gridSize) * gridSize;
        point.z = Mathf.Round(point.z / gridSize) * gridSize;
        if (new Vector2(point.x, point.z).magnitude > buildRadius) return;
        point.y = GetTerrainHeight(point.x, point.z);

        Vector3 halfExtents = new Vector3(
            definition.Footprint.x * .46f,
            Mathf.Max(.45f, definition.Footprint.y * .45f),
            definition.Footprint.z * .46f);

        Collider[] hits = Physics.OverlapBox(
            point + Vector3.up * definition.Footprint.y * .5f,
            halfExtents,
            Quaternion.identity);

        foreach (Collider hit in hits)
        {
            if (hit == null) continue;
            // Ignore the large terrain slab; other colliders reserve their space.
            if (hit.bounds.size.x > 30f || hit.bounds.size.z > 30f) continue;
            return;
        }

        // Spend only after every placement validation has passed.
        if (!economy.Inventory.TrySpend(definition.Cost)) return;

        GameObject building = GameObject.CreatePrimitive(definition.Shape);
        building.transform.position = point + Vector3.up * definition.Footprint.y * .5f;
        building.transform.localScale = definition.WorldScale;
        Renderer renderer = building.GetComponent<Renderer>();
        if (renderer != null) renderer.material.color = definition.Color;

        BuildingInstance instance = building.AddComponent<BuildingInstance>();
        instance.Initialize(definition);
        instance.isPlayerPlaced = true;

        if (definition.Id == 4 || definition.Id == 6)
            building.AddComponent<WatchtowerDefense>();

        if (progression != null)
            progression.AwardExperience(20 + definition.Id * 3);

        // Persist the structure, its resource cost and progression as one coordinated save.
        LocalSaveCoordinator coordinator = FindObjectOfType<LocalSaveCoordinator>();
        if (coordinator != null)
            coordinator.SaveAll();
        else
        {
            GameSaveSystem.Save(economy.Inventory);
            if (!BuildingWorldSaveSystem.Save(out string saveError) && !string.IsNullOrEmpty(saveError))
                Debug.LogWarning("Placed building could not be saved: " + saveError);
        }
    }

    private static float GetTerrainHeight(float x, float z)
    {
        float hills = Mathf.Sin(x * .19f) * .22f + Mathf.Cos(z * .16f) * .18f;
        float ridge = Mathf.PerlinNoise((x + 80f) * .075f, (z + 80f) * .075f) * .8f;
        return hills + ridge - .25f;
    }

    private void OnGUI()
    {
        if (TechnologyResearchPanel.IsOpen) return;
        Matrix4x4 previousMatrix = MobileUiLayout.Begin();
        GUI.Box(new Rect(12, 12, 390, 132),
            "افغانستان قدیم | ساخت‌وساز دهکده\n" +
            "لمس یک انگشت: چرخش دوربین | دو انگشت: حرکت و زوم\n" +
            "رایانه: WASD و ماوس | انتخاب از فهرست ۱۰ ساختمان\n" +
            "حالت ساخت: " + (buildMode ? "روشن" : "خاموش"));

        if (GUI.Button(new Rect(12, 148, 120, 32), buildMode ? "خروج از ساخت" : "ساخت‌وساز"))
            buildMode = !buildMode;

        EmpireProgressionSystem progression = EmpireProgressionSystem.Instance;
        VillageSimulation village = VillageSimulation.Instance;
        if (progression != null && village != null && village.Inventory != null)
        {
            int requiredXp = progression.ExperienceToNextLevel;
            string capitalLabel = progression.CapitalLevel >= EmpireProgressionSystem.MaximumCapitalLevel
                ? "پایتخت: حداکثر سطح"
                : "ارتقای پایتخت " + progression.CapitalLevel + " | XP " +
                  progression.Experience + "/" + requiredXp;
            GUI.enabled = progression.CanUpgradeCapital(village.Inventory);
            if (GUI.Button(new Rect(142, 148, 248, 32), capitalLabel))
            {
                if (progression.TryUpgradeCapital(village.Inventory))
                {
                    LocalSaveCoordinator coordinator = FindObjectOfType<LocalSaveCoordinator>();
                    if (coordinator != null) coordinator.SaveAll();
                    else GameSaveSystem.Save(village.Inventory);
                }
            }
            GUI.enabled = true;
        }

        if (buildMode)
        {
            for (int i = 1; i <= BuildingCatalog.Count; i++)
            {
                BuildingDefinition definition = BuildingCatalog.Get(i);
                float x = 12f + ((i - 1) % 3) * 132f;
                float y = 388f + ((i - 1) / 3) * 38f;
                bool unlocked = progression == null || progression.IsBuildingUnlocked(i);
                string label = (selectedBuilding == i ? "✓ " : "") + i + ". " +
                    definition.DisplayName + (unlocked ? "" : " | قفل");
                GUI.enabled = unlocked;
                if (GUI.Button(new Rect(x, y, 126, 32), label))
                    selectedBuilding = i;
                GUI.enabled = true;
            }

            BuildingDefinition selected = BuildingCatalog.Get(selectedBuilding);
            if (selected != null)
            {
                GUI.Box(new Rect(12, 540, 390, 38),
                    "هزینه " + selected.DisplayName + ": " +
                    selected.Cost.wood + " چوب، " +
                    selected.Cost.stone + " سنگ، " +
                    selected.Cost.food + " غذا");
            }
        }
    
        MobileUiLayout.End(previousMatrix);
    }
}
