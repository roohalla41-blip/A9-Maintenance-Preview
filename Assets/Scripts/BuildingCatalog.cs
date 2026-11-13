using UnityEngine;

public sealed class BuildingDefinition
{
    public readonly int Id;
    public readonly string DisplayName;
    public readonly Vector3 Footprint;
    public readonly ResourceAmounts Cost;
    public readonly Color Color;
    public readonly PrimitiveType Shape;

    public BuildingDefinition(int id, string displayName, Vector3 footprint,
        int wood, int stone, int food, Color color, PrimitiveType shape)
    {
        Id = id;
        DisplayName = displayName;
        Footprint = footprint;
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
            new Color(.48f, .45f, .4f), PrimitiveType.Cube)
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
    public int definitionId;
    public int level = 1;

    public void Initialize(BuildingDefinition definition)
    {
        if (definition == null) return;
        definitionId = definition.Id;
        level = 1;
        gameObject.name = definition.DisplayName + " (سطح ۱)";
    }
}
