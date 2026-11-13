using UnityEngine;

/// <summary>
/// In-game offline research screen backed by the local empire progression save.
/// </summary>
public sealed class TechnologyResearchPanel : MonoBehaviour
{
    public static bool IsOpen { get; private set; }
    private Vector2 scrollPosition;

    private void OnGUI()
    {
        GridBuildingSystem builder = GetComponent<GridBuildingSystem>();
        if (!IsOpen && builder != null && builder.IsBuildMode) return;

        Matrix4x4 previousMatrix = MobileUiLayout.Begin();
        int previousDepth = GUI.depth;
        GUI.depth = -100;

        if (!IsOpen)
        {
            if (GUI.Button(new Rect(12f, 590f, 130f, 32f), "مرکز پژوهش"))
                IsOpen = true;
            GUI.depth = previousDepth;
            MobileUiLayout.End(previousMatrix);
            return;
        }

        EmpireProgressionSystem progression = EmpireProgressionSystem.Instance;
        VillageSimulation village = VillageSimulation.Instance;
        float logicalHeight = Screen.height / MobileUiLayout.Scale;
        GUI.Box(new Rect(8f, 8f, 414f, logicalHeight - 16f),
            "مرکز پژوهش | " + (progression != null ? progression.CurrentAgeName : "دهکده"));

        if (GUI.Button(new Rect(326f, 16f, 86f, 32f), "بستن"))
            IsOpen = false;

        if (progression != null && village != null && village.Inventory != null)
        {
            GUI.Label(new Rect(20f, 52f, 390f, 28f),
                "پایتخت سطح " + progression.CapitalLevel +
                " | تجربه: " + progression.Experience +
                " | طلا: " + village.gold);

            Rect view = new Rect(16f, 86f, 398f, logicalHeight - 108f);
            Rect content = new Rect(0f, 0f, 370f, progression.TechnologyCount * 76f);
            scrollPosition = GUI.BeginScrollView(view, scrollPosition, content);

            for (int i = 0; i < progression.TechnologyCount; i++)
            {
                string id = progression.GetTechnologyIdAt(i);
                float y = i * 76f;
                ResourceAmounts cost = progression.GetTechnologyCost(id);
                bool researched = progression.IsTechnologyResearched(id);
                bool canResearch = progression.CanResearch(id, village.Inventory);

                GUI.Box(new Rect(0f, y, 370f, 70f), string.Empty);
                GUI.Label(new Rect(8f, y + 4f, 225f, 22f),
                    progression.GetTechnologyTitle(id) + (researched ? " ✓" : ""));
                GUI.Label(new Rect(8f, y + 27f, 230f, 36f),
                    progression.GetTechnologyDescription(id) + "\n" +
                    progression.GetTechnologyStatus(id));
                if (cost != null)
                    GUI.Label(new Rect(238f, y + 5f, 124f, 24f),
                        "چوب " + cost.wood + " | سنگ " + cost.stone);
                if (cost != null)
                    GUI.Label(new Rect(238f, y + 27f, 124f, 20f),
                        "طلا " + cost.gold + " | غذا " + cost.food);

                GUI.enabled = canResearch;
                if (GUI.Button(new Rect(246f, y + 48f, 112f, 20f),
                    researched ? "تکمیل شد" : "پژوهش"))
                {
                    if (progression.TryResearch(id, village.Inventory))
                    {
                        LocalSaveCoordinator coordinator = FindObjectOfType<LocalSaveCoordinator>();
                        if (coordinator != null) coordinator.SaveAll();
                    }
                }
                GUI.enabled = true;
            }

            GUI.EndScrollView();
        }
        else
        {
            GUI.Label(new Rect(20f, 70f, 380f, 32f), "سامانه پیشرفت هنوز آماده نیست.");
        }

        GUI.depth = previousDepth;
        MobileUiLayout.End(previousMatrix);
    }

    public static void Close()
    {
        IsOpen = false;
    }
}
