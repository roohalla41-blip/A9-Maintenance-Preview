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
        if (Input.GetKeyDown(KeyCode.B))
            buildMode = !buildMode;

        for (int i = 1; i <= BuildingCatalog.Count; i++)
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i - 1)))
                selectedBuilding = i;

        if (!buildMode) return;

        // Touch input is handled separately so a tap is never placed twice.
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Ended && !IsBuildUiPosition(touch.position))
                    PlaceBuilding(touch.position);
            }
        }
        else if (Input.GetMouseButtonDown(0) && !IsBuildUiPosition(Input.mousePosition))
        {
            PlaceBuilding(Input.mousePosition);
        }
    }

    private bool IsBuildUiPosition(Vector2 screenPosition)
    {
        float guiY = Screen.height - screenPosition.y;
        return screenPosition.x < 430f &&
               (guiY < 200f || (guiY > 350f && guiY < 510f));
    }

    private void PlaceBuilding(Vector2 screenPosition)
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        BuildingDefinition definition = BuildingCatalog.Get(selectedBuilding);
        VillageSimulation economy = VillageSimulation.Instance;
        if (definition == null || economy == null || economy.Inventory == null) return;
        if (!economy.Inventory.HasEnough(definition.Cost)) return;

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        if (!ground.Raycast(ray, out float enter)) return;

        Vector3 point = ray.GetPoint(enter);
        point.x = Mathf.Round(point.x / gridSize) * gridSize;
        point.z = Mathf.Round(point.z / gridSize) * gridSize;
        if (new Vector2(point.x, point.z).magnitude > buildRadius) return;

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
        building.transform.localScale = definition.Footprint;
        Renderer renderer = building.GetComponent<Renderer>();
        if (renderer != null) renderer.material.color = definition.Color;

        BuildingInstance instance = building.AddComponent<BuildingInstance>();
        instance.Initialize(definition);

        if (definition.Id == 4)
            building.AddComponent<WatchtowerDefense>();
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(12, 12, 390, 132),
            "افغانستان قدیم | ساخت‌وساز دهکده\n" +
            "رایانه: WASD حرکت، راست‌کلیک چرخش، چرخ ماوس زوم\n" +
            "B حالت ساخت | 1 تا 5 انتخاب ساختمان\n" +
            "حالت ساخت: " + (buildMode ? "روشن" : "خاموش"));

        if (GUI.Button(new Rect(12, 148, 120, 32), buildMode ? "خروج از ساخت" : "ساخت‌وساز"))
            buildMode = !buildMode;

        if (buildMode)
        {
            for (int i = 1; i <= BuildingCatalog.Count; i++)
            {
                BuildingDefinition definition = BuildingCatalog.Get(i);
                float x = 12f + ((i - 1) % 3) * 132f;
                float y = 370f + ((i - 1) / 3) * 38f;
                string label = (selectedBuilding == i ? "✓ " : "") + i + ". " + definition.DisplayName;
                if (GUI.Button(new Rect(x, y, 126, 32), label))
                    selectedBuilding = i;
            }

            BuildingDefinition selected = BuildingCatalog.Get(selectedBuilding);
            if (selected != null)
            {
                GUI.Box(new Rect(12, 446, 390, 38),
                    "هزینه " + selected.DisplayName + ": " +
                    selected.Cost.wood + " چوب، " +
                    selected.Cost.stone + " سنگ، " +
                    selected.Cost.food + " غذا");
            }
        }
    }
}
