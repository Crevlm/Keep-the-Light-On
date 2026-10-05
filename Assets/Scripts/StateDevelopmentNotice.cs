using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Temporary playtest signposting while the last two environments are being built.
public class StateDevelopmentNotice : MonoBehaviour
{
    private RoomController room;
    private GameObject panel;
    private Image background;
    private TextMeshProUGUI label;
    private int lastState = -1;
    private bool dismissed;

    public void Initialize(RoomController controller)
    {
        room = controller;
        var canvasObject = new GameObject("Environment progress notice", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;

        panel = new GameObject("State marker", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasObject.transform, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.one;
        rect.anchoredPosition = new Vector2(-24, -24);
        rect.sizeDelta = new Vector2(360, 64);
        background = panel.GetComponent<Image>();
        background.raycastTarget = false;

        var textObject = new GameObject("Notice text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panel.transform, false);
        label = textObject.GetComponent<TextMeshProUGUI>();
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = new Vector2(18, 12);
        label.rectTransform.offsetMax = new Vector2(-18, -12);
        label.fontSize = 20;
        label.color = new Color(1f, 0.96f, 0.87f);
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.raycastTarget = false;
        Refresh();
    }

    private void Update()
    {
        if (room != null && room.currentRoomState != lastState) Refresh();
    }

    public void Hide()
    {
        dismissed = true;
        if (panel != null) panel.SetActive(false);
    }

    private void Refresh()
    {
        lastState = room.currentRoomState;
        panel.SetActive(!dismissed && (lastState == 4 || lastState == 5));
        if (lastState == 4)
        {
            background.color = new Color(0.32f, 0.22f, 0.13f, 0.94f);
            label.text = "<b>STATE 4 · WORK IN PROGRESS</b>";
        }
        else if (lastState == 5)
        {
            background.color = new Color(0.14f, 0.29f, 0.22f, 0.94f);
            label.text = "<b>STATE 5 · WORK IN PROGRESS</b>";
        }
    }
}
