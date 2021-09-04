using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    public PlayerManager.ToolType toolType = PlayerManager.ToolType.None;

    [HideInInspector]
    public string interactTxt;
    [HideInInspector]
    public bool isInteractable;

    void Start()
    {
        gameObject.layer = 9;
        isInteractable = true;

        MeshCollider col = gameObject.GetComponent<MeshCollider>();
        if (col != null) { col.convex = true; }
    }

    public virtual void OnInteract() { }
}
