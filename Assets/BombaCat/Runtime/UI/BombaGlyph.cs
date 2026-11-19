using UnityEngine;
using UnityEngine.UI;

/// <summary>Original vector art, rendered at the current canvas resolution.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class BombaGlyph : MaskableGraphic
{
    public enum Kind { Tile, Cat, Bomb, Paw, Circle, Ring }
    public Kind kind;
    public float radius = 14f;
    [Range(0f, 1f)] public float fillAmount = 1f;
    [Range(.01f, .9f)] public float ringThickness = .12f;
    [Range(1, 40)] public int segments = 1;
    public float gapDegrees;
    public bool useGradient;
    public Color endColor = Color.white;

    public void SetFill(float value)
    {
        value = Mathf.Clamp01(value);
        if (Mathf.Abs(fillAmount - value) < .0001f) return;
        fillAmount = value;
        SetVerticesDirty();
    }

    public void SetSegments(int count)
    {
        count = Mathf.Clamp(count, 1, 40);
        if (segments == count) return;
        segments = count;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        if (kind == Kind.Tile) { Rounded(mesh, GetPixelAdjustedRect(), color, radius); return; }
        if (kind == Kind.Circle) { Disc(mesh, .5f, .5f, .5f, color, 72); return; }
        if (kind == Kind.Ring) { Ring(mesh); return; }
        Color ink = new Color32(25,22,36,255);
        if (kind == Kind.Cat)
        {
            Polygon(mesh,color,.14f,.56f,.12f,.96f,.46f,.73f);
            Polygon(mesh,color,.54f,.73f,.88f,.96f,.86f,.56f);
            Disc(mesh,.5f,.43f,.36f,color);
            Disc(mesh,.36f,.47f,.045f,ink); Disc(mesh,.64f,.47f,.045f,ink);
            Polygon(mesh,new Color32(255,115,107,255),.44f,.34f,.56f,.34f,.5f,.27f);
            Polygon(mesh,color,.01f,.37f,.31f,.34f,.31f,.31f,.01f,.33f);
            Polygon(mesh,color,.69f,.34f,.99f,.37f,.99f,.33f,.69f,.31f);
        }
        else if (kind == Kind.Bomb)
        {
            Disc(mesh,.47f,.4f,.34f,color);
            Polygon(mesh,color,.43f,.66f,.59f,.66f,.59f,.79f,.43f,.79f);
            Polygon(mesh,new Color32(255,242,216,255),.52f,.77f,.56f,.86f,.72f,.89f,.72f,.85f,.59f,.82f,.57f,.76f);
            Disc(mesh,.75f,.88f,.055f,new Color32(255,201,104,255));
            Disc(mesh,.35f,.51f,.065f,new Color32(255,242,216,200));
        }
        else
        {
            Disc(mesh,.5f,.34f,.25f,color); Disc(mesh,.22f,.58f,.12f,color);
            Disc(mesh,.4f,.78f,.12f,color); Disc(mesh,.64f,.77f,.12f,color); Disc(mesh,.8f,.56f,.12f,color);
        }
    }
    private Vector2 Point(float x,float y)
    {
        var r = GetPixelAdjustedRect(); return new Vector2(r.xMin+x*r.width,r.yMin+y*r.height);
    }
    private void Disc(VertexHelper mesh,float x,float y,float r,Color tint,int n=28)
    {
        int start = mesh.currentVertCount;
        mesh.AddVert(Point(x,y),tint,Vector2.zero);
        for(int i=0;i<n;i++) { float a=i*Mathf.PI*2f/n; mesh.AddVert(Point(x+Mathf.Cos(a)*r,y+Mathf.Sin(a)*r),tint,Vector2.zero); }
        for(int i=0;i<n;i++) mesh.AddTriangle(start,start+1+i,start+1+(i+1)%n);
    }
    // Clockwise from twelve o'clock; each health segment keeps a visible gap.
    private void Ring(VertexHelper mesh)
    {
        Rect bounds = GetPixelAdjustedRect();
        float outer = Mathf.Min(bounds.width, bounds.height) * .5f;
        float inner = outer * (1f - Mathf.Clamp(ringThickness, .01f, .9f));
        float sweep = Mathf.Clamp01(fillAmount) * 360f;
        int count = Mathf.Clamp(segments, 1, 40);
        float segmentSweep = 360f / count;
        float halfGap = Mathf.Clamp(gapDegrees, 0f, segmentSweep * .8f) * .5f;
        for (int segment = 0; segment < count; segment++)
        {
            float from = segment * segmentSweep + halfGap;
            float to = Mathf.Min((segment + 1) * segmentSweep - halfGap, sweep);
            if (to <= from) continue;
            int steps = Mathf.Max(1, Mathf.CeilToInt((to - from) / 4f));
            for (int i = 0; i < steps; i++)
            {
                float a = Mathf.Lerp(from, to, (float)i / steps);
                float b = Mathf.Lerp(from, to, (float)(i + 1) / steps);
                Vector2 av = new Vector2(Mathf.Cos((90f-a)*Mathf.Deg2Rad), Mathf.Sin((90f-a)*Mathf.Deg2Rad));
                Vector2 bv = new Vector2(Mathf.Cos((90f-b)*Mathf.Deg2Rad), Mathf.Sin((90f-b)*Mathf.Deg2Rad));
                Color ac = useGradient ? Color.Lerp(color, endColor, a / 360f) : color;
                Color bc = useGradient ? Color.Lerp(color, endColor, b / 360f) : color;
                int start = mesh.currentVertCount;
                mesh.AddVert(bounds.center + av*outer, ac, Vector2.zero);
                mesh.AddVert(bounds.center + bv*outer, bc, Vector2.zero);
                mesh.AddVert(bounds.center + bv*inner, bc, Vector2.zero);
                mesh.AddVert(bounds.center + av*inner, ac, Vector2.zero);
                mesh.AddTriangle(start, start+1, start+2);
                mesh.AddTriangle(start, start+2, start+3);
            }
        }
    }
    private void Polygon(VertexHelper mesh,Color tint,params float[] xy)
    {
        int start=mesh.currentVertCount;
        for(int i=0;i<xy.Length;i+=2) mesh.AddVert(Point(xy[i],xy[i+1]),tint,Vector2.zero);
        for(int i=1;i<xy.Length/2-1;i++) mesh.AddTriangle(start,start+i,start+i+1);
    }
    private static void Rounded(VertexHelper mesh,Rect rect,Color tint,float amount)
    {
        if(rect.width<=0 || rect.height<=0) return;
        float r=Mathf.Clamp(amount,0,Mathf.Min(rect.width,rect.height)*.5f);
        int start=mesh.currentVertCount; mesh.AddVert(rect.center,tint,Vector2.zero);
        const int steps=6;
        for(int corner=0;corner<4;corner++)
        {
            Vector2 center=corner==0?new Vector2(rect.xMax-r,rect.yMax-r):corner==1?new Vector2(rect.xMin+r,rect.yMax-r):corner==2?new Vector2(rect.xMin+r,rect.yMin+r):new Vector2(rect.xMax-r,rect.yMin+r);
            for(int i=0;i<=steps;i++) { float a=(corner*90f+i*90f/steps)*Mathf.Deg2Rad; mesh.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,tint,Vector2.zero); }
        }
        int count=4*(steps+1);
        for(int i=0;i<count;i++) mesh.AddTriangle(start,start+1+i,start+1+(i+1)%count);
    }
}
