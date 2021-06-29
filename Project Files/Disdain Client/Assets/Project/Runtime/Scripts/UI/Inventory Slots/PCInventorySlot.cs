using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class PCInventorySlot : BaseInventorySlot, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    bool PointerHovering;

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerHovering = true;
        if (itemSettings != null) StartCoroutine(EnableTooltipI());
    }

    IEnumerator EnableTooltipI()
    {
        yield return new WaitForSeconds(0.5f);
        if (PointerHovering) { TooltipSystem.Show(itemSettings.description, itemName); }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerHovering = false;
        TooltipSystem.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        base.UseItem();

        TweeningLibrary.LerpColor(BGIcon, new Color32(27, 27, 27, 255), new Color32(35, 35, 35, 255), 0.1f);
        StartCoroutine(IClick());
    }

    IEnumerator IClick()
    {
        yield return new WaitForSeconds(0.1f);
        TweeningLibrary.LerpColor(BGIcon, new Color32(35, 35, 35, 255), new Color32(27, 27, 27, 255), 0.1f);
    }
}
