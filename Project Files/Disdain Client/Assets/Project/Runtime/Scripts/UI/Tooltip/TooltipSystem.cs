using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipSystem : MonoBehaviour
{
    #region Singleton
    public static TooltipSystem instance;
    void Awake() { instance = this; }
    #endregion

    public Tooltip tooltip;
    bool tooltipEnabled;

    public static void Show(string content, string header = "")
    {
        instance.tooltipEnabled = true;

        if (string.IsNullOrEmpty(header))
        {
            instance.tooltip.Header.gameObject.SetActive(false);
        }
        else
        {
            instance.tooltip.Header.gameObject.SetActive(true);
            instance.tooltip.Header.text = header;
        }

        instance.tooltip.Content.text = content;
        TweeningLibrary.FadeIn(instance.tooltip.gameObject, 0.2f);
    }

    public static void Hide()
    {
        if (!instance.tooltipEnabled) return;
        TweeningLibrary.FadeOut(instance.tooltip.gameObject, 0.2f);    }
}
