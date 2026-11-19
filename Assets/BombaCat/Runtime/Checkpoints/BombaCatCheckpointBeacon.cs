using UnityEngine;
using TMPro;

/// <summary>A visible cat bowl, glowing rings and light column mark the safe place.</summary>
public sealed class BombaCatCheckpointBeacon : MonoBehaviour
{
    public bool isEntrance;
    private Transform visual;
    private LineRenderer halo, beam;
    private Light glow;
    private TMP_Text label;
    private Material bowlMaterial, insideMaterial, lightMaterial;
    private bool saved;
    private void Start()
    {
        Vector3 position = transform.position + (isEntrance ? Vector3.right * 1.5f : Vector3.left * 1.8f);
        float nearest=float.PositiveInfinity;
        foreach (var hit in Physics.RaycastAll(position + Vector3.up*5f,Vector3.down,12f,~0,QueryTriggerInteraction.Ignore))
            if (hit.distance<nearest && hit.normal.y>.6f && hit.collider.GetComponentInParent<PlayerController>()==null && hit.collider.GetComponentInParent<EnemyHealth>()==null)
            { nearest=hit.distance; position.y=hit.point.y+.06f; }
        visual=new GameObject("Glowing checkpoint bowl").transform;
        visual.SetParent(transform,false); visual.position=position;
        Shader shader=Shader.Find(UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline!=null?"Universal Render Pipeline/Lit":"Standard");
        bowlMaterial=new Material(shader); bowlMaterial.color=new Color(.58f,.31f,.12f);
        insideMaterial=new Material(shader); insideMaterial.color=new Color(.12f,.09f,.08f);
        lightMaterial=new Material(Shader.Find("Sprites/Default"));
        Cylinder("Cat bowl",new Vector3(0,.09f,0),new Vector3(.8f,.1f,.8f),bowlMaterial);
        Cylinder("Bowl interior",new Vector3(0,.195f,0),new Vector3(.61f,.012f,.61f),insideMaterial);
        halo=Line("Checkpoint halo",.055f,65);
        for(int i=0;i<65;i++) {float a=i*Mathf.PI*2f/64; halo.SetPosition(i,new Vector3(Mathf.Cos(a)*.65f,.04f,Mathf.Sin(a)*.65f));}
        var rim=Line("Bowl rim",.05f,65);
        for(int i=0;i<65;i++) {float a=i*Mathf.PI*2f/64; rim.SetPosition(i,new Vector3(Mathf.Cos(a)*.36f,.2f,Mathf.Sin(a)*.36f));}
        rim.startColor=rim.endColor=new Color(1f,.8f,.4f);
        beam=Line("Checkpoint light",.28f,2); beam.SetPosition(0,Vector3.up*.22f); beam.SetPosition(1,Vector3.up*3.1f);
        beam.startWidth=.32f; beam.endWidth=.04f;
        glow=new GameObject("Checkpoint glow").AddComponent<Light>(); glow.transform.SetParent(visual,false);
        glow.transform.localPosition=Vector3.up*.5f; glow.type=LightType.Point; glow.range=5f;
        label=new GameObject("Checkpoint label").AddComponent<TextMeshPro>(); label.transform.SetParent(visual,false);
        label.transform.localPosition=Vector3.up*1.3f; label.rectTransform.sizeDelta=new Vector2(3f,.65f);
        label.fontSize=2.2f; label.alignment=TextAlignmentOptions.Center; label.fontStyle=FontStyles.Bold;
        label.text="CHECKPOINT";
        if(TMP_Settings.defaultFontAsset!=null) label.font=TMP_Settings.defaultFontAsset;
    }
    private void Cylinder(string name,Vector3 position,Vector3 scale,Material material)
    {
        var part=GameObject.CreatePrimitive(PrimitiveType.Cylinder); part.name=name; part.transform.SetParent(visual,false);
        part.transform.localPosition=position; part.transform.localScale=scale;
        var collider=part.GetComponent<Collider>(); collider.enabled=false; Destroy(collider);
        part.GetComponent<Renderer>().sharedMaterial=material;
    }
    private LineRenderer Line(string name,float width,int count)
    {
        var line=new GameObject(name).AddComponent<LineRenderer>(); line.transform.SetParent(visual,false);
        line.useWorldSpace=false; line.sharedMaterial=lightMaterial; line.positionCount=count;
        line.startWidth=line.endWidth=width; line.numCornerVertices=4; line.numCapVertices=4;
        line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off; return line;
    }
    private void Update()
    {
        if(visual==null) return;
        saved=isEntrance || BombaCatCheckpointService.HasCheckpoint;
        Color color=saved?new Color(.35f,1f,.75f):new Color(1f,.72f,.25f);
        float pulse=.75f+.25f*Mathf.Sin(Time.time*2.5f);
        halo.startColor=halo.endColor=color;
        beam.startColor=new Color(color.r,color.g,color.b,.3f*pulse); beam.endColor=new Color(color.r,color.g,color.b,0f);
        glow.color=color; glow.intensity=1.1f*pulse;
        label.color=color; label.text=saved?"CHECKPOINT\nSAVED":"CHECKPOINT";
        var camera=Camera.main; if(camera!=null) label.transform.rotation=camera.transform.rotation;
    }
    private void OnDestroy()
    { if(bowlMaterial!=null) Destroy(bowlMaterial); if(insideMaterial!=null) Destroy(insideMaterial); if(lightMaterial!=null) Destroy(lightMaterial); }
}
