using System.Collections;
using UnityEngine;

public class UI_InteractableItem : InteractableItem
{
    public int UISection;

    void Awake() { interactTxt = "Open"; }

    public override void OnInteract()
    {
        GameManager.ToggleUISection(UISection);
    }
}
