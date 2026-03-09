using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor utility: attach to an empty GameObject and call BuildScene()
/// from the Inspector button (or call it from Awake for auto-build).
/// 
/// Builds the complete 4-floor / 3-elevator scene procedurally so you
/// don't have to manually create every GameObject.
/// </summary>
public class SceneBuilder : MonoBehaviour
{
    [Header("Layout")]
    public int   floorCount    = 4;
    public int   elevatorCount = 3;
    public float floorHeight   = 2.0f;
    public float floorWidth    = 12.0f;

    [Header("Auto-build on play")]
    public bool buildOnAwake = true;

    // Colours
    Color shaftColor    = new Color(0.15f, 0.15f, 0.20f);
    Color platformColor = new Color(0.25f, 0.25f, 0.35f);
    Color wallColor     = new Color(0.18f, 0.18f, 0.25f);
    Color elevColor     = new Color(0.30f, 0.60f, 1.00f);

    void Awake()
    {
        if (buildOnAwake) BuildScene();
    }

    public void BuildScene()
    {
        Camera.main.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = floorCount * floorHeight * 0.7f;
        Camera.main.transform.position = new Vector3(
            floorWidth * 0.5f,
            (floorCount - 1) * floorHeight * 0.5f,
            -10f);

        BuildBuilding();
        var elevators = BuildElevators();
        BuildUI(elevators);

        // Wire dispatcher
        var dispGO = new GameObject("ElevatorDispatcher");
        var disp   = dispGO.AddComponent<ElevatorDispatcher>();
        disp.elevators = elevators;
    }

    // ── Building geometry ──────────────────────────────────────────────────

    void BuildBuilding()
    {
        // Background wall
        CreateSprite("Building_Wall",
            new Vector3(floorWidth * 0.5f, (floorCount - 1) * floorHeight * 0.5f, 1f),
            new Vector2(floorWidth, floorCount * floorHeight + 0.5f),
            wallColor);

        // Floor platforms
        for (int f = 0; f < floorCount; f++)
        {
            float y = f * floorHeight - 0.1f;
            CreateSprite($"Floor_{f}",
                new Vector3(floorWidth * 0.5f, y, 0f),
                new Vector2(floorWidth, 0.18f),
                platformColor);

            // Floor number label on left wall
            CreateWorldLabel($"FloorNum_{f}", ElevatorController.FloorName(f),
                new Vector3(0.4f, f * floorHeight + 0.5f, -0.1f), 0.6f);
        }

        // Elevator shafts
        float shaftWidth = 1.2f;
        float shaftSpacing = 2.2f;
        float shaftStartX = 2.0f;

        for (int i = 0; i < elevatorCount; i++)
        {
            float x = shaftStartX + i * shaftSpacing;
            CreateSprite($"Shaft_{i}",
                new Vector3(x, (floorCount - 1) * floorHeight * 0.5f, 0.5f),
                new Vector2(shaftWidth, floorCount * floorHeight),
                shaftColor);
        }
    }

    // ── Elevators ──────────────────────────────────────────────────────────

    List<ElevatorController> BuildElevators()
    {
        var list = new List<ElevatorController>();

        float shaftSpacing = 2.2f;
        float shaftStartX  = 2.0f;

        for (int i = 0; i < elevatorCount; i++)
        {
            float x = shaftStartX + i * shaftSpacing;

            var go = new GameObject($"Elevator_{i}");
            go.transform.position = new Vector3(x, 0f, -0.5f);

            // Sprite
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateBoxSprite();
            sr.color  = elevColor;
            go.transform.localScale = new Vector3(0.9f, 0.8f, 1f);

            // Floor label (world-space canvas)
            var labelGO = CreateWorldLabel($"ElevLabel_{i}", "G",
                new Vector3(x, 0.55f, -0.6f), 0.55f);
            labelGO.transform.SetParent(go.transform, true);

            // Controller
            var ctrl = go.AddComponent<ElevatorController>();
            ctrl.elevatorID  = i;
            ctrl.floorHeight = floorHeight;
            ctrl.moveSpeed   = 2.5f;
            ctrl.floorLabel  = labelGO.GetComponent<TextMeshProUGUI>();

            list.Add(ctrl);
        }

        return list;
    }

    // ── UI (Canvas with floor buttons) ─────────────────────────────────────

