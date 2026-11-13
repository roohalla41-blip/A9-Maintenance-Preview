using UnityEngine;

// Modular first-pass blockout for the Panjshir-inspired defensive locations.
// These procedural primitives establish gameplay scale and layout only.
// Production art still requires authored meshes, UVs, PBR textures and animation.
public sealed class PanjshirDefenseKit : MonoBehaviour
{
    Transform root;
    Material mud, stone, sandbag, wood, metal, earth;

    public void Build(Transform parent)
    {
        if (parent == null || parent.Find("PanjshirDefenseLocations") != null) return;
        root = new GameObject("PanjshirDefenseLocations").transform;
        root.SetParent(parent, false);
        mud = Mat(new Color(.56f,.43f,.30f));
        stone = Mat(new Color(.39f,.38f,.34f));
        sandbag = Mat(new Color(.49f,.43f,.32f));
        wood = Mat(new Color(.25f,.16f,.10f));
        metal = Mat(new Color(.24f,.29f,.29f));
        earth = Mat(new Color(.34f,.31f,.25f));
        BuildLookout(new Vector3(-18f,.2f,-12f));
        BuildSandbagPost(new Vector3(13f,.2f,-14f));
        BuildRuralBase(new Vector3(17f,.2f,12f));
        BuildCheckpoint(new Vector3(-4f,.2f,19f));
        ScatterRubble();
    }

    void BuildLookout(Vector3 p)
    {
        Transform g=Group("پوسته دیده‌بانی کوهستانی",p);
        Part(g,"سکوی سنگی",PrimitiveType.Cube,new Vector3(0,.35f,0),new Vector3(5.8f,.7f,4.8f),stone);
        Part(g,"جان‌پناه عقب",PrimitiveType.Cube,new Vector3(0,1.05f,1.85f),new Vector3(5.8f,1.05f,.45f),mud);
        Part(g,"جان‌پناه چپ",PrimitiveType.Cube,new Vector3(-2.7f,1.05f,0),new Vector3(.45f,1.05f,4.1f),mud);
        Part(g,"جان‌پناه راست",PrimitiveType.Cube,new Vector3(2.7f,1.05f,0),new Vector3(.45f,1.05f,4.1f),mud);
        Part(g,"سقف محافظ چوبی",PrimitiveType.Cube,new Vector3(0,2.45f,.65f),new Vector3(3.2f,.18f,2.2f),wood);
        for(int i=0;i<7;i++) Part(g,"تیرک سقف",PrimitiveType.Cylinder,new Vector3(-1.35f+i*.45f,2.25f,-.25f),new Vector3(.09f,.55f,.09f),wood);
        Part(g,"نردبان",PrimitiveType.Cube,new Vector3(-2.15f,.95f,-2.25f),new Vector3(.16f,1.8f,.18f),wood);
        for(int i=0;i<6;i++) Part(g,"پله نردبان",PrimitiveType.Cylinder,new Vector3(-2.15f,.25f+i*.27f,-2.25f),new Vector3(.42f,.055f,.07f),wood);
        Rocks(g,new Vector3(-3f,.2f,-1f),6);
        Rocks(g,new Vector3(3.2f,.2f,1.1f),5);
    }

    void BuildSandbagPost(Vector3 p)
    {
        Transform g=Group("سنگر کیسه‌شنی",p);
        Part(g,"کف خاکی",PrimitiveType.Cube,new Vector3(0,.12f,0),new Vector3(5.5f,.24f,3.8f),earth);
        for(int row=0;row<2;row++) for(int i=0;i<9;i++)
            Part(g,"کیسه شن",PrimitiveType.Sphere,new Vector3(-2.25f+i*.56f,.38f+row*.34f,-1.25f),new Vector3(.68f,.42f,.52f),sandbag);
        for(int row=0;row<2;row++) for(int i=0;i<6;i++)
        {
            float z=-.75f+i*.55f;
            Part(g,"کیسه کناری",PrimitiveType.Sphere,new Vector3(-2.45f,.38f+row*.34f,z),new Vector3(.55f,.42f,.62f),sandbag);
            Part(g,"کیسه کناری",PrimitiveType.Sphere,new Vector3(2.45f,.38f+row*.34f,z),new Vector3(.55f,.42f,.62f),sandbag);
        }
        Part(g,"تیرک پوشش",PrimitiveType.Cylinder,new Vector3(-1.9f,1.45f,.95f),new Vector3(.12f,1.8f,.12f),wood);
        Part(g,"تیرک پوشش",PrimitiveType.Cylinder,new Vector3(1.9f,1.45f,.95f),new Vector3(.12f,1.8f,.12f),wood);
        Part(g,"پوشش سنگر",PrimitiveType.Cube,new Vector3(0,2.25f,.95f),new Vector3(4.2f,.12f,1.5f),sandbag);
        Part(g,"جعبه تدارکات",PrimitiveType.Cube,new Vector3(-1.45f,.42f,1.25f),new Vector3(.85f,.7f,.8f),wood);
        Part(g,"بشکه",PrimitiveType.Cylinder,new Vector3(1.55f,.52f,1.2f),new Vector3(.55f,.9f,.55f),metal);
    }

