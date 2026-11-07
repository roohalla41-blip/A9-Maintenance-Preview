using UnityEngine;

// Lightweight prototype combat: deliberately uses primitive characters until art assets are added.
public class EnemyRaidSystem : MonoBehaviour
{
    public float firstRaidDelay = 35f;
    public float raidInterval = 55f;
    public int villageHealth = 100;
    private float raidTimer;
    private int raidNumber;
    private string status = "دهکده در آرامش است";

    private void Start() { raidTimer = firstRaidDelay; }

    private void Update()
    {
        raidTimer -= Time.deltaTime;
        if (raidTimer <= 0f)
        {
            StartRaid();
            raidTimer = raidInterval;
        }
    }

    private void StartRaid()
    {
        raidNumber++;
        int count = Mathf.Min(3 + raidNumber * 2, 15);
        status = "حمله شماره " + raidNumber + " آغاز شد!";
        for (int i = 0; i < count; i++)
        {
            float angle = (Mathf.PI * 2f * i) / count;
            Vector3 spawn = new Vector3(Mathf.Cos(angle) * 21f, 1f, Mathf.Sin(angle) * 21f);
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "مهاجم دشمن";
            enemy.transform.position = spawn;
            enemy.transform.localScale = new Vector3(.7f, 1.05f, .7f);
            enemy.GetComponent<Renderer>().material.color = new Color(.62f, .16f, .14f);
            Raider raider = enemy.AddComponent<Raider>();
            raider.target = new Vector3(3f, 1f, 3f);
            raider.health = 35 + raidNumber * 5;
        }
    }

    public void TrainGuard()
    {
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
        guard.GetComponent<Renderer>().material.color = new Color(.25f, .34f, .58f);
        guard.AddComponent<VillageGuard>();
        economy.soldiers++;
        status = "یک نگهبان تازه آموزش دید";
    }

    public void DamageVillage(int amount)
    {
        villageHealth = Mathf.Max(0, villageHealth - amount);
        if (villageHealth == 0) status = "دهکده سقوط کرد! نگهبانان بیشتری آموزش بده";
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(12, 292, 300, 104),
            "دفاع از دهکده | سلامت: " + villageHealth + "%\n" +
            "حمله بعدی: " + Mathf.CeilToInt(Mathf.Max(0, raidTimer)) + " ثانیه\n" + status);
        if (GUI.Button(new Rect(18, 402, 180, 34), "آموزش نگهبان (۲۵ چوب، ۲۰ غذا)"))
            TrainGuard();
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
        Raider[] enemies = FindObjectsOfType<Raider>();
        Raider nearest = null;
        float best = float.MaxValue;
        foreach (Raider enemy in enemies)
        {
            float d = (enemy.transform.position - transform.position).sqrMagnitude;
            if (d < best) { best = d; nearest = enemy; }
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
                attackTimer = attackCooldown;
            }
        }
    }
}

public class Raider : MonoBehaviour
{
    public Vector3 target;
    public float health = 35f;
    public float moveSpeed = 1.8f;
    private float attackTimer;

    private void Update()
    {
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
                EnemyRaidSystem system = FindObjectOfType<EnemyRaidSystem>();
                if (system != null) system.DamageVillage(5);
                attackTimer = 2f;
            }
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f) Destroy(gameObject);
    }
}
