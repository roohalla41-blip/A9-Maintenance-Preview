using UnityEngine;

public sealed class BuildingDefinition
{
    public readonly int Id;
    public readonly string DisplayName;
    public readonly Vector3 Footprint;
    public readonly Vector3 WorldScale;
    public readonly ResourceAmounts Cost;
    public readonly Color Color;
    public readonly PrimitiveType Shape;

    public BuildingDefinition(int id, string displayName, Vector3 footprint,
        int wood, int stone, int food, Color color, PrimitiveType shape)
    {
        Id = id;
        DisplayName = displayName;
        Footprint = footprint;
        // Unity's built-in cylinder primitive is two units tall by default.
        WorldScale = new Vector3(footprint.x,
            shape == PrimitiveType.Cylinder ? footprint.y * .5f : footprint.y,
            footprint.z);
        Cost = new ResourceAmounts { wood = wood, stone = stone, food = food };
        Color = color;
        Shape = shape;
    }
}

public static class BuildingCatalog
{
    private static readonly BuildingDefinition[] Definitions =
    {
        new BuildingDefinition(1, "خانه", new Vector3(2f, 1.6f, 2f), 30, 0, 10,
            new Color(.68f, .48f, .31f), PrimitiveType.Cube),
        new BuildingDefinition(2, "انبار", new Vector3(3f, 1.8f, 3f), 40, 15, 10,
            new Color(.52f, .39f, .25f), PrimitiveType.Cube),
        new BuildingDefinition(3, "سربازخانه", new Vector3(4f, 2.1f, 3f), 50, 30, 20,
            new Color(.46f, .34f, .28f), PrimitiveType.Cube),
        new BuildingDefinition(4, "برج نگهبانی", new Vector3(2.2f, 4.2f, 2.2f), 65, 45, 15,
            new Color(.65f, .59f, .46f), PrimitiveType.Cylinder),
        new BuildingDefinition(5, "دیوار دفاعی", new Vector3(4f, 2.2f, 1f), 25, 35, 5,
            new Color(.48f, .45f, .4f), PrimitiveType.Cube),
        new BuildingDefinition(6, "برج توپخانه", new Vector3(2.8f, 4.8f, 2.8f), 110, 95, 25,
            new Color(.58f, .34f, .19f), PrimitiveType.Cylinder),
        new BuildingDefinition(7, "دژ سنگی", new Vector3(5f, 5f, 5f), 180, 240, 70,
            new Color(.43f, .48f, .52f), PrimitiveType.Cube),
        new BuildingDefinition(8, "آکادمی سلطنتی", new Vector3(4.5f, 3.4f, 4f), 210, 180, 120,
            new Color(.32f, .43f, .62f), PrimitiveType.Cube),
        new BuildingDefinition(9, "کارگاه محاصره", new Vector3(5f, 3f, 4f), 260, 190, 150,
            new Color(.42f, .34f, .28f), PrimitiveType.Cube),
        new BuildingDefinition(10, "کاخ بزرگ", new Vector3(7f, 6f, 7f), 420, 380, 260,
            new Color(.72f, .57f, .29f), PrimitiveType.Cube),
        new BuildingDefinition(11, "زرادخانه", new Vector3(4.2f, 2.8f, 3.8f), 150, 125, 75,
            new Color(.38f, .39f, .43f), PrimitiveType.Cube)
    };

    public static int Count { get { return Definitions.Length; } }

    public static BuildingDefinition Get(int id)
    {
        for (int i = 0; i < Definitions.Length; i++)
            if (Definitions[i].Id == id) return Definitions[i];
        return null;
    }
}

public class BuildingInstance : MonoBehaviour
{
    public const int MaximumLevel = 5;

    public int definitionId;
    public int level = 1;
    public bool isPlayerPlaced;

    private Vector3 baseScale;
    private Color baseColor = Color.white;
    private Renderer cachedRenderer;

    public string DisplayName
    {
        get
        {
            BuildingDefinition definition = BuildingCatalog.Get(definitionId);
            return definition != null ? definition.DisplayName : "ساختمان";
        }
    }

