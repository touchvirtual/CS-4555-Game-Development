#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class BombaCatRigInspect
{
    public static void Inspect()
    {
        const string path = "Assets/Ladymito/Free_cat/Models/cat.fbx";
        var sb = new StringBuilder();
        var importer = (ModelImporter)AssetImporter.GetAtPath(path);
        foreach (var c in importer.defaultClipAnimations)
            sb.AppendLine("TAKE " + c.name + " " + c.firstFrame + ".." + c.lastFrame);
        var clips = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().ToArray();
        foreach (var c in clips) sb.AppendLine("CLIP " + c.name + " " + c.length);
        var cat = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        var idle = clips.First(c => c.name.EndsWith("|idle"));
        idle.SampleAnimation(cat, 0f);
        cat.transform.localScale = Vector3.one * 3f;
        foreach (var b in cat.GetComponentsInChildren<Transform>())
        {
            if (!b.name.StartsWith("DEF_") && b.name != "root" && b.name != "cat_armature") continue;
            sb.AppendLine(b.name + " pos=" + b.position.ToString("F5") + " right=" + b.right.ToString("F4") + " up=" + b.up.ToString("F4") + " forward=" + b.forward.ToString("F4") + " local=" + b.localRotation.ToString("F4") + " path=" + AnimationUtility.CalculateTransformPath(b, cat.transform));
        }
        Object.DestroyImmediate(cat);
        string destination = Path.GetFullPath(Path.Combine(Application.dataPath, "../../rig-inspection.txt"));
        File.WriteAllText(destination, sb.ToString());
        Debug.Log("Rig inspection: " + destination);
    }
}
#endif
