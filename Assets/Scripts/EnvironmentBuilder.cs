using UnityEngine;

public class EnvironmentBuilder : MonoBehaviour
{
    public void Build(int size, Material groundMat, Material waterMat)
    {
        if (transform.Find("GeneratedVillage") != null) return;
        GameObject root = new GameObject("GeneratedVillage");
        root.transform.SetParent(transform, false);

        Make(root.transform, "دشت دهکده", PrimitiveType.Cube, new Vector3(0,-0.6f,0), new Vector3(size,1,size), groundMat, new Color(.55f,.48f,.32f));
        Make(root.transform, "رودخانه", PrimitiveType.Cube, new Vector3(-14,-.02f,0), new Vector3(3.5f,.12f,size), waterMat, new Color(.12f,.4f,.6f));
        Make(root.transform, "جاده مرکزی", PrimitiveType.Cube, new Vector3(1,.02f,-1), new Vector3(2.5f,.08f,31), null, new Color(.68f,.55f,.37f));

        for (int i=0;i<16;i++)
        {
            float x=Mathf.Sin(i*1.71f)*23f, z=Mathf.Cos(i*2.13f)*23f;
            Make(root.transform,"تپه‌های پیرامونی",PrimitiveType.Sphere,new Vector3(x,-.25f,z),new Vector3(9,4,8),null,new Color(.43f,.38f,.29f));
        }

        Building(root.transform,"قصر فرمانروایی",new Vector3(3,1,3),new Vector3(5,2.8f,5),new Color(.72f,.61f,.44f));
        Building(root.transform,"خانه دهکده",new Vector3(8,.65f,-3),new Vector3(3.2f,1.8f,3.2f),new Color(.65f,.43f,.29f));
        Building(root.transform,"انبار منابع",new Vector3(8,.65f,3),new Vector3(3.3f,1.8f,3.3f),new Color(.52f,.39f,.25f));
        Building(root.transform,"سربازخانه",new Vector3(-2,.7f,-5),new Vector3(4.2f,2,3.5f),new Color(.48f,.35f,.28f));
        Make(root.transform,"مزرعه گندم",PrimitiveType.Cube,new Vector3(-2,.1f,9),new Vector3(8,.22f,5),null,new Color(.52f,.57f,.25f));

        for (int i=0;i<28;i++)
        {
            float x=Mathf.Sin(i*4.3f)*17f, z=Mathf.Cos(i*3.2f)*17f;
            if (Mathf.Abs(x+14f)<3f) continue;
            Make(root.transform,"تنه درخت",PrimitiveType.Cylinder,new Vector3(x,.7f,z),new Vector3(.35f,.7f,.35f),null,new Color(.3f,.19f,.1f));
            Make(root.transform,"تاج درخت",PrimitiveType.Sphere,new Vector3(x,1.7f,z),new Vector3(1.5f,1.7f,1.5f),null,new Color(.2f,.38f,.18f));
        }

        RenderSettings.ambientLight = new Color(.7f,.68f,.6f);
        if (FindObjectOfType<Light>() == null)
        {
            GameObject sun = new GameObject("نور خورشید");
            sun.transform.SetParent(root.transform,false);
            Light light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            sun.transform.rotation = Quaternion.Euler(48,-32,0);
        }
    }

    private void Building(Transform parent,string title,Vector3 position,Vector3 scale,Color color)
    {
        Make(parent,title,PrimitiveType.Cube,position,scale,null,color);
        Make(parent,title+" سقف",PrimitiveType.Cube,position+Vector3.up*(scale.y*.62f),new Vector3(scale.x*1.08f,.45f,scale.z*1.08f),null,new Color(.36f,.2f,.15f));
    }

    private GameObject Make(Transform parent,string title,PrimitiveType type,Vector3 position,Vector3 scale,Material material,Color fallback)
    {
        GameObject obj=GameObject.CreatePrimitive(type);
        obj.name=title;
        obj.transform.SetParent(parent,true);
        obj.transform.position=position;
        obj.transform.localScale=scale;
        Renderer renderer=obj.GetComponent<Renderer>();
        if (material!=null) renderer.sharedMaterial=material;
        else renderer.material.color=fallback;
        return obj;
    }
}