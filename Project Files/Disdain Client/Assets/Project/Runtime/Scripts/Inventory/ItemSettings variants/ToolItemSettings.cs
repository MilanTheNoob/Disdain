using UnityEngine;

[CreateAssetMenu(menuName = "Custom Scriptable Objects/Inventory System/Tool Item Settings")]
public class ToolItemSettings : ItemSettings
{
    private void Awake() { isUsableItem = true; }
    public override void Use() { GameManager.ActivePlayerManager.EquipTool(this); GameManager.ToggleUISection(0); }
}