    void BuildRuralBase(Vector3 p)
    {
        Transform g=Group("پایگاه روستایی خشتی",p);
        Part(g,"حیاط",PrimitiveType.Cube,new Vector3(0,.12f,0),new Vector3(8f,.24f,7f),earth);
        Part(g,"دیوار شمالی",PrimitiveType.Cube,new Vector3(0,1.25f,3.35f),new Vector3(8f,2.5f,.35f),mud);
        Part(g,"دیوار جنوبی",PrimitiveType.Cube,new Vector3(0,1.25f,-3.35f),new Vector3(8f,2.5f,.35f),mud);
        Part(g,"دیوار غربی",PrimitiveType.Cube,new Vector3(-3.85f,1.25f,0),new Vector3(.35f,2.5f,6.4f),mud);
        Part(g,"دیوار شرقی",PrimitiveType.Cube,new Vector3(3.85f,1.25f,0),new Vector3(.35f,2.5f,6.4f),mud);
        Part(g,"دروازه چوبی",PrimitiveType.Cube,new Vector3(0,.95f,-3.4f),new Vector3(1.5f,1.9f,.22f),wood);
        Part(g,"ساختمان اصلی",PrimitiveType.Cube,new Vector3(1.6f,1.3f,1.45f),new Vector3(3.2f,2.6f,2.8f),mud);
        Part(g,"بام مسطح",PrimitiveType.Cube,new Vector3(1.6f,2.68f,1.45f),new Vector3(3.45f,.18f,3.05f),sandbag);
        Part(g,"برجک گوشه",PrimitiveType.Cube,new Vector3(-3.35f,2.2f,2.85f),new Vector3(1.1f,4.4f,1.1f),stone);
        Part(g,"سقف برجک",PrimitiveType.Cube,new Vector3(-3.35f,4.45f,2.85f),new Vector3(1.35f,.25f,1.35f),sandbag);
        Part(g,"انبار",PrimitiveType.Cube,new Vector3(-1.65f,.8f,1.1f),new Vector3(1.8f,1.6f,1.8f),stone);
        Part(g,"محل آتش",PrimitiveType.Cylinder,new Vector3(-1.1f,.28f,-1.2f),new Vector3(.85f,.22f,.85f),stone);
        for(int i=0;i<3;i++) Part(g,"جعبه تدارکات",PrimitiveType.Cube,new Vector3(2.4f+i*.65f,.35f,-1.8f),new Vector3(.6f,.7f,.6f),wood);
    }

    void BuildCheckpoint(Vector3 p)
    {
        Transform g=Group("پاسگاه ورودی دره",p);
        Part(g,"جاده خاکی",PrimitiveType.Cube,new Vector3(0,.06f,0),new Vector3(13f,.12f,3.2f),earth);
        Part(g,"دیوار سنگی چپ",PrimitiveType.Cube,new Vector3(-4f,.8f,2.3f),new Vector3(4.5f,1.6f,.7f),stone);
        Part(g,"دیوار سنگی راست",PrimitiveType.Cube,new Vector3(4f,.8f,2.3f),new Vector3(4.5f,1.6f,.7f),stone);
        Part(g,"اتاقک نگهبانی",PrimitiveType.Cube,new Vector3(0,1.25f,3.8f),new Vector3(3.2f,2.5f,2.4f),mud);
        Part(g,"سقف اتاقک",PrimitiveType.Cube,new Vector3(0,2.58f,3.8f),new Vector3(3.5f,.18f,2.7f),wood);
        Part(g,"مانع ورودی",PrimitiveType.Cube,new Vector3(0,.95f,-.2f),new Vector3(.22f,1.9f,3.2f),wood);
        Part(g,"نوار فلزی مانع",PrimitiveType.Cube,new Vector3(0,1.5f,-.2f),new Vector3(.3f,.12f,3.2f),metal);
        Rocks(g,new Vector3(6f,.1f,4f),8);
    }

