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
        if (TechnologyResearchPanel.IsOpen) return;
        GridBuildingSystem builder = GetComponent<GridBuildingSystem>();
        if (builder != null && builder.IsBuildMode) return;

        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended && !IsUpgradeUiPosition(touch.position))
                SelectAt(touch.position);
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
        return MobileUiLayout.IsHudPosition(position);
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
        if (TechnologyResearchPanel.IsOpen || selected == null) return;
        Matrix4x4 previousMatrix = MobileUiLayout.Begin();

        float y = (Screen.height / MobileUiLayout.Scale) - 140f;
        ResourceAmounts cost = selected.GetUpgradeCost();
        string title = selected.DisplayName + " | سطح " + selected.level + "/" + BuildingInstance.MaximumLevel;
        EmpireProgressionSystem progression = EmpireProgressionSystem.Instance;
        string requirement = BuildingProgressionRules.GetBuildingUpgradeRequirement(
            selected.definitionId, selected.level + 1,
            progression != null ? progression.CapitalLevel : 1);

        GUI.Box(new Rect(12f, y, 390f, 132f), title);
        if (cost == null)
        {
            GUI.Label(new Rect(22f, y + 28f, 360f, 24f), "این ساختمان به بالاترین سطح رسیده است.");
        }
        else
        {
            GUI.Label(new Rect(22f, y + 23f, 365f, 30f),
                BuildingProgressionRules.GetNextLevelBenefit(selected.definitionId, selected.level + 1));
            GUI.Label(new Rect(22f, y + 52f, 365f, 18f),
                "هزینه: " + cost.wood + " چوب، " + cost.stone + " سنگ، " + cost.food + " غذا");
            VillageSimulation village = VillageSimulation.Instance;
            bool canAfford = village != null && village.Inventory != null &&
                village.Inventory.HasEnough(cost);
            GUI.enabled = canAfford && string.IsNullOrEmpty(requirement);
            if (!string.IsNullOrEmpty(requirement))
                GUI.Label(new Rect(22f, y + 70f, 365f, 18f), requirement);
            if (GUI.Button(new Rect(22f, y + 91f, 170f, 30f), "ارتقای ساختمان"))
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

        if (GUI.Button(new Rect(202f, y + 91f, 170f, 30f), "بستن انتخاب"))
            selected = null;
    
        MobileUiLayout.End(previousMatrix);
    }
}
