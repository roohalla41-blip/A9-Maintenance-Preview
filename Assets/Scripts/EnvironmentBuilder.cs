using UnityEngine;

// Procedural first-pass 3D environment for the Afghanistan Old prototype.
// Art is generated from meshes/primitives so the scene can run without external assets.
public class EnvironmentBuilder : MonoBehaviour
{
    private Transform root;

    public void Build(int size, Material groundMat, Material waterMat)
    {
        if (transform.Find("GeneratedVillage") != null) return;
        GameObject container = new GameObject("GeneratedVillage");
        container.transform.SetParent(transform, false);
        root = container.transform;

        BuildTerrain(Mathf.Max(32, size), groundMat);
        BuildRiver(waterMat);
        BuildRoads();
        BuildVillageCenter();
        // Add Panjshir-inspired defensive locations to the environment.
        if (GetComponent<PanjshirDefenseKit>() == null)
            gameObject.AddComponent<PanjshirDefenseKit>().Build(root);
        BuildFields();
        BuildTreesAndRocks();
        BuildLighting();
    }

    private void BuildTerrain(int size, Material material)
    {
        int cells = 48;
        float half = size * .5f;
        Vector3[] vertices = new Vector3[(cells + 1) * (cells + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[cells * cells * 6];
        for (int z = 0; z <= cells; z++)
        for (int x = 0; x <= cells; x++)
        {
            float wx = -half + size * x / cells;
            float wz = -half + size * z / cells;
            float hills = Mathf.Sin(wx * .19f) * .22f + Mathf.Cos(wz * .16f) * .18f;
            float ridge = Mathf.PerlinNoise((wx + 80f) * .075f, (wz + 80f) * .075f) * .8f;
            vertices[z * (cells + 1) + x] = new Vector3(wx, hills + ridge - .25f, wz);
            uv[z * (cells + 1) + x] = new Vector2(x / (float)cells, z / (float)cells);
        }
        int t = 0;
        for (int z = 0; z < cells; z++)
        for (int x = 0; x < cells; x++)
        {
            int a = z * (cells + 1) + x;
            int b = a + cells + 1;
            triangles[t++] = a; triangles[t++] = b; triangles[t++] = a + 1;
            triangles[t++] = a + 1; triangles[t++] = b; triangles[t++] = b + 1;
        }
        Mesh mesh = new Mesh();
        mesh.name = "Afghan valley terrain";
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        GameObject terrain = new GameObject("دره و زمین طبیعی");
        terrain.transform.SetParent(root, false);
        terrain.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer renderer = terrain.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material != null ? material : MakeMaterial(new Color(.48f, .43f, .31f));
        terrain.AddComponent<MeshCollider>().sharedMesh = mesh;
    }

    private void BuildRiver(Material waterMat)
    {
        Make("رودخانه آبی", PrimitiveType.Cube, new Vector3(-14f, .05f, 0f),
            new Vector3(3.4f, .12f, 48f), waterMat, new Color(.12f, .38f, .52f));
        Make("کناره رودخانه", PrimitiveType.Cube, new Vector3(-11.95f, -.01f, 0f),
            new Vector3(.45f, .12f, 48f), null, new Color(.66f, .57f, .39f));
    }

    private void BuildRoads()
    {
        Make("جاده اصلی دهکده", PrimitiveType.Cube, new Vector3(1f, .18f, -1f),
            new Vector3(2.6f, .12f, 34f), null, new Color(.68f, .55f, .38f));
        Make("راه بازار", PrimitiveType.Cube, new Vector3(4f, .19f, 4f),
            new Vector3(17f, .12f, 2f), null, new Color(.63f, .51f, .35f));
        for (int i = 0; i < 18; i++)
            Make("سنگفرش راه", PrimitiveType.Cube, new Vector3(-.25f, .27f, -15f + i * 1.7f),
                new Vector3(.16f, .035f, .7f), null, new Color(.43f, .4f, .34f));
    }

    private void BuildVillageCenter()
    {
        BuildHouse("ارگ مرکزی", new Vector3(3f, .55f, 3f), 5.6f, 3.2f, 5.2f, new Color(.72f, .61f, .45f), true, 10);
        BuildHouse("خانه کاهگلی", new Vector3(8f, .45f, -4f), 3.3f, 2.1f, 3.2f, new Color(.65f, .45f, .3f), false, 1);
        BuildHouse("انبار غله", new Vector3(8f, .45f, 3f), 3.5f, 2.2f, 3.5f, new Color(.55f, .4f, .28f), false, 2);
        BuildHouse("سربازخانه", new Vector3(-2f, .5f, -5f), 4.4f, 2.5f, 3.7f, new Color(.52f, .38f, .29f), false, 3);
        BuildHouse("زرادخانه", new Vector3(13f, .4f, 8f), 3.5f, 2f, 3f, new Color(.48f, .38f, .3f), false, 11);
        BuildWatchtower(new Vector3(-7f, .3f, -9f));
        BuildWatchtower(new Vector3(15f, .3f, -12f));
        BuildWall(new Vector3(2f, .55f, -12f), 25f, 1.7f);
    }

    private void BuildHouse(string title, Vector3 basePosition, float width, float height, float depth, Color wallColor, bool grand, int definitionId)
    {
        Transform house = NewGroup(title, basePosition);
        MakeChild(house, "دیوارهای خشتی", PrimitiveType.Cube, new Vector3(0f, height * .5f, 0f),
            new Vector3(width, height, depth), wallColor);
        Color roof = new Color(.36f, .22f, .16f);
        GameObject roofA = MakeChild(house, "شیب سقف اول", PrimitiveType.Cube,
            new Vector3(0f, height + .35f, -depth * .22f), new Vector3(width * 1.12f, .22f, depth * .62f), roof);
        roofA.transform.localRotation = Quaternion.Euler(-25f, 0f, 0f);
        GameObject roofB = MakeChild(house, "شیب سقف دوم", PrimitiveType.Cube,
            new Vector3(0f, height + .35f, depth * .22f), new Vector3(width * 1.12f, .22f, depth * .62f), roof);
        roofB.transform.localRotation = Quaternion.Euler(25f, 0f, 0f);
        MakeChild(house, "دروازه چوبی", PrimitiveType.Cube, new Vector3(0f, .72f, -depth * .501f),
            new Vector3(.72f, 1.42f, .12f), new Color(.24f, .14f, .09f));
        MakeChild(house, "پنجره چپ", PrimitiveType.Cube, new Vector3(-width * .28f, height * .65f, -depth * .505f),
            new Vector3(.48f, .48f, .1f), new Color(.18f, .2f, .18f));
        MakeChild(house, "پنجره راست", PrimitiveType.Cube, new Vector3(width * .28f, height * .65f, -depth * .505f),
            new Vector3(.48f, .48f, .1f), new Color(.18f, .2f, .18f));
        if (grand)
        {
            for (int i = -1; i <= 1; i += 2)
            for (int j = -1; j <= 1; j += 2)
                MakeChild(house, "برجک ارگ", PrimitiveType.Cylinder,
                    new Vector3(i * width * .42f, height + .55f, j * depth * .42f),
                    new Vector3(.65f, .8f, .65f), new Color(.61f, .49f, .35f));
        }
        RegisterUpgradeable(house, definitionId, new Vector3(width, height + 1.2f, depth));
    }

    private void BuildWatchtower(Vector3 position)
    {
        Transform tower = NewGroup("برج نگهبانی", position);
        MakeChild(tower, "پایه برج", PrimitiveType.Cylinder, new Vector3(0f, 1.35f, 0f),
            new Vector3(1.7f, 1.35f, 1.7f), new Color(.59f, .49f, .37f));
        MakeChild(tower, "اتاقک برج", PrimitiveType.Cube, new Vector3(0f, 3f, 0f),
            new Vector3(2f, 1.2f, 2f), new Color(.68f, .57f, .42f));
        MakeChild(tower, "سقف برج", PrimitiveType.Cylinder, new Vector3(0f, 3.8f, 0f),
            new Vector3(2.35f, .28f, 2.35f), new Color(.32f, .2f, .14f));
        RegisterUpgradeable(tower, 4, new Vector3(2.5f, 4.4f, 2.5f));
        tower.gameObject.AddComponent<WatchtowerDefense>();
    }

    private void BuildWall(Vector3 center, float length, float height)
    {
        Transform wall = NewGroup("دیوار دفاعی خشتی", center);
        for (int i = 0; i < 13; i++)
        {
            float x = -length * .5f + i * length / 12f;
            MakeChild(wall, "بخش دیوار", PrimitiveType.Cube,
                new Vector3(x, height * .5f, 0f),
                new Vector3(1.9f, height, .85f), new Color(.56f, .46f, .34f));
            MakeChild(wall, "کنگره دیوار", PrimitiveType.Cube,
                new Vector3(x, height + .18f, 0f),
                new Vector3(.55f, .35f, .9f), new Color(.56f, .46f, .34f));
        }
        RegisterUpgradeable(wall, 5, new Vector3(length + 1f, height + .5f, 1.2f));
    }

    private void RegisterUpgradeable(Transform rootTransform, int definitionId, Vector3 colliderSize)
    {
        BuildingDefinition definition = BuildingCatalog.Get(definitionId);
        if (definition == null) return;
        BoxCollider hitArea = rootTransform.gameObject.AddComponent<BoxCollider>();
        hitArea.center = new Vector3(0f, colliderSize.y * .5f, 0f);
        hitArea.size = colliderSize;
        BuildingInstance instance = rootTransform.gameObject.AddComponent<BuildingInstance>();
        instance.Initialize(definition);
        instance.isPlayerPlaced = false;
    }

    private void BuildFields()
    {
        Make("مزرعه گندم", PrimitiveType.Cube, new Vector3(-2f, .25f, 10f),
            new Vector3(8f, .18f, 6f), null, new Color(.48f, .5f, .22f));
        for (int row = 0; row < 7; row++)
        for (int col = 0; col < 12; col++)
        {
            float x = -5.3f + col * .6f;
            float z = 7.8f + row * .7f;
            GameObject stalk = Make("خوشه گندم", PrimitiveType.Cylinder, new Vector3(x, .62f, z),
                new Vector3(.055f, .38f, .055f), null, new Color(.78f, .63f, .25f));
            stalk.transform.rotation = Quaternion.Euler(0f, 0f, (col % 2 == 0 ? 7f : -7f));
        }
    }

    private void BuildTreesAndRocks()
    {
        for (int i = 0; i < 38; i++)
        {
            float x = Mathf.Sin(i * 4.3f) * 19f;
            float z = Mathf.Cos(i * 3.2f) * 19f;
            if (Mathf.Abs(x + 14f) < 3f || (x > -1f && x < 11f && z > -7f && z < 10f)) continue;
            Make("تنه درخت", PrimitiveType.Cylinder, new Vector3(x, .75f, z),
                new Vector3(.28f, .75f, .28f), null, new Color(.29f, .18f, .1f));
            Make("تاج درخت", PrimitiveType.Sphere, new Vector3(x, 1.9f, z),
                new Vector3(1.7f, 1.8f, 1.7f), null, new Color(.2f, .34f, .16f));
        }
        for (int i = 0; i < 18; i++)
        {
            float x = Mathf.Sin(i * 2.4f) * 22f;
            float z = Mathf.Cos(i * 1.8f) * 22f;
            Make("صخره کوهستان", PrimitiveType.Sphere, new Vector3(x, .45f, z),
                new Vector3(Random.Range(1.2f, 2.8f), Random.Range(.7f, 1.8f), Random.Range(1.2f, 2.8f)),
                null, new Color(.4f, .37f, .31f));
        }
    }

    private void BuildLighting()
    {
        RenderSettings.ambientLight = new Color(.72f, .68f, .59f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(.67f, .62f, .52f);
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = .008f;
        if (FindObjectOfType<Light>() == null)
        {
            GameObject sun = new GameObject("خورشید دره");
            sun.transform.SetParent(root, false);
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.25f;
            light.color = new Color(1f, .88f, .7f);
            sun.transform.rotation = Quaternion.Euler(48f, -32f, 0f);
        }
    }

    private Transform NewGroup(string title, Vector3 position)
    {
        GameObject group = new GameObject(title);
        group.transform.SetParent(root, false);
        group.transform.localPosition = position;
        return group.transform;
    }

    private GameObject Make(string title, PrimitiveType type, Vector3 position, Vector3 scale, Material material, Color color)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = title;
        obj.transform.SetParent(root, false);
        obj.transform.localPosition = position;
        obj.transform.localScale = scale;
        ApplyMaterial(obj, material, color);
        return obj;
    }

    private GameObject MakeChild(Transform parent, string title, PrimitiveType type, Vector3 position, Vector3 scale, Color color)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = title;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position;
        obj.transform.localScale = scale;
        ApplyMaterial(obj, null, color);
        return obj;
    }

    private void ApplyMaterial(GameObject obj, Material material, Color color)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null) return;
        renderer.sharedMaterial = material != null ? material : MakeMaterial(color);
    }

    private Material MakeMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        Material material = new Material(shader);
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);
        return material;
    }
}