    void ScatterRubble()
    {
        for(int i=0;i<24;i++)
        {
            float x=Mathf.Sin(i*2.17f)*23f, z=Mathf.Cos(i*1.71f)*23f;
            Part(root,"سنگ و آوار",PrimitiveType.Sphere,new Vector3(x,.16f,z),
                new Vector3(.25f+(i%3)*.16f,.18f,.22f+(i%4)*.12f),i%2==0?stone:earth);
        }
    }

    void Rocks(Transform parent,Vector3 p,int count)
    {
        for(int i=0;i<count;i++)
            Part(parent,"سنگ طبیعی",PrimitiveType.Sphere,p+new Vector3(Mathf.Sin(i*2.3f)*.8f,.15f,Mathf.Cos(i*1.7f)*.7f),
                new Vector3(.55f+(i%3)*.25f,.35f+(i%2)*.22f,.5f+(i%4)*.16f),i%2==0?stone:earth);
    }

    Transform Group(string name,Vector3 position)
    {
        Transform t=new GameObject(name).transform; t.SetParent(root,false); t.localPosition=position; return t;
    }

    GameObject Part(Transform parent,string name,PrimitiveType type,Vector3 pos,Vector3 scale,Material material)
    {
        GameObject o=GameObject.CreatePrimitive(type); o.name=name; o.transform.SetParent(parent,false);
        o.transform.localPosition=pos; o.transform.localScale=scale;
        Renderer r=o.GetComponent<Renderer>(); if(r!=null) r.sharedMaterial=material;
        Collider c=o.GetComponent<Collider>(); if(c!=null) Destroy(c);
        return o;
    }

    // Shared tileable surface maps add material breakup without allocating one
    // texture per prop. This is still a blockout material pass, not final authored PBR.
    Material Mat(Color color)
    {
        bool usingScriptablePipeline = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;
        Shader s = usingScriptablePipeline
            ? Shader.Find("Universal Render Pipeline/Lit")
            : Shader.Find("Standard");
        if(s==null) s=Shader.Find("Universal Render Pipeline/Lit");
        Material m=new Material(s);
        m.color=Color.white;
        Texture2D texture=BuildSurfaceTexture(color, SurfaceSeed(color), 128);
        texture.name="Panjshir surface "+ColorUtility.ToHtmlStringRGB(color);
        texture.wrapMode=TextureWrapMode.Repeat;
        texture.filterMode=FilterMode.Bilinear;
        if(m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap",texture);
        if(m.HasProperty("_MainTex")) m.SetTexture("_MainTex",texture);
        if(m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness",.12f);
        if(m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness",.12f);
        if(m.HasProperty("_Metallic")) m.SetFloat("_Metallic",.03f);
        return m;
    }

    int SurfaceSeed(Color c)
    {
        return Mathf.RoundToInt(c.r*997f+c.g*313f+c.b*127f);
    }

    Texture2D BuildSurfaceTexture(Color baseColor,int seed,int resolution)
    {
        Texture2D tex=new Texture2D(resolution,resolution,TextureFormat.RGB24,false);
        tex.anisoLevel=2;
        // Deterministic hash noise keeps the same material stable between launches.
        for(int y=0;y<resolution;y++)
        for(int x=0;x<resolution;x++)
        {
            float broad=HashNoise(x/8,y/8,seed);
            float fine=HashNoise(x,y,seed+71);
            float grain=(broad-.5f)*.22f+(fine-.5f)*.16f;
            float streak=0f;
            if(baseColor.r<.35f && baseColor.g<.25f)
                streak=(HashNoise(x/3,y/13,seed+19)-.5f)*.18f; // dark wood grain
            float shade=Mathf.Clamp(1f+grain+streak,.72f,1.25f);
            Color pixel=new Color(baseColor.r*shade,baseColor.g*shade,baseColor.b*shade,1f);
            tex.SetPixel(x,y,pixel);
        }
        tex.Apply(false,true);
        return tex;
    }

    float HashNoise(int x,int y,int seed)
    {
        unchecked
        {
            int n=x*374761393+y*668265263+seed*1442695041;
            n=(n^(n>>13))*1274126177;
            n^=n>>16;
            return (n&0x7fffffff)/(float)int.MaxValue;
        }
    }
}