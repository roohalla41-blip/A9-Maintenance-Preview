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
        if (economy == null || !economy.TrySpend(25, 0, 20))
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
        guard.AddComponent<VillageGuard>();
        economy.soldiers++;
        economy.SaveRoster();
        status = "یک نگهبان تازه آموزش دید";
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
        Matrix4x4 previousMatrix = MobileUiLayout.Begin();
        GUI.Box(new Rect(12, 292, 300, 104),
            "دفاع از دهکده | سلامت: " + villageHealth + "%\n" +
            (villageActiveLabel()) + "\n" + status);
        if (GUI.Button(new Rect(18, 402, 180, 34), "آموزش نگهبان (۲۵ چوب، ۲۰ غذا)"))
            TrainGuard();
    
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
        for (int i = 0; i < Raider.Active.Count; i++)
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
