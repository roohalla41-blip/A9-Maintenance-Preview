using NUnit.Framework;
using UnityEngine;

public sealed class ResourceAmountsTests
{
    [Test]
    public void Add_SaturatesInsteadOfOverflowing()
    {
        var amounts = new ResourceAmounts { wood = 999999990 };
        amounts.Add(ResourceType.Wood, 100);
        Assert.AreEqual(999999999, amounts.wood);
    }

    [Test]
    public void Set_ClampsNegativeAndOversizedValues()
    {
        var amounts = new ResourceAmounts();
        amounts.Set(ResourceType.Stone, -10);
        Assert.AreEqual(0, amounts.stone);
        amounts.Set(ResourceType.Gold, int.MaxValue);
        Assert.AreEqual(999999999, amounts.gold);
    }

    [Test]
    public void BuildingCatalog_ContainsTenValidDefinitions()
    {
        Assert.AreEqual(10, BuildingCatalog.Count);
        for (int id = 1; id <= BuildingCatalog.Count; id++)
        {
            BuildingDefinition definition = BuildingCatalog.Get(id);
            Assert.IsNotNull(definition, "Missing building definition " + id);
            Assert.Greater(definition.Footprint.x, 0f);
            Assert.Greater(definition.Footprint.y, 0f);
            Assert.Greater(definition.Footprint.z, 0f);
        }
    }

    [Test]
    public void CylinderDefinitions_UseUnityPrimitiveHeightCorrection()
    {
        for (int id = 1; id <= BuildingCatalog.Count; id++)
        {
            BuildingDefinition definition = BuildingCatalog.Get(id);
            if (definition.Shape == PrimitiveType.Cylinder)
                Assert.AreEqual(definition.Footprint.y * .5f, definition.WorldScale.y, .0001f);
        }
    }

    [Test]
    public void InventorySpend_IsAtomicWhenResourcesAreInsufficient()
    {
        GameObject host = new GameObject("Inventory test");
        try
        {
            ResourceInventory inventory = host.AddComponent<ResourceInventory>();
            var cost = new ResourceAmounts { wood = 1000, stone = 1000 };
            int woodBefore = inventory.GetAmount(ResourceType.Wood);
            int stoneBefore = inventory.GetAmount(ResourceType.Stone);

            Assert.IsFalse(inventory.TrySpend(cost));
            Assert.AreEqual(woodBefore, inventory.GetAmount(ResourceType.Wood));
            Assert.AreEqual(stoneBefore, inventory.GetAmount(ResourceType.Stone));
        }
        finally
        {
            Object.DestroyImmediate(host);
        }
    }
}