using System.Collections.Generic;
using UnityEngine;

// Offline raid loop. Primitive visuals remain temporary until production art is integrated.
public class EnemyRaidSystem : MonoBehaviour
{
    public float firstRaidDelay = 35f;
    public float raidInterval = 55f;
    [Min(1)] public int villageHealth = 100;
    public int maximumRaiders = 15;

    private float raidTimer;
    private int raidNumber;
    private bool raidActive;
    private bool villageDefeated;
    private string status = "دهکده در آرامش است";

    private void Start()
    {
        raidTimer = Mathf.Max(5f, firstRaidDelay);
        RefreshStatus();
    }

    private void Update()
    {
        if (villageDefeated) return;

        if (raidActive)
        {
            if (Raider.ActiveCount == 0)
            {
                raidActive = false;
                raidTimer = Mathf.Max(10f, raidInterval);
                status = "حمله دفع شد؛ دهکده فرصت بازسازی دارد";
            }
            return;
        }

        raidTimer -= Time.deltaTime;
        if (raidTimer <= 0f) StartRaid();
    }

    private void StartRaid()
    {
        if (villageDefeated || raidActive) return;

        raidNumber++;
        int count = Mathf.Clamp(3 + raidNumber * 2, 1, Mathf.Max(1, maximumRaiders));
        raidActive = true;
        status = "حمله شماره " + raidNumber + " آغاز شد";

        for (int i = 0; i < count; i++)
        {
            float angle = (Mathf.PI * 2f * i) / count;
            Vector3 spawn = new Vector3(Mathf.Cos(angle) * 21f, 1f, Mathf.Sin(angle) * 21f);
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "مهاجم دشمن";
            enemy.transform.position = spawn;
            enemy.transform.localScale = new Vector3(.7f, 1.05f, .7f);
            Renderer renderer = enemy.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = new Color(.62f, .16f, .14f);

            Raider raider = enemy.AddComponent<Raider>();
            raider.target = new Vector3(3f, 1f, 3f);
            raider.health = 35f + raidNumber * 5f;
        }
    }

    public void TrainGuard()
    {
        if (villageDefeated)
        {
            status = "دهکده سقوط کرده است؛ ابتدا بازی را از ذخیره بازیابی کن";
            return;
        }

        VillageSimulation economy = VillageSimulation.Instance;
        if (economy == null) return;

        if (economy.soldiers >= 500)
        {
            status = "ظرفیت نگهبانان تکمیل شده است";
            return;
        }

        BuildingInstance[] buildings = FindObjectsOfType<BuildingInstance>();
        int barracksLevel = 0;
        foreach (BuildingInstance building in buildings)
        {
            if (building != null && building.definitionId == 3)
                barracksLevel = Mathf.Max(barracksLevel, building.level);
        }
        if (barracksLevel == 0)
        {
            status = "برای آموزش نگهبان، ابتدا سربازخانه بساز";
            return;
        }

        int soldierLimit = 20 + barracksLevel * 20;
        if (economy.soldiers >= soldierLimit)
        {
            status = "ظرفیت سربازخانه سطح " + barracksLevel + " تکمیل شده است";
            return;
        }

        int woodCost = 25 + (barracksLevel - 1) * 8;
        int foodCost = 20 + (barracksLevel - 1) * 6;
        if (!economy.TrySpend(woodCost, 0, foodCost))
        {
            status = "منابع کافی برای آموزش نگهبان ندارید";
            return;
        }

        GameObject guard = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        guard.name = "نگهبان آموزش‌دیده";
        guard.transform.position = new Vector3(Random.Range(-6f, 7f), 1f, Random.Range(-7f, 7f));
        guard.transform.localScale = new Vector3(.65f, 1f, .65f);
        Renderer renderer = guard.GetComponent<Renderer>();
        if (renderer != null) renderer.material.color = new Color(.25f, .34f, .58f);
        VillageGuard trainedGuard = guard.AddComponent<VillageGuard>();
        EmpireProgressionSystem progression = EmpireProgressionSystem.Instance;
        int armyTier = BuildingProgressionRules.GetArmyTier(
            progression != null ? progression.CapitalLevel : 1);
        trainedGuard.attackDamage = 8f + armyTier * 5f + (barracksLevel - 1) * 2.5f;
        trainedGuard.moveSpeed = 2.8f + armyTier * .25f + (barracksLevel - 1) * .08f;
        economy.soldiers++;
        LocalSaveCoordinator coordinator = FindObjectOfType<LocalSaveCoordinator>();
        if (coordinator != null)
            coordinator.SaveAll();
        else
        {
            GameSaveSystem.Save(economy.Inventory);
            economy.SaveRoster();
        }
        status = "نگهبان سطح " + Mathf.Max(1, armyTier) +
            " آموزش دید؛ ظرفیت سربازخانه: " + soldierLimit;
    }

