using System.Collections;
using UnityEngine;

public class Interaction_InteractableItem : InteractableItem
{
    public ItemSettings itemSettings;

    bool dontInteract;

    void Awake() { interactTxt = "Pick up"; }

    public override void OnInteract()
    { 
        if (!dontInteract && Inventory.instance.items.Count < 14)
        {
            PropsGeneration.instance.AddToPropPool(gameObject);
            Inventory.instance.Add(itemSettings);

            dontInteract = true;
            StartCoroutine(ResetI());
        }
    }

    IEnumerator ResetI()
    {
        yield return new WaitForSeconds(0.1f);
        dontInteract = false;
        gameObject.SetActive(false);
    }
}
