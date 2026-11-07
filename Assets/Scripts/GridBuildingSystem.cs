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
        if (Input.GetKeyDown(KeyCode.Alpha4)) selectedBuilding = 4;
        if (Input.GetKeyDown(KeyCode.Alpha5)) selectedBuilding = 5;
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
        Vector3 size = GetSize();
        Collider[] hits = Physics.OverlapBox(point + Vector3.up * .55f, new Vector3(size.x * .45f, .5f, size.z * .45f));
        foreach (Collider hit in hits)
        {
            if (hit.bounds.size.x > 30f || hit.bounds.size.z > 30f) continue;
            return;
        }
        VillageSimulation economy = VillageSimulation.Instance;
        if (economy == null) return;
        int wood, stone, food;
        GetCost(out wood, out stone, out food);
        if (!economy.TrySpend(wood, stone, food)) return;
        GameObject building = GameObject.CreatePrimitive(selectedBuilding == 4 ? PrimitiveType.Cylinder : PrimitiveType.Cube);
        building.name = GetName();
        building.transform.position = point + Vector3.up * size.y * .5f;
        building.transform.localScale = size;
        building.GetComponent<Renderer>().material.color = GetColor();
        if (selectedBuilding == 4) building.AddComponent<WatchtowerDefense>();
    }
    private Vector3 GetSize()
    {
        switch (selectedBuilding)
        {
            case 1: return new Vector3(2, 1.6f, 2);
            case 2: return new Vector3(3, 1.8f, 3);
            case 3: return new Vector3(4, 2.1f, 3);
            case 4: return new Vector3(2.2f, 4.2f, 2.2f);
            default: return new Vector3(4, 2.2f, 1);
        }
    }
    private void GetCost(out int wood, out int stone, out int food)
    {
        switch (selectedBuilding)
        {
            case 1: wood = 30; stone = 0; food = 10; break;
            case 2: wood = 40; stone = 15; food = 10; break;
            case 3: wood = 50; stone = 30; food = 20; break;
            case 4: wood = 65; stone = 45; food = 15; break;
            default: wood = 25; stone = 35; food = 5; break;
        }
    }
    private string GetName()
    {
        switch (selectedBuilding)
        {
            case 1: return "خانه جدید";
            case 2: return "انبار جدید";
            case 3: return "سربازخانه جدید";
            case 4: return "برج نگهبانی";
            default: return "دیوار دفاعی";
        }
    }
    private Color GetColor()
    {
        switch (selectedBuilding)
        {
            case 1: return new Color(.68f, .48f, .31f);
            case 2: return new Color(.52f, .39f, .25f);
            case 3: return new Color(.46f, .34f, .28f);
            case 4: return new Color(.65f, .59f, .46f);
            default: return new Color(.48f, .45f, .4f);
        }
    }
    private void OnGUI()
    {
        GUI.Box(new Rect(12, 12, 390, 132),
            "افغانستان قدیم | نمونه دفاعی\nWASD حرکت | راست‌کلیک چرخش | چرخ ماوس زوم\nB ساخت | 1 خانه، 2 انبار، 3 سربازخانه\n4 برج نگهبانی، 5 دیوار دفاعی\nحالت ساخت: " + (buildMode ? "روشن" : "خاموش"));
        if (buildMode)
        {
            int w, s, f;
            GetCost(out w, out s, out f);
            GUI.Box(new Rect(12, 370, 330, 38), GetName() + ": " + w + " چوب، " + s + " سنگ، " + f + " غذا");
        }
    }
}