    public void TrainArmoredVehicle()
    {
        VillageSimulation economy = VillageSimulation.Instance;
        EmpireProgressionSystem progression = EmpireProgressionSystem.Instance;
        if (economy == null || economy.Inventory == null || progression == null) return;

        int tier = BuildingProgressionRules.GetArmyTier(progression.CapitalLevel);
        if (tier < 3)
        {
            status = "تانک قفل است؛ تالار سطح ۱۱، زرادخانه سطح ۲ و آکادمی سطح ۲ لازم است";
            return;
        }
        if (economy.armoredVehicles >= 20)
        {
            status = "ظرفیت خودروهای زرهی تکمیل شده است";
            return;
        }

        ResourceAmounts cost = new ResourceAmounts
        {
            wood = 100, stone = 150, gold = 100, food = 120
        };
        if (!economy.Inventory.TrySpend(cost))
        {
            status = "منابع کافی برای ساخت تانک ندارید";
            return;
        }

        Vector3 spawn = new Vector3(Random.Range(-8f, 8f), .25f, Random.Range(-8f, 8f));
        ArmoredVehicleGuard.Create(spawn);
        economy.armoredVehicles++;
        LocalSaveCoordinator coordinator = FindObjectOfType<LocalSaveCoordinator>();
        if (coordinator != null) coordinator.SaveAll();
        else
        {
            GameSaveSystem.Save(economy.Inventory);
            economy.SaveRoster();
        }
        status = "خودروی زرهی دفاعی ساخته شد";
    }

    public void DamageVillage(int amount)
    {
        if (villageDefeated || amount <= 0) return;

        villageHealth = Mathf.Max(0, villageHealth - amount);
        if (villageHealth == 0)
        {
            villageDefeated = true;
            raidActive = false;
            status = "دهکده سقوط کرد؛ حمله متوقف شد";
            Raider.RemoveAllActive();
        }
    }

    private void RefreshStatus()
    {
        if (!villageDefeated && !raidActive)
            status = "دهکده در آرامش است";
    }

    private void OnGUI()
    {
        if (TechnologyResearchPanel.IsOpen) return;
        GridBuildingSystem builder = GetComponent<GridBuildingSystem>();
        if (builder != null && builder.IsBuildMode) return;
        Matrix4x4 previousMatrix = MobileUiLayout.Begin();
        GUI.Box(new Rect(12, 400, 390, 104),
            "دفاع از دهکده | سلامت: " + villageHealth + "%\n" +
            (villageActiveLabel()) + "\n" + status);
        if (GUI.Button(new Rect(18, 510, 180, 34), "آموزش نگهبان"))
            TrainGuard();
        GUI.enabled = VillageSimulation.Instance != null &&
            VillageSimulation.Instance.armoredVehicles < 20 &&
            EmpireProgressionSystem.Instance != null &&
            BuildingProgressionRules.GetArmyTier(EmpireProgressionSystem.Instance.CapitalLevel) >= 3;
        if (GUI.Button(new Rect(205, 510, 185, 34), "ساخت تانک دفاعی"))
            TrainArmoredVehicle();
        GUI.enabled = true;
    
        MobileUiLayout.End(previousMatrix);
    }

    private string villageActiveLabel()
    {
        if (villageDefeated) return "وضعیت: سقوط کرده";
        if (raidActive) return "مهاجمان باقی‌مانده: " + Raider.ActiveCount;
        return "حمله بعدی: " + Mathf.CeilToInt(Mathf.Max(0f, raidTimer)) + " ثانیه";
    }
}

public class VillageGuard : MonoBehaviour
{
    public float moveSpeed = 3.2f;
    public float attackRange = 1.5f;
    public float attackDamage = 12f;
    public float attackCooldown = 1f;
    private float attackTimer;

    private void Update()
    {
        Raider nearest = null;
        float best = float.MaxValue;
        for (int i = 0; i < Raider.ActiveCount; i++)
        {
            Raider enemy = Raider.GetActive(i);
            if (enemy == null) continue;
            float distance = (enemy.transform.position - transform.position).sqrMagnitude;
            if (distance < best) { best = distance; nearest = enemy; }
        }

        if (nearest == null) return;
        Vector3 direction = nearest.transform.position - transform.position;
        direction.y = 0f;
        if (direction.magnitude > attackRange)
        {
            transform.position += direction.normalized * moveSpeed * Time.deltaTime;
            if (direction.sqrMagnitude > .01f) transform.rotation = Quaternion.LookRotation(direction);
        }
        else
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                nearest.TakeDamage(attackDamage);
                attackTimer = Mathf.Max(.1f, attackCooldown);
            }
        }
    }
}

