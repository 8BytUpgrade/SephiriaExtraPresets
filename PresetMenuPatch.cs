using System.Collections.Generic;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SephiriaPresets;

[HarmonyPatch(typeof(UI_PresetPanel), "Awake")]
internal static class PresetWindowAwakePatch
{
    private static float originalPresetHeight;
    [HarmonyPrefix]
    private static void Prefix(UI_PresetPanel __instance)
    // Make buttons
    {
        var buttons = __instance.presetButtons;

        RectTransform content = buttons[0].transform.parent as RectTransform;
        // Make Unity calculate the original layout first.
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        // Save the height required by the original preset list.
        originalPresetHeight = LayoutUtility.GetPreferredHeight(content);

        UI_HorayButton template = buttons[buttons.Count - 1];

        const int extraPresetCount = 25;
        for (int i = 0; i < extraPresetCount ; i++)
        {
            int newSlotIndex = buttons.Count;
            int slotNumber = newSlotIndex + 1;
            
            UI_HorayButton clone =
                Object.Instantiate(
                    template,
                    template.transform.parent
                );
            clone.name = $"Slot{slotNumber}";
            buttons.Add(clone);

            int capturedSlotIndex = buttons.IndexOf(clone);
            clone.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
            clone.onClick.AddListener(() =>
            {
                __instance.SelectPresetSlot(capturedSlotIndex);
            });
        }
    }
    [HarmonyPostfix]
    private static void Postfix(UI_PresetPanel __instance)
    {
        MakeScrollable(__instance.presetButtons);
    }
    // Scroll box thingy
    private static void MakeScrollable(List<UI_HorayButton> presetButtons)
    {
        if (presetButtons == null || presetButtons.Count == 0)
            return;

        RectTransform content =
            presetButtons[0].transform.parent as RectTransform;

        if (content == null)
            return;

        // Already converted.
        if (content.parent != null &&
            content.parent.name == "PresetViewport")
        {
            return;
        }

        RectTransform panel = content.parent as RectTransform;

        if (panel == null)
            return;

        // Preserve PresetGroup's current visual position.
        Vector2 originalAnchorMin = content.anchorMin;
        Vector2 originalAnchorMax = content.anchorMax;
        Vector2 originalPivot = content.pivot;
        Vector2 originalSize = content.sizeDelta;
        Vector2 originalPosition = content.anchoredPosition;

        int siblingIndex = content.GetSiblingIndex();

        // --------------------------------------------------
        // ScrollRect root
        // --------------------------------------------------

        GameObject scrollObject = new GameObject(
            "PresetScroll",
            typeof(RectTransform),
            typeof(ScrollRect)
        );

        RectTransform scrollRectTransform =
            scrollObject.GetComponent<RectTransform>();

        scrollRectTransform.SetParent(panel, false);
        scrollRectTransform.SetSiblingIndex(siblingIndex);

        // Make the viewport occupy exactly the area that
        // PresetGroup currently occupies.
        scrollRectTransform.anchorMin = originalAnchorMin;
        scrollRectTransform.anchorMax = originalAnchorMax;
        scrollRectTransform.pivot = originalPivot;
        scrollRectTransform.sizeDelta = 
            new Vector2(
                originalSize.x,
                originalPresetHeight
            );
        scrollRectTransform.anchoredPosition = originalPosition;

        // --------------------------------------------------
        // Viewport
        // --------------------------------------------------

        GameObject viewportObject = new GameObject(
            "PresetViewport",
            typeof(RectTransform),
            typeof(Image),
            typeof(RectMask2D)
        );

        RectTransform viewport =
            viewportObject.GetComponent<RectTransform>();

        viewport.SetParent(scrollRectTransform, false);

        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = Vector2.zero;

        // Transparent Graphic so the viewport can receive
        // pointer/scroll events.
        Image viewportImage =
            viewportObject.GetComponent<Image>();

        viewportImage.color =
            new Color(0f, 0f, 0f, 0f);

        viewportImage.raycastTarget = true;

        // --------------------------------------------------
        // Existing PresetGroup becomes ScrollRect content
        // --------------------------------------------------

        content.SetParent(viewport, false);

        // Top anchored, horizontally stretched.
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(1f, 1f);

        content.anchoredPosition = Vector2.zero;

        // Width now comes from the viewport.
        // Height remains controlled by ContentSizeFitter.
        content.sizeDelta =
            new Vector2(0f, originalSize.y);

        // --------------------------------------------------
        // ScrollRect
        // --------------------------------------------------

        ScrollRect scrollRect =
            scrollObject.GetComponent<ScrollRect>();

        scrollRect.content = content;
        scrollRect.viewport = viewport;

        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        scrollRect.movementType =
            ScrollRect.MovementType.Clamped;

        scrollRect.inertia = true;
        scrollRect.scrollSensitivity = 2f;

        // Force Unity's layout system to recalculate
        // PresetGroup after the hierarchy change.
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }
}