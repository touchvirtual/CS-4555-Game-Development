using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BombaCatEnemyBar : MonoBehaviour
{
    private EnemyHealth health;
    private RectTransform root, fill;
    private Canvas canvas;
    private TMP_Text number;
    private Renderer[] renderers;
    private Camera cameraRef;
    private int lastHealth = -1;
    public static void Ensure(EnemyHealth enemy)
    {
        if (enemy != null && enemy.GetComponent<BombaCatEnemyBar>() == null)
            enemy.gameObject.AddComponent<BombaCatEnemyBar>();
    }
    private void Start()
    {
        health = GetComponent<EnemyHealth>();
        renderers = GetComponentsInChildren<Renderer>();
        root = new GameObject("Enemy vitality", typeof(RectTransform), typeof(Canvas)).GetComponent<RectTransform>();
        root.gameObject.layer = 5; root.sizeDelta = new Vector2(90, 24);
        root.localScale = Vector3.one * .012f;
        canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace; canvas.sortingOrder = 5;
        var track = Image(root, new Color32(26, 22, 34, 245), new Vector2(90, 8), new Vector2(0, -6));
        fill = Image(track.rectTransform, new Color32(255, 113, 104, 255), new Vector2(86, 4), Vector2.zero).rectTransform;
        fill.pivot = new Vector2(0, .5f); fill.anchorMin = fill.anchorMax = new Vector2(0, .5f); fill.anchoredPosition = new Vector2(2, 0);
        number = new GameObject("Health", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        number.gameObject.layer = 5; number.transform.SetParent(root, false);
        number.rectTransform.sizeDelta = new Vector2(90, 17); number.rectTransform.anchoredPosition = new Vector2(0, 8);
        number.fontSize = 13; number.alignment = TextAlignmentOptions.Center; number.color = new Color32(255, 242, 216, 255);
        number.fontStyle = FontStyles.Bold; number.raycastTarget = false;
        if (TMP_Settings.defaultFontAsset != null) number.font = TMP_Settings.defaultFontAsset;
    }
    private static Image Image(Transform parent, Color color, Vector2 size, Vector2 position)
    {
        var image = new GameObject("Bar", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        image.gameObject.layer = 5; image.transform.SetParent(parent, false);
        image.rectTransform.sizeDelta = size; image.rectTransform.anchoredPosition = position;
        image.color = color; image.raycastTarget = false; return image;
    }
    private void LateUpdate()
    {
        if (health == null || root == null) return;
        if (cameraRef == null) cameraRef = Camera.main;
        if (cameraRef == null) return;
        bool visible = health.CurrentHealth > 0 && Vector3.Distance(cameraRef.transform.position, transform.position) < 45f;
        canvas.enabled = visible;
        if (!visible) return;
        Bounds bounds = new Bounds(transform.position + Vector3.up, Vector3.one);
        bool found = false;
        foreach (var renderer in renderers)
        {
            if (renderer == null || renderer is ParticleSystemRenderer) continue;
            if (!found) { bounds = renderer.bounds; found = true; } else bounds.Encapsulate(renderer.bounds);
        }
        root.position = new Vector3(bounds.center.x, bounds.max.y + .28f, bounds.center.z);
        root.rotation = cameraRef.transform.rotation;
        if (lastHealth != health.CurrentHealth)
        {
            lastHealth = health.CurrentHealth;
            number.SetText("{0}/{1}", (float)Mathf.Max(0, lastHealth), (float)health.maxHealth);
            fill.sizeDelta = new Vector2(86f * Mathf.Clamp01((float)lastHealth / Mathf.Max(1, health.maxHealth)), 4f);
        }
    }
    private void OnDisable() { if (root != null) root.gameObject.SetActive(false); }
    private void OnEnable() { if (root != null) root.gameObject.SetActive(true); }
    private void OnDestroy() { if (root != null) Destroy(root.gameObject); }
}
