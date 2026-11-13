using UnityEngine;

/// <summary>
/// Building-level gates and unlock tiers for the offline Afghan empire progression.
/// All checks use the actual placed buildings in the current village.
/// </summary>
public static class BuildingProgressionRules
{
    public static int CountBuildings(int definitionId, int minimumLevel = 1)
    {
        BuildingInstance[] all = Object.FindObjectsOfType<BuildingInstance>();
        int count = 0;
        foreach (BuildingInstance item in all)
            if (item != null && item.definitionId == definitionId && item.level >= minimumLevel)
                count++;
        return count;
    }

    public static bool HasBuilding(int definitionId, int minimumLevel = 1)
    {
        return CountBuildings(definitionId, minimumLevel) > 0;
    }

    public static string GetCapitalRequirement(int nextCapitalLevel)
    {
        switch (nextCapitalLevel)
        {
            case 3: return HasBuilding(3, 2) ? null : "برای ارتقای تالار به سطح ۳، سربازخانه باید سطح ۲ باشد.";
            case 6:
                if (!HasBuilding(4, 2)) return "برای تالار سطح ۶، برج نگهبانی باید سطح ۲ باشد.";
                if (!HasBuilding(11, 2)) return "برای تالار سطح ۶، زرادخانه باید سطح ۲ باشد.";
                return null;
            case 11:
                if (!HasBuilding(6, 2)) return "برای تالار سطح ۱۱، برج توپخانه باید سطح ۲ باشد.";
                if (!HasBuilding(8, 2)) return "برای تالار سطح ۱۱، آکادمی باید سطح ۲ باشد.";
                return null;
            case 16:
                if (!HasBuilding(7, 2)) return "برای تالار سطح ۱۶، دژ سنگی باید سطح ۲ باشد.";
                if (!HasBuilding(11, 3)) return "برای تالار سطح ۱۶، زرادخانه باید سطح ۳ باشد.";
                return null;
            case 21:
                if (!HasBuilding(9, 2)) return "برای تالار سطح ۲۱، کارگاه محاصره باید سطح ۲ باشد.";
                if (!HasBuilding(6, 3)) return "برای تالار سطح ۲۱، برج توپخانه باید سطح ۳ باشد.";
                return null;
            case 26:
                if (!HasBuilding(8, 3)) return "برای تالار سطح ۲۶، آکادمی باید سطح ۳ باشد.";
                if (!HasBuilding(9, 3)) return "برای تالار سطح ۲۶، کارگاه محاصره باید سطح ۳ باشد.";
                return null;
            default: return null;
        }
    }

    public static string GetBuildingUpgradeRequirement(int buildingId, int nextLevel, int capitalLevel)
    {
        if (buildingId == 3 && nextLevel >= 2 && capitalLevel < 2)
            return "برای این ارتقا، تالار مرکزی باید سطح ۲ باشد.";
        if ((buildingId == 11 || buildingId == 6) && nextLevel >= 2 && capitalLevel < 4)
            return "برای این ارتقا، تالار مرکزی باید سطح ۴ باشد.";
        if (buildingId == 8 && nextLevel >= 2 && capitalLevel < 8)
            return "برای این ارتقا، تالار مرکزی باید سطح ۸ باشد.";
        if (buildingId == 9 && nextLevel >= 2 && capitalLevel < 12)
            return "برای این ارتقا، تالار مرکزی باید سطح ۱۲ باشد.";
        return null;
    }

    public static int GetArmyTier(int capitalLevel)
    {
        if (capitalLevel >= 21 && HasBuilding(11, 3) && HasBuilding(9, 2)) return 4;
        if (capitalLevel >= 11 && HasBuilding(11, 2) && HasBuilding(8, 2)) return 3;
        if (capitalLevel >= 6 && HasBuilding(11, 1)) return 2;
        return HasBuilding(3, 1) ? 1 : 0;
    }

    public static int GetWeaponTier(int capitalLevel)
    {
        if (capitalLevel >= 21 && HasBuilding(6, 3) && HasBuilding(11, 3)) return 4;
        if (capitalLevel >= 11 && HasBuilding(6, 2)) return 3;
        if (capitalLevel >= 6 && HasBuilding(11, 2)) return 2;
        return HasBuilding(4, 1) ? 1 : 0;
    }
}
