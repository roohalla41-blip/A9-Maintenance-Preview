using UnityEngine;
public class WatchtowerDefense : MonoBehaviour
{
    public float range = 9f;
    public float damage = 10f;
    public float fireInterval = 1.4f;
    private float cooldown;
    private void Update()
    {
        cooldown -= Time.deltaTime;
        if (cooldown > 0f) return;
        Raider[] enemies = FindObjectsOfType<Raider>();
        Raider target = null;
        float best = range * range;
        foreach (Raider enemy in enemies)
        {
            float distance = (enemy.transform.position - transform.position).sqrMagnitude;
            if (distance < best) { best = distance; target = enemy; }
        }
        if (target == null) return;
        target.TakeDamage(damage);
        cooldown = fireInterval;
        transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z));
    }
}