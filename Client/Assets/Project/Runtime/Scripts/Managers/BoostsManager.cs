using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostsManager : MonoBehaviour
{
    public ItemBoosts[] Boosts;

    void Start() { Inventory.instance.onItemChangedCallback += CheckForItems; }

    void CheckForItems()
    {
        for (int i = 0; i < Boosts.Length; i++)
        {
            if (!Boosts[i].inInventory && Inventory.instance.items.Contains(Boosts[i].item))
            {
                PlayerManager.speed += Boosts[i].speedBoost;
                PlayerManager.jump += Boosts[i].jumpBoost;
                Boosts[i].inInventory = true;
            }
            else if (Boosts[i].inInventory && !Inventory.instance.items.Contains(Boosts[i].item))
            {
                PlayerManager.speed -= Boosts[i].speedBoost;
                PlayerManager.jump -= Boosts[i].jumpBoost;
                Boosts[i].inInventory = false;
            }
        }
    }

    [System.Serializable]
    public class ItemBoosts
    {
        public ItemSettings item;
        public float speedBoost;
        public float jumpBoost;

        [HideInInspector]
        public bool inInventory;
    }
}
