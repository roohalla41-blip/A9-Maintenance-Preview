using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum AfghanEmpireAge
{
    Village = 0,
    Confederacy = 1,
    Kingdom = 2,
    Sultanate = 3,
    Empire = 4,
    Renaissance = 5
}

[Serializable]
public sealed class EmpireProgressState
{
    public int version = 1;
    public int capitalLevel = 1;
    public int experience;
    public int totalExperience;
    public List<string> researchedTechnologies = new List<string>();
    public List<string> completedObjectives = new List<string>();
}

public sealed class EmpireProgressionSystem : MonoBehaviour
{
    public const int MaximumCapitalLevel = 30;
    private const string SaveName = "afghanistan-old-empire-progress.json";

    [Serializable]
    private sealed class TechnologyDefinition
    {
        public string id;
        public string title;
        public string description;
        public int ageLevel;
        public ResourceAmounts cost;
        public string prerequisite;
        public int experienceReward;
    }

    private static readonly TechnologyDefinition[] Technologies =
    {
        Tech("irrigation", "آبیاری پیشرفته", "افزایش بازده مزارع و مدیریت آب", 1, 35, 20, 25, null, 90),
        Tech("masonry", "سنگ‌تراشی", "گشودن سازه‌های سنگی مقاوم", 1, 25, 55, 15, null, 90),
        Tech("caravan", "راه‌های کاروانی", "گسترش تجارت و مسیرهای دوردست", 2, 55, 35, 45, "irrigation", 130),
        Tech("archery", "آموزش کمانداران", "گشودن واحدهای کماندار تخصصی", 2, 45, 30, 35, "masonry", 130),
        Tech("metallurgy", "فلزکاری", "گشودن تجهیزات و برج‌های توپخانه", 3, 65, 90, 55, "masonry", 180),
        Tech("cavalry", "سواره‌نظام", "گشودن نیروهای سریع و ضربتی", 3, 80, 45, 70, "caravan", 180),
        Tech("fortification", "مهندسی استحکامات", "افزایش دوام دیوارها و دژها", 4, 75, 150, 65, "metallurgy", 230),
        Tech("medicine", "دانش درمان", "بهبود بازیابی و پایداری ارتش", 4, 90, 75, 110, "caravan", 230),
        Tech("siegecraft", "مهندسی محاصره", "گشودن ماشین‌های محاصره", 5, 130, 180, 110, "fortification", 300),
        Tech("diplomacy", "دیپلماسی دربار", "گشودن پیمان‌ها و روابط منطقه‌ای", 5, 150, 90, 160, "medicine", 300),
        Tech("architecture", "معماری باشکوه", "گشودن بناهای شاخص پایتخت", 6, 190, 230, 150, "siegecraft", 380),
        Tech("logistics", "لشکرکشی سازمان‌یافته", "افزایش ظرفیت تدارکات سپاه", 6, 180, 150, 220, "diplomacy", 380)
    };

    public static EmpireProgressionSystem Instance { get; private set; }
    public EmpireProgressState State { get; private set; }
    public event Action ProgressChanged;

    public int CapitalLevel { get { return State != null ? Mathf.Clamp(State.capitalLevel, 1, MaximumCapitalLevel) : 1; } }
    public int Experience { get { return State != null ? State.experience : 0; } }
    public int ExperienceToNextLevel { get { return CapitalLevel >= MaximumCapitalLevel ? 0 : 100 + CapitalLevel * 75; } }
    public AfghanEmpireAge CurrentAge { get { return (AfghanEmpireAge)Mathf.Clamp((CapitalLevel - 1) / 5, 0, 5); } }

    public string CurrentAgeName
    {
        get
        {
            switch (CurrentAge)
            {
                case AfghanEmpireAge.Village: return "دهکده";
                case AfghanEmpireAge.Confederacy: return "اتحاد قبایل";
                case AfghanEmpireAge.Kingdom: return "پادشاهی";
                case AfghanEmpireAge.Sultanate: return "سلطنت";
                case AfghanEmpireAge.Empire: return "امپراتوری";
                default: return "عصر شکوفایی";
            }
        }
    }

