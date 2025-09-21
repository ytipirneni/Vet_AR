using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

public class AutoClickRayUI : MonoBehaviour
{
    public CurveVisualController rightHandCurveVisual;
    public CurveVisualController leftHandCurveVisual;
    public HandRaySwitcher handRaySwitcher; // Drag your HandRaySwitcher object here

    public TextMeshProUGUI debugText;
    public float hoverTimeToClick = 4f;
    public Image fillImage;

    GameObject currentHoveredButton;
    float hoverStartTime;

    void Start()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
            fillImage.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (handRaySwitcher == null)
        {
            Debug.LogWarning("HandRaySwitcher not assigned.");
            return;
        }

        CurveVisualController activeController = GetActiveCurve();

        if (activeController == null || activeController.CurrentEndPointType != EndPointType.UI)
        {
            ResetHover("Not pointing at UI");
            return;
        }

        GameObject hoveredButton = GetUIElementAtCursor(activeController);

        if (hoveredButton == null)
        {
            ResetHover("No button detected");
            return;
        }

        if (hoveredButton == currentHoveredButton)
        {
            float t = Time.unscaledTime - hoverStartTime;
            UpdateDebug($"Hovering on '{hoveredButton.name}' for {t:F1}s");
            UpdateFill(t);

            if (t >= hoverTimeToClick)
            {
                var button = hoveredButton.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.Invoke();
                    UpdateDebug($"✅ Clicked '{hoveredButton.name}'");
                    hoverStartTime = float.MaxValue; // prevent re-click
                }
            }
        }
        else
        {
            currentHoveredButton = hoveredButton;
            hoverStartTime = Time.unscaledTime;
            UpdateDebug($"Started hovering: {hoveredButton.name}");
            SetFillerVisible(true);
        }
    }

    CurveVisualController GetActiveCurve()
    {
        if (handRaySwitcher.rightRay && rightHandCurveVisual != null)
            return rightHandCurveVisual;

        if (handRaySwitcher.leftRay && leftHandCurveVisual != null)
            return leftHandCurveVisual;

        return null;
    }

    GameObject GetUIElementAtCursor(CurveVisualController controller)
    {
        Vector2 screenPos = Camera.main.WorldToScreenPoint(controller.CurrentCursorWorldPosition);

        PointerEventData pointer = new PointerEventData(EventSystem.current) { position = screenPos };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        foreach (var result in results)
        {
            if (result.gameObject.GetComponent<Button>() != null)
                return result.gameObject;
        }

        return null;
    }

    void ResetHover(string message)
    {
        if (currentHoveredButton != null)
            UpdateDebug($"⛔ Hover lost: {message}");

        currentHoveredButton = null;
        hoverStartTime = 0;
        SetFillerVisible(false);
    }

    void UpdateDebug(string msg)
    {
        if (debugText != null)
            debugText.text = msg;
    }

    void UpdateFill(float timeHovered)
    {
        if (fillImage == null)
            return;

        float progress = Mathf.Clamp01(timeHovered / hoverTimeToClick);
        fillImage.fillAmount = Mathf.SmoothStep(0f, 1f, progress);
    }

    void SetFillerVisible(bool visible)
    {
        if (fillImage == null)
            return;

        fillImage.gameObject.SetActive(visible);
        if (!visible)
            fillImage.fillAmount = 0f;
    }
}
