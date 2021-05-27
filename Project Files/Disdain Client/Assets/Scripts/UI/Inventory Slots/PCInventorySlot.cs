using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class PCInventorySlot : BaseInventorySlot, IPointerEnterHandler, IPointerExitHandler
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
}
