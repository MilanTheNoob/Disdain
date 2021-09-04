using UnityEngine;

[CreateAssetMenu(menuName = "Custom Scriptable Objects/Inventory System/Item Settings")]
[System.Serializable]
public class ItemSettings : ScriptableObject
{
    public Sprite icon;
    public GameObject gameObject;

    [Space]

    public bool ignoreGravity;

    [Space]

    public string description;

    [HideInInspector]
    public bool isUsableItem;
    [HideInInspector]
    public bool dontDrop = false;

    public virtual void Use() { }
}
