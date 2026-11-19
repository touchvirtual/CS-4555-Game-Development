using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class BombaCatPortraitBuilder
{
    [MenuItem("Tools/BombaCat/Render Cat Portrait")]
    public static void Build()
    {
        var preview = EditorSceneManager.NewPreviewScene();
        var cat = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Ladymito/Free_cat/Models/cat.fbx"));
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cat, preview);
        cat.transform.localScale = Vector3.one * 3f;
        var animator = cat.GetComponentInChildren<Animator>();
        if (animator != null) animator.enabled = false;
        var head = cat.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.name == "DEF_head");
        var bounds = cat.GetComponentInChildren<SkinnedMeshRenderer>().bounds;
        Vector3 target = head != null ? head.position + Vector3.up * .025f : bounds.center + Vector3.up * bounds.size.y * .2f;
        var camera = new GameObject("Cat portrait camera").AddComponent<Camera>();
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(camera.gameObject, preview);
        camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(preview);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.15f, .12f, .2f, 0f);
        camera.orthographic = true; camera.orthographicSize = .255f;
        camera.nearClipPlane = .01f; camera.farClipPlane = 10f;
        camera.transform.position = target + new Vector3(.08f, .045f, 1.8f);
        camera.transform.LookAt(target);
        camera.allowHDR = false; camera.allowMSAA = true;
        var key = new GameObject("Portrait key").AddComponent<Light>();
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(key.gameObject, preview);
        key.type = LightType.Directional; key.intensity = 1.5f; key.transform.rotation = Quaternion.Euler(30, -25, 0);
        var fill = new GameObject("Portrait fill").AddComponent<Light>();
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(fill.gameObject, preview);
        fill.type = LightType.Directional; fill.intensity = .7f; fill.transform.rotation = Quaternion.Euler(15, 140, 0);
        ShaderUtil.allowAsyncCompilation = false;
        var texture = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
        camera.targetTexture = texture;
        camera.Render();
        var old = RenderTexture.active; RenderTexture.active = texture;
        var pixels = new Texture2D(512, 512, TextureFormat.RGBA32, false);
        pixels.ReadPixels(new Rect(0, 0, 512, 512), 0, 0); pixels.Apply();
        const string path = "Assets/BombaCat/Resources/BombaCatPortrait.png";
        Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllBytes(path, pixels.EncodeToPNG());
        RenderTexture.active = old; camera.targetTexture = null;
        Object.DestroyImmediate(pixels); Object.DestroyImmediate(texture);
        EditorSceneManager.ClosePreviewScene(preview);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
        importer.sRGBTexture = true; importer.maxTextureSize = 512;
        importer.wrapMode = TextureWrapMode.Clamp; importer.SaveAndReimport();
        Debug.Log("Rendered the actual BombaCat portrait.");
    }
}
