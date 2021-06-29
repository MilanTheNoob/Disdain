using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class BaseInventorySlot : MonoBehaviour
{
    [HideInInspector]
    public ItemSettings itemSettings;
    [HideInInspector]
    public string itemName;

    [HideInInspector]
    public bool UseInventory;
    [HideInInspector]
    public bool Clicked;
    [HideInInspector]
    public Image BGIcon;

    Image ItemIcon;
    GameObject CloseButton;

    void Awake()
    {
        UpdateUI();
        if (itemSettings == null) { ClearSlot(); }

        UseInventory = true;
    }

    public virtual void AddItem(ItemSettings newItemSettings)
    {
        Clicked = true;
        GameManager.SetSlotVar(this);

        if (ItemIcon == null) { UpdateUI(); }
        if (newItemSettings == null || !UseInventory) { return; }

        itemSettings = newItemSettings;

        if (ItemIcon != null)
        {
            print(ItemIcon.gameObject.name);
            ItemIcon.enabled = true;
            ItemIcon.sprite = itemSettings.icon;
        }
        if (CloseButton != null) { CloseButton.SetActive(true); }

        itemName = string.Join(" ", itemSettings.name.Replace("-", " ").Split(' ').ToList()
                .ConvertAll(word =>
                        word.Substring(0, 1).ToUpper() + word.Substring(1)
                )
        );
    }

    public virtual void ClearSlot()
    {
        itemSettings = null;

        if (ItemIcon != null) { ItemIcon.sprite = null; ItemIcon.enabled = false; }
        if (CloseButton != null) { CloseButton.SetActive(false); }
    }
    public virtual void UseItem()
    {
        if (itemSettings != null)
        {
            if (!itemSettings.isUsableItem) return;

            itemSettings.Use();
            Inventory.instance.Destroy(itemSettings);
            ClearSlot();
            AudioManager.PlayEquip();
        }
    }

    public void RemoveItem()
    {
        Inventory.instance.Remove(itemSettings);
    }

    void UpdateUI()
    {
        Image[] temp = GetComponentsInChildren<Image>();
        BGIcon = temp[0];
        ItemIcon = temp[1];
        if (temp.Length >= 3) { CloseButton = temp[2].gameObject; }
    }
}
