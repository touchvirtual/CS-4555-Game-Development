using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>North-up terrain map. Edge dots indicate enemies beyond the local map radius.</summary>
public sealed class BombaCatMinimap : MonoBehaviour
{
    [Min(8f)] public float mapRadius = 28f;
    [Range(2f, 15f)] public float refreshRate = 6f;
    private RectTransform viewport, playerMarker;
    private PlayerController player;
    private Camera mapCamera;
    private RenderTexture texture;
    private float nextRender, nextScan;
    private readonly Dictionary<EnemyHealth, RectTransform> markers = new Dictionary<EnemyHealth, RectTransform>();
    private readonly List<EnemyHealth> remove = new List<EnemyHealth>();
    public RenderTexture MapTexture => texture;
    public int EnemyMarkerCount => markers.Count;

    public void Build(RectTransform parent)
    {
        if (mapCamera != null) return;
        viewport = new GameObject("Terrain mask", typeof(RectTransform), typeof(BombaCatMapGraphic), typeof(Mask)).GetComponent<RectTransform>();
        viewport.SetParent(parent, false);
        viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one;
        viewport.offsetMin = viewport.offsetMax = Vector2.zero;
        viewport.GetComponent<Mask>().showMaskGraphic = false;
        viewport.GetComponent<BombaCatMapGraphic>().raycastTarget = false;
        var raw = new GameObject("Terrain", typeof(RectTransform), typeof(RawImage)).GetComponent<RawImage>();
        raw.transform.SetParent(viewport, false);
        raw.rectTransform.anchorMin = Vector2.zero; raw.rectTransform.anchorMax = Vector2.one;
        raw.rectTransform.offsetMin = raw.rectTransform.offsetMax = Vector2.zero;
        raw.raycastTarget = false;
        texture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32) { name = "BombaCat local terrain", filterMode = FilterMode.Bilinear };
        texture.Create(); raw.texture = texture;
        mapCamera = new GameObject("BombaCat minimap camera").AddComponent<Camera>();
        mapCamera.enabled = false; mapCamera.orthographic = true;
        mapCamera.orthographicSize = mapRadius; mapCamera.aspect = 1f;
        mapCamera.nearClipPlane = .1f; mapCamera.farClipPlane = 220f;
        mapCamera.clearFlags = CameraClearFlags.SolidColor;
        mapCamera.backgroundColor = new Color(.12f, .18f, .18f);
        mapCamera.cullingMask = ~(1 << 5); // World health bars live on the UI layer.
        mapCamera.allowHDR = false; mapCamera.allowMSAA = false;
        mapCamera.targetTexture = texture;
        mapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        playerMarker = Marker("You", new Color32(156, 255, 216, 255), 15f, true);
        var north = new GameObject("North", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        north.transform.SetParent(parent, false);
        north.rectTransform.anchorMin = north.rectTransform.anchorMax = new Vector2(.5f, 1f);
        north.rectTransform.sizeDelta = new Vector2(26, 22); north.rectTransform.anchoredPosition = new Vector2(0, -12);
        north.text = "N"; north.fontSize = 12; north.fontStyle = FontStyles.Bold;
        north.alignment = TextAlignmentOptions.Center; north.raycastTarget = false;
        north.color = new Color32(255, 242, 216, 255);
        if (TMP_Settings.defaultFontAsset != null) north.font = TMP_Settings.defaultFontAsset;
    }

    private RectTransform Marker(string label, Color color, float size, bool arrow)
    {
        var marker = new GameObject(label, typeof(RectTransform), typeof(BombaCatMapGraphic)).GetComponent<RectTransform>();
        marker.SetParent(viewport, false); marker.anchorMin = marker.anchorMax = new Vector2(.5f, .5f);
        marker.sizeDelta = new Vector2(size, size);
        var graphic = marker.GetComponent<BombaCatMapGraphic>(); graphic.color = color; graphic.arrow = arrow; graphic.raycastTarget = false;
        return marker;
    }

    private void LateUpdate()
    {
        if (mapCamera == null || viewport == null) return;
        if (Time.unscaledTime >= nextScan)
        {
            nextScan = Time.unscaledTime + .5f;
            if (player == null) player = FindAnyObjectByType<PlayerController>();
            foreach (var enemy in FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))
                if (enemy.CurrentHealth > 0 && !markers.ContainsKey(enemy))
                    markers.Add(enemy, Marker("Enemy marker", new Color32(255, 102, 92, 255), 8f, false));
            remove.Clear();
            foreach (var pair in markers)
                if (pair.Key == null || !pair.Key.gameObject.activeInHierarchy || pair.Key.CurrentHealth <= 0) remove.Add(pair.Key);
            foreach (var key in remove) { if (markers[key] != null) Destroy(markers[key].gameObject); markers.Remove(key); }
        }
        if (player == null) return;
        float radius = Mathf.Min(viewport.rect.width, viewport.rect.height) * .5f - 8f;
        foreach (var pair in markers)
        {
            if (pair.Key == null) continue;
            Vector3 delta = pair.Key.transform.position - player.transform.position;
            Vector2 point = new Vector2(delta.x, delta.z) / Mathf.Max(1f, mapRadius) * (radius + 8f);
            bool outside = point.sqrMagnitude > radius * radius;
            pair.Value.anchoredPosition = Vector2.ClampMagnitude(point, radius);
            pair.Value.GetComponent<BombaCatMapGraphic>().color = outside ? new Color(1f, .65f, .3f, .9f) : new Color(1f, .36f, .32f);
        }
        playerMarker.localRotation = Quaternion.Euler(0, 0, -player.transform.eulerAngles.y);
        playerMarker.SetAsLastSibling();
        if (Time.unscaledTime < nextRender) return;
        nextRender = Time.unscaledTime + 1f / Mathf.Max(1f, refreshRate);
        mapCamera.transform.position = player.transform.position + Vector3.up * 100f;
        mapCamera.orthographicSize = mapRadius;
        mapCamera.Render();
    }

    private void OnDestroy()
    {
        if (mapCamera != null) Destroy(mapCamera.gameObject);
        if (texture != null) { texture.Release(); Destroy(texture); }
    }
}