    public ResourceAmounts GetUpgradeCost()
    {
        BuildingDefinition definition = BuildingCatalog.Get(definitionId);
        if (definition == null || level >= MaximumLevel) return null;

        // Each next level costs more than the previous one.
        float multiplier = 0.75f + (level * 0.8f);
        return new ResourceAmounts
        {
            wood = Mathf.CeilToInt(definition.Cost.wood * multiplier),
            stone = Mathf.CeilToInt(definition.Cost.stone * multiplier),
            food = Mathf.CeilToInt(definition.Cost.food * multiplier)
        };
    }

    public bool TryUpgrade(ResourceInventory inventory)
    {
        if (inventory == null || level >= MaximumLevel) return false;
        EmpireProgressionSystem progression = EmpireProgressionSystem.Instance;
        int capitalLevel = progression != null ? progression.CapitalLevel : 1;
        if (BuildingProgressionRules.GetBuildingUpgradeRequirement(
            definitionId, level + 1, capitalLevel) != null) return false;
        ResourceAmounts cost = GetUpgradeCost();
        if (cost == null || !inventory.TrySpend(cost)) return false;

        level++;
        ApplyLevelAppearance();
        return true;
    }

    public void Initialize(BuildingDefinition definition)
    {
        if (definition == null) return;
        definitionId = definition.Id;
        level = 1;
        baseScale = transform.localScale;
        cachedRenderer = GetComponent<Renderer>();
        if (cachedRenderer != null) baseColor = cachedRenderer.material.color;
        gameObject.name = definition.DisplayName + " (سطح ۱)";
        ApplyLevelAppearance();
    }

    public void RestoreLevel(int savedLevel)
    {
        level = Mathf.Clamp(savedLevel, 1, MaximumLevel);
        if (baseScale == Vector3.zero) baseScale = transform.localScale;
        if (cachedRenderer == null) cachedRenderer = GetComponent<Renderer>();
        if (cachedRenderer != null && baseColor == Color.white)
            baseColor = cachedRenderer.material.color;
        ApplyLevelAppearance();
    }

    private void ApplyLevelAppearance()
    {
        // The same building remains in place; each tier adds visible fortification details.
        transform.localScale = baseScale * (1f + (level - 1) * 0.075f);
        if (cachedRenderer != null)
            cachedRenderer.material.color = Color.Lerp(baseColor, Color.white, (level - 1) * 0.12f);

        Transform oldDetails = transform.Find("UpgradeVisuals");
        if (oldDetails != null) Destroy(oldDetails.gameObject);
        if (level <= 1) return;

        GameObject details = new GameObject("UpgradeVisuals");
        details.transform.SetParent(transform, false);
        BuildingDefinition definition = BuildingCatalog.Get(definitionId);
        float width = definition != null ? definition.Footprint.x : 3f;
        float height = definition != null ? definition.Footprint.y : 2f;
        float depth = definition != null ? definition.Footprint.z : 3f;
        Color stone = new Color(.48f, .47f, .42f);
        Color dark = new Color(.25f, .2f, .16f);

        if (level >= 2)
        {
            AddDetail(details.transform, PrimitiveType.Cube,
                new Vector3(0f, height + .08f, 0f), new Vector3(width * 1.08f, .22f, depth * 1.08f), stone);
        }
        if (level >= 3)
        {
            for (int x = -1; x <= 1; x += 2)
            for (int z = -1; z <= 1; z += 2)
                AddDetail(details.transform, PrimitiveType.Cylinder,
                    new Vector3(x * width * .42f, height + .42f, z * depth * .42f),
                    new Vector3(.48f, .48f, .48f), stone);
        }
        if (level >= 4)
        {
            AddDetail(details.transform, PrimitiveType.Cylinder,
                new Vector3(0f, height + 1.05f, 0f), new Vector3(.09f, .8f, .09f), dark);
            AddDetail(details.transform, PrimitiveType.Cube,
                new Vector3(.32f, height + 1.35f, 0f), new Vector3(.58f, .34f, .05f),
                new Color(.55f, .16f, .12f));
        }
        if (level >= 5)
        {
            AddDetail(details.transform, PrimitiveType.Cube,
                new Vector3(0f, height + 1.7f, 0f),
                new Vector3(width * .48f, .75f, depth * .48f), new Color(.57f, .49f, .37f));
        }
    }

    private static void AddDetail(Transform parent, PrimitiveType shape,
        Vector3 position, Vector3 scale, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(shape);
        part.name = "ارتقای ظاهری";
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null) renderer.material.color = color;
    }

}
