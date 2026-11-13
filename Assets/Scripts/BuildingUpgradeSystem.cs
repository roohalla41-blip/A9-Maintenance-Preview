using UnityEngine;

public class BuildingUpgradeSystem : MonoBehaviour
{
    private BuildingInstance selected;
    private Camera viewCamera;

    private void Start()
    {
        viewCamera = Camera.main;
    }

    private void Update()
    {
        GridBuildingSystem builder = GetComponent<GridBuildingSystem>();
        if (builder != null && builder.IsBuildMode) return;

        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Ended && !IsUpgradeUiPosition(touch.position))
                    SelectAt(touch.position);
            }
        }
        else if (Input.GetMouseButtonDown(0) && !IsUpgradeUiPosition(Input.mousePosition))
        {
            SelectAt(Input.mousePosition);
        }

        if (selected == null) return;
        if (!selected.gameObject.activeInHierarchy) selected = null;
    }

    private bool IsUpgradeUiPosition(Vector2 position)
    {
        float guiY = Screen.height - position.y;
        return position.x < 430f && guiY > Screen.height - 125f;
    }

    private void SelectAt(Vector2 screenPosition)
    {
        if (viewCamera == null) viewCamera = Camera.main;
        if (viewCamera == null) return;

        Ray ray = viewCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 500f))
        {
            selected = hit.collider.GetComponentInParent<BuildingInstance>();
            return;
        }

        selected = null;
    }

    private void OnGUI()
    {
        if (selected == null) return;

        float y = Screen.height - 116f;
        ResourceAmounts cost = selected.GetUpgradeCost();
        string title = selected.DisplayName + " | سطح " + selected.level + "/" + BuildingInstance.MaximumLevel;

        GUI.Box(new Rect(12f, y, 390f, 104f), title);
        if (cost == null)
        {
            GUI.Label(new Rect(22f, y + 28f, 360f, 24f), "این ساختمان به بالاترین سطح رسیده است.");
        }
        else
        {
            GUI.Label(new Rect(22f, y + 27f, 365f, 24f),
                "هزینه ارتقا: " + cost.wood + " چوب، " + cost.stone + " سنگ، " + cost.food + " غذا");
            VillageSimulation village = VillageSimulation.Instance;
            bool canAfford = village != null && village.Inventory != null &&
                village.Inventory.HasEnough(cost);
            GUI.enabled = canAfford;
            if (GUI.Button(new Rect(22f, y + 57f, 170f, 32f), "ارتقای ساختمان"))
            {
                if (selected != null && village != null &&
                    selected.TryUpgrade(village.Inventory))
                {
                    LocalSaveCoordinator coordinator = FindObjectOfType<LocalSaveCoordinator>();
                    if (coordinator != null) coordinator.SaveAll();
                    else if (!BuildingWorldSaveSystem.Save(out string saveError) &&
                             !string.IsNullOrEmpty(saveError))
                        Debug.LogWarning("Upgraded building could not be saved: " + saveError);
                    Debug.Log("ساختمان با موفقیت ارتقا یافت.");
                }
            }
            GUI.enabled = true;
        }

        if (GUI.Button(new Rect(202f, y + 57f, 170f, 32f), "بستن انتخاب"))
            selected = null;
    }
}
