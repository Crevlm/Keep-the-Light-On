using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingSequence : MonoBehaviour
{
    private LightController finalLight;
    private NarrativeTextController narrative;
    private bool begun;
    private CanvasGroup overlay;
    private TextMeshProUGUI hint;

    public void Begin(LightController lightController, NarrativeTextController narrativeController)
    {
        if (begun) return;
        begun = true;
        finalLight = lightController;
        narrative = narrativeController;
        finalLight.PrepareFinalRelight();
        StartCoroutine(Finish());
    }

    private IEnumerator Finish()
    {
        // Let the final path response finish before adding a gameplay prompt.
        yield return null;
        yield return new WaitUntil(() => narrative.IsDialogueIdle);
        Canvas canvas = CreateCanvas();
        hint = Label(canvas.transform, "Click the light one last time.", 26, new Vector2(0, -280));
        yield return new WaitUntil(() => finalLight.FinalLightHeld);
        hint.gameObject.SetActive(false);
        yield return new WaitUntil(() => finalLight.lightRatio >= 1f);
        yield return new WaitForSecondsRealtime(2f);
        narrative.PlayEnding();
        yield return null;
        yield return new WaitUntil(() => narrative.IsDialogueIdle);
        yield return new WaitForSecondsRealtime(2f);

        // Reaching the card is completion, separate from restarting or closing a tab.
        PlaytestRecorder.CompleteGame();
        GetComponent<StateDevelopmentNotice>()?.Hide();
        BuildCompletionCard(canvas.transform);
        float elapsed = 0;
        while (elapsed < 2f)
        {
            elapsed += Time.unscaledDeltaTime;
            overlay.alpha = Mathf.Clamp01(elapsed / 2f);
            yield return null;
        }
        overlay.interactable = true;
    }

    private Canvas CreateCanvas()
    {
        var go = new GameObject("Ending UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(transform, false);
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private void BuildCompletionCard(Transform parent)
    {
        var panel = new GameObject("Completion", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        panel.transform.SetParent(parent, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.055f, 0.055f, 0.08f, 0.88f);
        overlay = panel.GetComponent<CanvasGroup>();
        overlay.alpha = 0;
        overlay.interactable = false;
        Label(panel.transform, "KEEP THE LIGHT ON", 22, new Vector2(0, 140));
        Label(panel.transform, "You made it through.", 48, new Vector2(0, 50));
        Label(panel.transform, "Thank you for playing.", 24, new Vector2(0, -30));
        var buttonObject = new GameObject("Play Again", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(panel.transform, false);
        var buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(240, 60);
        buttonRect.anchoredPosition = new Vector2(0, -140);
        buttonObject.GetComponent<Image>().color = new Color(0.3f, 0.27f, 0.22f, 1f);
        var button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();
        button.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
        var label = Label(buttonObject.transform, "Play Again", 26, Vector2.zero);
        label.rectTransform.sizeDelta = new Vector2(230, 55);
    }

    private TextMeshProUGUI Label(Transform parent, string text, float size, Vector2 position)
    {
        var go = new GameObject("Ending text", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var label = go.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(1f, 0.94f, 0.81f);
        label.raycastTarget = false;
        label.rectTransform.sizeDelta = new Vector2(1000, 90);
        label.rectTransform.anchoredPosition = position;
        return label;
    }
}
