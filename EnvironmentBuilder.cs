using UnityEngine;

public class EnvironmentBuilder : MonoBehaviour
{
    public void Build(int size, Material groundMat, Material waterMat)
    {
        Make("دشت دهکده", PrimitiveType.Cube, new Vector3(0, -0.6f, 0),
            new Vector3(size, 1, size), groundMat, new Color(0.55f, 0.48f, 0.32f));

        Make("رودخانه", PrimitiveType.Cube, new Vector3(-14, -0.02f, 0),
            new Vector3(3.5f, 0.12f, size), waterMat, new Color(0.12f, 0.40f, 0.60f));

        Make("جاده مرکزی", PrimitiveType.Cube, new Vector3(1, 0.02f, -1),
            new Vector3(2.5f, 0.08f, 31), null, new Color(0.68f, 0.55f, 0.37f));

        // تپه‌های پیرامونی
        for (int i = 0; i < 16; i++)
        {
            float x = Mathf.Sin(i * 1.71f) * 23f;
            float z = Mathf.Cos(i * 2.13f) * 23f;
            GameObject hill = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hill.name = "تپه‌های پیرامونی";
            hill.transform.position = new Vector3(x, -0.25f, z);
            hill.transform.localScale = new Vector3(9, 4, 8);
            Tint(hill, new Color(0.43f, 0.38f, 0.29f));
        }

        // ساختمان‌های آغازین
        Building("قصر فرمانروایی", new Vector3(3, 1.0f, 3),
            new Vector3(5, 2.8f, 5), new Color(0.72f, 0.61f, 0.44f));
        Building("خانه دهکده", new Vector3(8, 0.65f, -3),
            new Vector3(3.2f, 1.8f, 3.2f), new Color(0.65f, 0.43f, 0.29f));
        Building("انبار منابع", new Vector3(8, 0.65f, 3),
            new Vector3(3.3f, 1.8f, 3.3f), new Color(0.52f, 0.39f, 0.25f));
        Building("سربازخانه", new Vector3(-2, 0.7f, -5),
            new Vector3(4.2f, 2.0f, 3.5f), new Color(0.48f, 0.35f, 0.28f));

        Make("مزرعه گندم", PrimitiveType.Cube, new Vector3(-2, 0.10f, 9),
            new Vector3(8, 0.22f, 5), null, new Color(0.52f, 0.57f, 0.25f));

        // درختان ساده
        for (int i = 0; i < 28; i++)
        {
            float x = Mathf.Sin(i * 4.3f) * 17f;
            float z = Mathf.Cos(i * 3.2f) * 17f;
            if (Mathf.Abs(x + 14) < 3f) continue;
            Make("تنه درخت", PrimitiveType.Cylinder, new Vector3(x, 0.7f, z),
                new Vector3(0.35f, 0.7f, 0.35f), null, new Color(0.30f, 0.19f, 0.10f));
            Make("تاج درخت", PrimitiveType.Sphere, new Vector3(x, 1.7f, z),
                new Vector3(1.5f, 1.7f, 1.5f), null, new Color(0.20f, 0.38f, 0.18f));
        }

        RenderSettings.ambientLight = new Color(0.70f, 0.68f, 0.60f);
        if (FindObjectOfType<Light>() == null)
        {
            GameObject sun = new GameObject("نور خورشید");
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            sun.transform.rotation = Quaternion.Euler(48, -32, 0);
        }
    }

    private void Building(string title, Vector3 position, Vector3 scale, Color color)
    {
        Make(title, PrimitiveType.Cube, position, scale, null, color);
        Make(title + " سقف", PrimitiveType.Cube,
            position + Vector3.up * (scale.y * 0.62f),
            new Vector3(scale.x * 1.08f, 0.45f, scale.z * 1.08f),
            null, new Color(0.36f, 0.20f, 0.15f));
    }

    private GameObject Make(string title, PrimitiveType type, Vector3 position,
        Vector3 scale, Material material, Color fallback)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = title;
        obj.transform.position = position;
        obj.transform.localScale = scale;
        Renderer renderer = obj.GetComponent<Renderer>();
        if (material != null) renderer.material = material;
        else renderer.material.color = fallback;
        return obj;
    }

    private void Tint(GameObject obj, Color color)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null) renderer.material.color = color;
    }
}
