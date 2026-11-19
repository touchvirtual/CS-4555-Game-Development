using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public sealed class BombaCatMapGraphic : MaskableGraphic
{
    public bool arrow;
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect r = GetPixelAdjustedRect();
        if (arrow)
        {
            mesh.AddVert(new Vector2(r.center.x, r.yMax), color, Vector2.zero);
            mesh.AddVert(new Vector2(r.xMin, r.yMin), color, Vector2.zero);
            mesh.AddVert(new Vector2(r.center.x, r.yMin + r.height * .25f), color, Vector2.zero);
            mesh.AddVert(new Vector2(r.xMax, r.yMin), color, Vector2.zero);
            mesh.AddTriangle(0, 1, 2); mesh.AddTriangle(0, 2, 3);
            return;
        }
        mesh.AddVert(r.center, color, Vector2.zero);
        for (int i = 0; i < 64; i++)
        {
            float a = i * Mathf.PI / 32f;
            mesh.AddVert(r.center + new Vector2(Mathf.Cos(a) * r.width, Mathf.Sin(a) * r.height) * .5f, color, Vector2.zero);
        }
        for (int i = 0; i < 64; i++) mesh.AddTriangle(0, i + 1, (i + 1) % 64 + 1);
    }
}
