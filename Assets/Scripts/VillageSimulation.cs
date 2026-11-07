using UnityEngine;

public class VillageSimulation : MonoBehaviour
{
    public static VillageSimulation Instance { get; private set; }
    public int wood = 120;
    public int stone = 80;
    public int food = 150;
    public int population = 7;
    public int soldiers = 2;
    private float productionTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        SpawnPeople(population, soldiers);
    }

    private void Update()
    {
        productionTimer += Time.deltaTime;
        if (productionTimer >= 12f)
        {
            productionTimer = 0f;
            food += Mathf.Max(1, population / 3);
            wood += 1;
        }
    }

    public bool TrySpend(int woodCost, int stoneCost, int foodCost)
    {
        if (wood < woodCost || stone < stoneCost || food < foodCost) return false;
        wood -= woodCost;
        stone -= stoneCost;
        food -= foodCost;
        return true;
    }

    private void SpawnPeople(int citizenCount, int soldierCount)
    {
        for (int i = 0; i < citizenCount + soldierCount; i++)
        {
            bool soldier = i >= citizenCount;
            GameObject person = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            person.name = soldier ? "نگهبان دهکده" : "مردم دهکده";
            person.transform.position = new Vector3(Random.Range(-7f, 8f), 1f, Random.Range(-8f, 8f));
            person.transform.localScale = soldier ? new Vector3(.65f, 1f, .65f) : new Vector3(.55f, .9f, .55f);
            person.GetComponent<Renderer>().material.color = soldier
                ? new Color(.35f, .38f, .48f) : new Color(.72f, .55f, .36f);
            VillageWalker walker = person.AddComponent<VillageWalker>();
            walker.walkSpeed = soldier ? 1.5f : 1.1f;
        }
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(12, 132, 300, 112),
            "منابع دهکده\nچوب: " + wood + "   سنگ: " + stone + "   غذا: " + food +
            "\nجمعیت: " + population + "   نگهبانان: " + soldiers +
            "\nتولید خودکار: هر ۱۲ ثانیه");
        if (GUI.Button(new Rect(18, 250, 90, 34), "جمع‌آوری چوب +10")) wood += 10;
        if (GUI.Button(new Rect(114, 250, 90, 34), "جمع‌آوری سنگ +10")) stone += 10;
        if (GUI.Button(new Rect(210, 250, 90, 34), "برداشت غذا +10")) food += 10;
    }
}

public class VillageWalker : MonoBehaviour
{
    public float walkSpeed = 1.1f;
    private Vector3 destination;
    private float retargetTimer;

    private void Start() { ChooseDestination(); }

    private void Update()
    {
        retargetTimer -= Time.deltaTime;
        if (retargetTimer <= 0f || Vector3.Distance(transform.position, destination) < .4f)
            ChooseDestination();

        Vector3 direction = destination - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > .05f)
        {
            transform.position += direction.normalized * walkSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(direction.normalized);
        }
    }

    private void ChooseDestination()
    {
        destination = new Vector3(Random.Range(-9f, 10f), 1f, Random.Range(-9f, 10f));
        retargetTimer = Random.Range(3f, 7f);
    }
}