    void BuildUI(List<ElevatorController> elevators)
    {
        // Main canvas
        var canvasGO = new GameObject("UI_Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Title
        CreateUILabel(canvasGO.transform, "Title",
            "🏢  Elevator Simulation", new Vector2(0, -30), new Vector2(400, 50), 22);

        // Panel on right side for floor buttons
        var panelGO = new GameObject("ButtonPanel");
        panelGO.transform.SetParent(canvasGO.transform, false);
        var panelRT = panelGO.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(1, 0.5f);
        panelRT.anchorMax = new Vector2(1, 0.5f);
        panelRT.pivot     = new Vector2(1, 0.5f);
        panelRT.anchoredPosition = new Vector2(-20, 0);
        panelRT.sizeDelta = new Vector2(130, floorCount * 70 + 20);

        var bg = panelGO.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

        // Header
        CreateUILabel(panelGO.transform, "PanelTitle",
            "CALL LIFT", new Vector2(0, (floorCount * 70 / 2f) - 10), new Vector2(120, 30), 14);

        // One button per floor (top = highest floor)
        for (int f = floorCount - 1; f >= 0; f--)
        {
            int floorIndex   = floorCount - 1 - f; // visual order
            float yOffset    = (floorCount * 70 / 2f) - 40 - floorIndex * 65;
            string floorName = ElevatorController.FloorName(f);

            var btnGO = new GameObject($"FloorBtn_{f}");
            btnGO.transform.SetParent(panelGO.transform, false);

            var rt = btnGO.AddComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0, yOffset);
            rt.sizeDelta        = new Vector2(110, 55);

            var img = btnGO.AddComponent<Image>();
            img.color = new Color(0.85f, 0.85f, 0.85f);

            var btn = btnGO.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor      = new Color(0.85f, 0.85f, 0.85f);
            colors.highlightedColor = new Color(1f, 1f, 0.7f);
            colors.pressedColor     = new Color(0.5f, 0.8f, 0.5f);
            btn.colors = colors;

            // Label inside button
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = $"Floor {floorName}\n▲ Call";
            tmp.fontSize  = 13;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.black;
            var trt = textGO.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;

            // Wire FloorButton component
            var fb       = btnGO.AddComponent<FloorButton>();
            fb.floorNumber = f;
            fb.button      = btn;
            fb.label       = tmp;
        }

        // Elevator status strip at bottom
        BuildStatusStrip(canvasGO.transform, elevators);
    }

    void BuildStatusStrip(Transform parent, List<ElevatorController> elevators)
    {
        var stripGO = new GameObject("StatusStrip");
        stripGO.transform.SetParent(parent, false);
        var rt = stripGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot     = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 10);
        rt.sizeDelta = new Vector2(0, 40);

        var bg = stripGO.AddComponent<Image>();
        bg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);

        for (int i = 0; i < elevators.Count; i++)
        {
            int captured = i;
            var labelGO = new GameObject($"Status_{i}");
            labelGO.transform.SetParent(stripGO.transform, false);
            var lrt = labelGO.AddComponent<RectTransform>();
            lrt.anchorMin = new Vector2(i / (float)elevators.Count, 0);
            lrt.anchorMax = new Vector2((i + 1) / (float)elevators.Count, 1);
            lrt.offsetMin = lrt.offsetMax = Vector2.zero;

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.fontSize  = 12;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;

            // Live updater
            var updater = labelGO.AddComponent<ElevatorStatusUpdater>();
            updater.elevator   = elevators[captured];
            updater.statusText = tmp;
            updater.elevatorID = captured;
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    GameObject CreateSprite(string name, Vector3 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreateBoxSprite();
        sr.color  = color;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        return go;
    }

    GameObject CreateWorldLabel(string name, string text, Vector3 pos, float size)
    {
        var go      = new GameObject(name);
        go.transform.position = pos;
        var canvas  = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        var tmp     = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(2f, 1f);
        rt.localScale = Vector3.one * 0.5f;
        return go;
    }

    void CreateUILabel(Transform parent, string name, string text,
                       Vector2 anchoredPos, Vector2 size, float fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
    }

    Sprite CreateBoxSprite()
    {
        var tex = new Texture2D(4, 4);
        Color[] pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 1f);
    }
}
