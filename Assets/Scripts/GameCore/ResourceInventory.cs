using System;
using UnityEngine;

public enum ResourceType
{
    Wood,
    Stone,
    Gold,
    Food
}

[Serializable]
public class ResourceAmounts
{
    private const int MaximumResourceAmount = 999999999;

    public int wood;
    public int stone;
    public int gold;
    public int food;

    public int Get(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Wood: return wood;
            case ResourceType.Stone: return stone;
            case ResourceType.Gold: return gold;
            case ResourceType.Food: return food;
            default: return 0;
        }
    }

    public void Set(ResourceType type, int value)
    {
        value = Mathf.Clamp(value, 0, MaximumResourceAmount);
        switch (type)
        {
            case ResourceType.Wood: wood = value; break;
            case ResourceType.Stone: stone = value; break;
            case ResourceType.Gold: gold = value; break;
            case ResourceType.Food: food = value; break;
        }
    }

    public void Add(ResourceType type, int amount)
    {
        long total = (long)Get(type) + amount;
        Set(type, (int)Math.Min(MaximumResourceAmount, Math.Max(0L, total)));
    }
}

public class ResourceInventory : MonoBehaviour
{
    [Header("Starting resources")]
    [SerializeField] private ResourceAmounts amounts = new ResourceAmounts
    {
        wood = 250,
        stone = 150,
        gold = 100,
        food = 200
    };

    public event Action<ResourceType, int> ResourceChanged;

    public int GetAmount(ResourceType type)
    {
        return amounts.Get(type);
    }

    public bool HasEnough(ResourceAmounts cost)
    {
        if (cost == null) return false;
        return amounts.wood >= cost.wood &&
               amounts.stone >= cost.stone &&
               amounts.gold >= cost.gold &&
               amounts.food >= cost.food;
    }

    public bool TrySpend(ResourceAmounts cost)
    {
        if (!HasEnough(cost)) return false;

        SetAmount(ResourceType.Wood, amounts.wood - cost.wood);
        SetAmount(ResourceType.Stone, amounts.stone - cost.stone);
        SetAmount(ResourceType.Gold, amounts.gold - cost.gold);
        SetAmount(ResourceType.Food, amounts.food - cost.food);
        return true;
    }

    public void Add(ResourceType type, int amount)
    {
        if (amount <= 0) return;
        SetAmount(type, GetAmount(type) + amount);
    }

    public ResourceAmounts CreateSnapshot()
    {
        return new ResourceAmounts
        {
            wood = amounts.wood,
            stone = amounts.stone,
            gold = amounts.gold,
            food = amounts.food
        };
    }

    public void Restore(ResourceAmounts saved)
    {
        if (saved == null) return;
        SetAmount(ResourceType.Wood, saved.wood);
        SetAmount(ResourceType.Stone, saved.stone);
        SetAmount(ResourceType.Gold, saved.gold);
        SetAmount(ResourceType.Food, saved.food);
    }

    private void SetAmount(ResourceType type, int value)
    {
        amounts.Set(type, value);
        ResourceChanged?.Invoke(type, amounts.Get(type));
    }
}