    private string SavePath { get { return Path.Combine(Application.persistentDataPath, SaveName); } }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Load();
    }

    public ResourceAmounts GetCapitalUpgradeCost()
    {
        if (CapitalLevel >= MaximumCapitalLevel) return null;
        int level = CapitalLevel;
        return new ResourceAmounts
        {
            wood = 90 + level * 28,
            stone = 65 + level * 34,
            gold = 20 + level * 22,
            food = 45 + level * 18
        };
    }

    public bool CanUpgradeCapital(ResourceInventory inventory)
    {
        ResourceAmounts cost = GetCapitalUpgradeCost();
        return cost != null && inventory != null &&
            Experience >= ExperienceToNextLevel && inventory.HasEnough(cost);
    }

    public bool TryUpgradeCapital(ResourceInventory inventory)
    {
        ResourceAmounts cost = GetCapitalUpgradeCost();
        if (cost == null || inventory == null || Experience < ExperienceToNextLevel ||
            !inventory.TrySpend(cost)) return false;

        int requiredExperience = ExperienceToNextLevel;
        State.capitalLevel++;
        State.experience -= requiredExperience;
        Save();
        RaiseChanged();
        return true;
    }

    public void AwardExperience(int amount)
    {
        if (amount <= 0 || State == null) return;
        State.experience += amount;
        State.totalExperience += amount;
        Save();
        RaiseChanged();
    }

    public bool IsBuildingUnlocked(int buildingId)
    {
        switch (buildingId)
        {
            case 1: return CapitalLevel >= 1;  // homes
            case 2: return CapitalLevel >= 1;  // storehouse
            case 3: return CapitalLevel >= 2;  // barracks
            case 4: return CapitalLevel >= 2;  // archer tower
            case 5: return CapitalLevel >= 3;  // stone wall
            case 6: return CapitalLevel >= 6;  // cannon tower
            case 7: return CapitalLevel >= 11; // advanced keep
            case 8: return CapitalLevel >= 16; // royal academy
            case 9: return CapitalLevel >= 21; // siege workshop
            case 10: return CapitalLevel >= 26; // grand palace
            default: return false;
        }
    }

    public bool IsTechnologyResearched(string id)
    {
        return State != null && !string.IsNullOrEmpty(id) && State.researchedTechnologies.Contains(id);
    }

    public bool TryResearch(string id, ResourceInventory inventory)
    {
        TechnologyDefinition technology = FindTechnology(id);
        if (technology == null || inventory == null || IsTechnologyResearched(id)) return false;
        if ((int)CurrentAge + 1 < technology.ageLevel) return false;
        if (!string.IsNullOrEmpty(technology.prerequisite) &&
            !IsTechnologyResearched(technology.prerequisite)) return false;
        if (!inventory.TrySpend(technology.cost)) return false;

        State.researchedTechnologies.Add(id);
        AwardExperience(technology.experienceReward);
        Save();
        RaiseChanged();
        return true;
    }

    public string GetTechnologyStatus(string id)
    {
        TechnologyDefinition technology = FindTechnology(id);
        if (technology == null) return "نامعتبر";
        if (IsTechnologyResearched(id)) return "پژوهش شده";
        if ((int)CurrentAge + 1 < technology.ageLevel) return "قفل: عصر " + technology.ageLevel;
        if (!string.IsNullOrEmpty(technology.prerequisite) &&
            !IsTechnologyResearched(technology.prerequisite)) return "نیازمند: " + technology.prerequisite;
        return "آماده پژوهش";
    }

    public void CompleteObjective(string objectiveId, int experienceReward)
    {
        if (State == null || string.IsNullOrEmpty(objectiveId) ||
            State.completedObjectives.Contains(objectiveId)) return;
        State.completedObjectives.Add(objectiveId);
        AwardExperience(experienceReward);
        Save();
        RaiseChanged();
    }

    public bool IsObjectiveComplete(string objectiveId)
    {
        return State != null && State.completedObjectives.Contains(objectiveId);
    }

    public void Save()
    {
        if (State == null) return;
        try
        {
            string json = JsonUtility.ToJson(State, true);
            string temporary = SavePath + ".tmp";
            File.WriteAllText(temporary, json);
            if (File.Exists(SavePath)) File.Delete(SavePath);
            File.Move(temporary, SavePath);
        }
        catch (Exception exception) { Debug.LogError("Empire progress save failed: " + exception.Message); }
    }

    private void Load()
    {
        State = new EmpireProgressState();
        if (!File.Exists(SavePath)) return;
        try
        {
            EmpireProgressState loaded = JsonUtility.FromJson<EmpireProgressState>(File.ReadAllText(SavePath));
            if (loaded != null && loaded.version == 1)
            {
                if (loaded.researchedTechnologies == null) loaded.researchedTechnologies = new List<string>();
                if (loaded.completedObjectives == null) loaded.completedObjectives = new List<string>();
                loaded.capitalLevel = Mathf.Clamp(loaded.capitalLevel, 1, MaximumCapitalLevel);
                State = loaded;
            }
        }
        catch (Exception exception) { Debug.LogError("Empire progress load failed: " + exception.Message); }
    }

    private static TechnologyDefinition FindTechnology(string id)
    {
        for (int i = 0; i < Technologies.Length; i++)
            if (Technologies[i].id == id) return Technologies[i];
        return null;
    }

    private static TechnologyDefinition Tech(string id, string title, string description, int age,
        int wood, int stone, int gold, string prerequisite, int reward)
    {
        return new TechnologyDefinition
        {
            id = id, title = title, description = description, ageLevel = age,
            cost = new ResourceAmounts { wood = wood, stone = stone, gold = gold },
            prerequisite = prerequisite, experienceReward = reward
        };
    }

    private void RaiseChanged()
    {
        if (ProgressChanged != null) ProgressChanged();
    }

    private void OnApplicationPause(bool paused) { if (paused) Save(); }
    private void OnApplicationQuit() { Save(); }
    private void OnDestroy() { if (Instance == this) Instance = null; }
}
