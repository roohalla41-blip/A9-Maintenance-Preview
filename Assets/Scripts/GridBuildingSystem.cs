using UnityEngine;

public class GridBuildingSystem : MonoBehaviour
{
    public float gridSize = 1f;
    public float buildRadius = 17f;
    private bool buildMode;
    private int selectedBuilding = 1;
    private Camera mainCamera;

    private void Start() { mainCamera = Camera.main; }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)) buildMode = !buildMode;
        if (Input.GetKeyDown(KeyCode.Alpha1)) selectedBuilding = 1;
        if (Input.GetKeyDown(KeyCode.Alpha2)) selectedBuilding = 2;
        if (Input.GetKeyDown(KeyCode.Alpha3)) selectedBuilding = 3;
        if (buildMode && Input.GetMouseButtonDown(0)) PlaceBuilding();
    }

    private void PlaceBuilding()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        if (!ground.Raycast(ray, out float enter)) return;
        Vector3 point = ray.GetPoint(enter);
        point.x = Mathf.Round(point.x / gridSize) * gridSize;
        point.z = Mathf.Round(point.z / gridSize) * gridSize;
        if (new Vector2(point.x, point.z).magnitude > buildRadius) return;

        Vector3 size = selectedBuilding == 1 ? new Vector3(2, 1.6f, 2) :
                       selectedBuilding == 2 ? new Vector3(3, 1.8f, 3) :
                                                new Vector3(4, 2.1f, 3);
        Color color = selectedBuilding == 1 ? new Color(0.68f, 0.48f, 0.31f) :
                      selectedBuilding == 2 ? new Color(0.52f, 0.39f, 0.25f) :
                                               new Color(0.46f, 0.34f, 0.28f);
        GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
        building.name = selectedBuilding == 1 ? "خانه جدید" :
                        selectedBuilding == 2 ? "انبار جدید" : "سربازخانه جدید";
        building.transform.position = point + Vector3.up * size.y * 0.5f;
        building.transform.localScale = size;
        building.GetComponent<Renderer>().material.color = color;
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(12, 12, 330, 105),
            "افغانستان قدیم | نمونه 0.1\nWASD: حرکت | راست‌کلیک و کشیدن: چرخش\nچرخ ماوس: زوم | B: ساخت | 1 خانه، 2 انبار، 3 سربازخانه\nحالت ساخت: " + (buildMode ? "روشن" : "خاموش"));
    }
}