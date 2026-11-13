using System;
using System.IO;
using UnityEngine;

public class VillageSimulation : MonoBehaviour
{
    public static VillageSimulation Instance { get; private set; }

    [Min(1)] public int population = 7;
    [Min(0)] public int soldiers = 2;

    public ResourceInventory Inventory { get; private set; }

    public int wood { get { return Inventory != null ? Inventory.GetAmount(ResourceType.Wood) : 0; } }
    public int stone { get { return Inventory != null ? Inventory.GetAmount(ResourceType.Stone) : 0; } }
    public int gold { get { return Inventory != null ? Inventory.GetAmount(ResourceType.Gold) : 0; } }
    public int food { get { return Inventory != null ? Inventory.GetAmount(ResourceType.Food) : 0; } }

    [Min(1f)] public float productionInterval = 12f;
    private float productionTimer;
    private const int MaximumRosterCount = 500;
    private const int RosterVersion = 1;

    [Serializable]
    private class RosterSaveData
    {
        public int version = RosterVersion;
        public int population;
        public int soldiers;
    }

    private string RosterPath { get { return Path.Combine(Application.persistentDataPath, "village-roster.json"); } }
    private string RosterBackupPath { get { return RosterPath + ".bak"; } }
    private string RosterTempPath { get { return RosterPath + ".tmp"; } }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Inventory = GetComponent<ResourceInventory>();
        if (Inventory == null)
            Inventory = gameObject.AddComponent<ResourceInventory>();
    }

    private void Start()
    {
        // Restore resources and the local village roster before creating citizens.
        if (GameSaveSystem.HasSave())
            GameSaveSystem.Load(Inventory);
        LoadRoster();

        SpawnPeople(population, soldiers);
    }

    private void Update()
    {
        productionTimer += Time.deltaTime;
        if (productionTimer >= productionInterval)
        {
            productionTimer = 0f;
            Inventory.Add(ResourceType.Food, Mathf.Max(1, population / 3));
            Inventory.Add(ResourceType.Wood, 1);
        }
    }

    // Kept as the shared economy API so building and defense scripts use
    // the same inventory instead of maintaining a second set of resources.
    public bool TrySpend(int woodCost, int stoneCost, int foodCost)
    {
        if (Inventory == null) return false;

        ResourceAmounts cost = new ResourceAmounts
        {
            wood = Mathf.Max(0, woodCost),
            stone = Mathf.Max(0, stoneCost),
            food = Mathf.Max(0, foodCost)
        };
        return Inventory.TrySpend(cost);
    }

    public bool SaveRoster()
    {
        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            RosterSaveData data = new RosterSaveData
            {
                version = RosterVersion,
                population = Mathf.Clamp(population, 1, MaximumRosterCount),
                soldiers = Mathf.Clamp(soldiers, 0, MaximumRosterCount)
            };
            File.WriteAllText(RosterTempPath, JsonUtility.ToJson(data));
            if (File.Exists(RosterPath))
                File.Copy(RosterPath, RosterBackupPath, true);
            if (File.Exists(RosterPath)) File.Delete(RosterPath);
            File.Move(RosterTempPath, RosterPath);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Village roster save failed: " + exception.Message);
            TryDeleteRosterTemp();
            return false;
        }
    }

    private void LoadRoster()
    {
        RosterSaveData data = ReadRosterFile(RosterPath);
        if (data == null)
        {
            data = ReadRosterFile(RosterBackupPath);
            if (data != null)
            {
                try { File.Copy(RosterBackupPath, RosterPath, true); }
                catch (Exception exception) { Debug.LogWarning("Roster backup restore failed: " + exception.Message); }
            }
        }

        if (data == null) return;
        population = Mathf.Clamp(data.population, 1, MaximumRosterCount);
        soldiers = Mathf.Clamp(data.soldiers, 0, MaximumRosterCount);
    }

    private RosterSaveData ReadRosterFile(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;
            RosterSaveData data = JsonUtility.FromJson<RosterSaveData>(File.ReadAllText(path));
            if (data == null || data.version != RosterVersion ||
                data.population < 1 || data.population > MaximumRosterCount ||
                data.soldiers < 0 || data.soldiers > MaximumRosterCount)
                return null;
            return data;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Village roster load failed: " + exception.Message);
            return null;
        }
    }

    private void TryDeleteRosterTemp()
    {
        try { if (File.Exists(RosterTempPath)) File.Delete(RosterTempPath); }
        catch { }
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) SaveRoster();
    }

    private void OnApplicationQuit()
    {
        SaveRoster();
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
            if (soldier)
            {
                walker.enabled = false;
                person.AddComponent<VillageGuard>();
            }
        }
    }

    private void OnGUI()
    {
        if (Inventory == null) return;
        Matrix4x4 previousMatrix = MobileUiLayout.Begin();

        GUI.Box(new Rect(12, 190, 390, 112),
            "منابع دهکده" +
            "\nچوب: " + wood + "   سنگ: " + stone + "   طلا: " + gold + "   غذا: " + food +
            "\nجمعیت: " + population + "   نگهبانان: " + soldiers +
            "\nتولید خودکار: هر " + productionInterval.ToString("0") + " ثانیه");

        if (GUI.Button(new Rect(18, 308, 90, 34), "جمع‌آوری چوب +10"))
            Inventory.Add(ResourceType.Wood, 10);
        if (GUI.Button(new Rect(114, 308, 90, 34), "جمع‌آوری سنگ +10"))
            Inventory.Add(ResourceType.Stone, 10);
        if (GUI.Button(new Rect(210, 308, 90, 34), "برداشت غذا +10"))
            Inventory.Add(ResourceType.Food, 10);

        if (GUI.Button(new Rect(18, 346, 132, 32), "ذخیره بازی"))
        {
            LocalSaveCoordinator coordinator = FindObjectOfType<LocalSaveCoordinator>();
            if (coordinator != null) coordinator.SaveAll();
            else if (GameSaveSystem.Save(Inventory))
                Debug.Log("منابع بازی ذخیره شد.");
            SaveRoster();
        }
        if (GUI.Button(new Rect(158, 346, 132, 32), "بارگذاری بازی"))
        {
            if (!GameSaveSystem.Load(Inventory))
                Debug.LogWarning("ذخیره معتبری برای بارگذاری پیدا نشد.");
        }
    
        MobileUiLayout.End(previousMatrix);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
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
