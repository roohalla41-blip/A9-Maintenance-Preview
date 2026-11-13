using UnityEngine;

/// <summary>
/// Local automated tower defense. Targets the shared raid registry without
/// allocating a scene-wide object array every firing cycle.
/// </summary>
public class WatchtowerDefense : MonoBehaviour
{
    [Min(1f)] public float range = 9f;
    [Min(0f)] public float damage = 10f;
    [Min(.1f)] public float fireInterval = 1.4f;

    private float cooldown;
    private BuildingInstance building;

    private void Awake()
    {
        building = GetComponent<BuildingInstance>();
    }

    private void Update()
    {
        cooldown -= Time.deltaTime;
        if (cooldown > 0f) return;

        int level = building != null ? Mathf.Clamp(building.level, 1, BuildingInstance.MaximumLevel) : 1;
        EmpireProgressionSystem progression = EmpireProgressionSystem.Instance;
        int weaponTier = BuildingProgressionRules.GetWeaponTier(
            progression != null ? progression.CapitalLevel : 1);
        float effectiveRange = range + (level - 1) * .45f + Mathf.Max(0, weaponTier - 1) * .6f;
        float effectiveDamage = damage * (1f + (level - 1) * .22f) *
            (1f + Mathf.Max(0, weaponTier - 1) * .18f);
        float best = effectiveRange * effectiveRange;
        Raider target = null;

        int enemyCount = Raider.ActiveCount;
        for (int i = 0; i < enemyCount; i++)
        {
            Raider enemy = Raider.GetActive(i);
            if (enemy == null) continue;

            float distance = (enemy.transform.position - transform.position).sqrMagnitude;
            if (distance < best)
            {
                best = distance;
                target = enemy;
            }
        }

        if (target == null) return;

        target.TakeDamage(effectiveDamage);
        cooldown = Mathf.Max(.1f, fireInterval);
        Vector3 aimPoint = target.transform.position;
        aimPoint.y = transform.position.y;
        transform.LookAt(aimPoint);
    }
}
