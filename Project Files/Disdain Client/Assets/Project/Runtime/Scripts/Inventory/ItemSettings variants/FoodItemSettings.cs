using UnityEngine;

[CreateAssetMenu(menuName = "Inventory System/Food Item Settings")]
public class FoodItemSettings : ItemSettings
{
    [Space]

    public float healthChange = 0.05f;
    public float hungerChange = 0.4f;

    bool used;

    private void Awake() { isUsableItem = true; }

    public override void Use()
    {
        if (DataManager.GameState == GameStateEnum.Singleplayer)
        {
            base.Use();

            VitalsManager.instance.ModifyVitalAmount(0, healthChange);
            VitalsManager.instance.ModifyVitalAmount(1, hungerChange);

            Inventory.instance.Destroy(this);
        }
        else if (DataManager.GameState == GameStateEnum.Multiplayer && !used)
        {
            //ClientSend.UpdateVital(healthChange, hungerChange);
            used = true;
        }
    }
}