public class Raider : MonoBehaviour
{
    private static readonly List<Raider> active = new List<Raider>();

    public static int ActiveCount
    {
        get
        {
            PruneInactiveEntries();
            return active.Count;
        }
    }

    public static Raider GetActive(int index)
    {
        PruneInactiveEntries();
        return index >= 0 && index < active.Count ? active[index] : null;
    }

    private static void PruneInactiveEntries()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            if (active[i] == null)
                active.RemoveAt(i);
        }
    }

    public Vector3 target;
    public float health = 35f;
    public float moveSpeed = 1.8f;
    private float attackTimer;
    private EnemyRaidSystem raidSystem;

    private void OnEnable()
    {
        if (!active.Contains(this)) active.Add(this);
        if (raidSystem == null) raidSystem = FindObjectOfType<EnemyRaidSystem>();
    }

    private void OnDisable()
    {
        active.Remove(this);
    }

    private void Update()
    {
        if (raidSystem == null) raidSystem = FindObjectOfType<EnemyRaidSystem>();
        if (raidSystem == null) return;

        Vector3 direction = target - transform.position;
        direction.y = 0f;
        if (direction.magnitude > 1.8f)
        {
            transform.position += direction.normalized * moveSpeed * Time.deltaTime;
            if (direction.sqrMagnitude > .01f) transform.rotation = Quaternion.LookRotation(direction);
        }
        else
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
            {
                raidSystem.DamageVillage(5);
                attackTimer = 2f;
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;
        health -= amount;
        if (health <= 0f) Destroy(gameObject);
    }

    public static void RemoveAllActive()
    {
        for (int i = active.Count - 1; i >= 0; i--)
            if (active[i] != null) Destroy(active[i].gameObject);
        active.Clear();
    }
}


/// <summary>
/// Unlockable offline armored defender. Its visual is a temporary low-poly blockout.
/// </summary>
public sealed class ArmoredVehicleGuard : MonoBehaviour
{
    private float fireTimer;
    public float moveSpeed = 1.25f;
    public float attackRange = 8f;
    public float damage = 28f;

    public static ArmoredVehicleGuard Create(Vector3 position)
    {
        GameObject root = new GameObject("خودروی زرهی دفاعی");
        root.transform.position = position;

        GameObject hull = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hull.name = "بدنه زرهی";
        hull.transform.SetParent(root.transform, false);
        hull.transform.localPosition = new Vector3(0f, .55f, 0f);
        hull.transform.localScale = new Vector3(2.5f, .75f, 1.55f);
        hull.GetComponent<Renderer>().material.color = new Color(.27f, .32f, .24f);

        GameObject turret = GameObject.CreatePrimitive(PrimitiveType.Cube);
        turret.name = "برجک";
        turret.transform.SetParent(root.transform, false);
        turret.transform.localPosition = new Vector3(0f, 1.05f, 0f);
        turret.transform.localScale = new Vector3(1.05f, .45f, .95f);
        turret.GetComponent<Renderer>().material.color = new Color(.2f, .25f, .19f);

        GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barrel.name = "لوله دفاعی";
        barrel.transform.SetParent(root.transform, false);
        // Unity forward is +Z; keep the barrel aligned with the vehicle's
        // forward direction so target-facing rotation aims the weapon correctly.
        barrel.transform.localPosition = new Vector3(0f, 1.08f, .72f);
        barrel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        barrel.transform.localScale = new Vector3(.14f, .7f, .14f);
        barrel.GetComponent<Renderer>().material.color = new Color(.16f, .18f, .16f);

        for (int side = -1; side <= 1; side += 2)
        {
            GameObject track = GameObject.CreatePrimitive(PrimitiveType.Cube);
            track.name = "زنجیر شنی";
            track.transform.SetParent(root.transform, false);
            track.transform.localPosition = new Vector3(side * 1.1f, .25f, 0f);
            track.transform.localScale = new Vector3(.38f, .48f, 1.85f);
            track.GetComponent<Renderer>().material.color = new Color(.12f, .13f, .12f);
        }

        return root.AddComponent<ArmoredVehicleGuard>();
    }

    private void Update()
    {
        Raider target = null;
        float nearest = attackRange * attackRange;
        int count = Raider.ActiveCount;
        for (int i = 0; i < count; i++)
        {
            Raider enemy = Raider.GetActive(i);
            if (enemy == null) continue;
            float distance = (enemy.transform.position - transform.position).sqrMagnitude;
            if (distance < nearest) { nearest = distance; target = enemy; }
        }
        if (target == null) return;

        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 3.2f)
        {
            transform.position += direction.normalized * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(direction);
            return;
        }

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            target.TakeDamage(damage);
            fireTimer = 1.8f;
        }
    }
}
