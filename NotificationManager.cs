using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationManager : MonoBehaviour
{
    private Canvas worldCanvas;
    private RectTransform panelRT;
    private VerticalLayoutGroup layout;
    private Camera targetCamera;
    private List<GameObject> active = new List<GameObject>();
    private int maxLines = 6;

    private Font font;
    private int fontSize = 36;
    private Color textColor = Color.white;
    private float lineSpacing = 4f;
    private float fadeDuration = 0.6f;
    private float distanceFromCamera = 0.6f;
    private float bottomOffset = 0.15f;

    public void Init(Camera cam, float bottomOffset = 0.15f)
    {
        targetCamera = cam;
        this.bottomOffset = bottomOffset;

        GameObject root = this.gameObject;
        root.transform.SetParent(null);
        DontDestroyOnLoad(root);

        GameObject container = new GameObject("LobbyNotifier_Canvas");
        container.transform.SetParent(root.transform, false);

        worldCanvas = container.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.worldCamera = cam;
        worldCanvas.sortingOrder = 9999;

        container.AddComponent<CanvasScaler>();
        container.AddComponent<GraphicRaycaster>();

        RectTransform canvasRT = worldCanvas.GetComponent<RectTransform>();
        canvasRT.sizeDelta = new Vector2(1.8f, 0.5f);

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(container.transform, false);

        panelRT = panel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0f);
        panelRT.anchorMax = new Vector2(0.5f, 0f);
        panelRT.pivot = new Vector2(0.5f, 0f);
        panelRT.anchoredPosition = new Vector2(0f, 0f);
        panelRT.sizeDelta = new Vector2(1.6f, 0.45f);

        layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.LowerCenter;
        layout.spacing = lineSpacing;
        layout.childForceExpandHeight = false;
        layout.childControlHeight = true;

        ContentSizeFitter fitter = panel.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        UpdatePositionAndRotation();
    }

    void Update()
    {
        if (targetCamera != null)
            UpdatePositionAndRotation();
    }

    private void UpdatePositionAndRotation()
    {
        Transform camT = targetCamera.transform;
        Vector3 pos = camT.position + camT.forward * distanceFromCamera - camT.up * bottomOffset;
        worldCanvas.transform.position = pos;
        worldCanvas.transform.rotation = Quaternion.LookRotation(camT.forward, camT.up);
        worldCanvas.transform.localScale = Vector3.one * 0.005f;
    }

    public void Send(string text, float seconds)
    {
        if (string.IsNullOrEmpty(text)) return;

        GameObject go = new GameObject("NotifLine");
        go.transform.SetParent(panelRT, false);

        Text t = go.AddComponent<Text>();
        t.font = font;
        t.fontSize = fontSize;
        t.text = text;
        t.alignment = TextAnchor.LowerCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.color = textColor;
        t.supportRichText = true;

        RectTransform rt = t.rectTransform;
        rt.sizeDelta = new Vector2(1400f, 100f);

        active.Add(go);
        while (active.Count > maxLines)
        {
            GameObject oldest = active[0];
            active.RemoveAt(0);
            Destroy(oldest);
        }

        StartCoroutine(ClearAfter(go, seconds));
    }

    private IEnumerator ClearAfter(GameObject go, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Text t = go.GetComponent<Text>();
        if (t == null) yield break;

        float start = Time.time;
        Color startC = t.color;
        while (Time.time - start < fadeDuration)
        {
            float f = 1f - (Time.time - start) / fadeDuration;
            t.color = new Color(startC.r, startC.g, startC.b, Mathf.Clamp01(f));
            yield return null;
        }
        active.Remove(go);
        Destroy(go);
    }
}