using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageManager : MonoBehaviour
{
    #region Singleton

    public static StorageManager instance;

    private void Awake()
    {
        instance = this;
    }

    #endregion

    List<ItemSettings> storage;
    List<ItemSettings> inventory;

    bool isStoring;
    bool canTrade;
    Vector3 currentPos;

    public void ToStorage(int slot)
    {
        if (storage.Count < 20 && isStoring && GameManager.ActiveUI.StoragePlayer[slot].itemSettings != null && !canTrade)
        {
            storage.Add(GameManager.ActiveUI.StoragePlayer[slot].itemSettings);
            inventory.Remove(GameManager.ActiveUI.StoragePlayer[slot].itemSettings);

            canTrade = true;
            StartCoroutine(ResetTradeI());

            UpdateStorageUI();
        }
    }

    public void ToPlayer(int slot)
    {
        if (inventory.Count < 28 && isStoring && GameManager.ActiveUI.StorageContainer[slot].itemSettings != null && !canTrade)
        {
            inventory.Add(GameManager.ActiveUI.StorageContainer[slot].itemSettings);
            storage.Remove(GameManager.ActiveUI.StorageContainer[slot].itemSettings);

            canTrade = true;
            StartCoroutine(ResetTradeI());

            UpdateStorageUI();
        }
    }

    IEnumerator ResetTradeI()
    {
        yield return new WaitForSeconds(0.1f);
        canTrade = false;
    }

    public void UpdateStorageUI()
    {
        for (int i = 0; i < GameManager.ActiveUI.StorageContainer.Length; i++) { GameManager.ActiveUI.StorageContainer[i].ClearSlot(); }
        for (int i = 0; i < GameManager.ActiveUI.StoragePlayer.Length; i++) { GameManager.ActiveUI.StoragePlayer[i].ClearSlot(); }

        for (int i = 0; i < storage.Count; i++) { GameManager.ActiveUI.StorageContainer[i].AddItem(storage[i]); }
        for (int i = 0; i < inventory.Count; i++) { GameManager.ActiveUI.StoragePlayer[i].AddItem(inventory[i]); }
    }

    public void InteractWithStorage(Vector3 pos)
    {
        if (isStoring || SavingManager.GameState != SavingManager.GameStateEnum.Singleplayer) { return; }

        inventory = new List<ItemSettings>(Inventory.instance.items);
        GameManager.ToggleUISection(2);
        storage.Clear();

        if (SavingManager.SaveFile.storage.ContainsKey(pos))
        {
            for (int i = 0; i < SavingManager.SaveFile.storage[pos].items.Count; i++)
            {
                storage.Add(Resources.Load<ItemSettings>("Interactables/" + SavingManager.SaveFile.storage[pos].items[i])); 
            }
        }
        else
        {
            SavingManager.SaveFile.storage.Add(pos, new StorageData());
            storage = new List<ItemSettings>();
        }

        UpdateStorageUI();
        isStoring = true;
        currentPos = pos;
    }

    public void StopInteractWithStorage()
    {
        if (SavingManager.GameState == SavingManager.GameStateEnum.Multiplayer)
        {
            using (Packet _packet = new Packet((int)ClientPackets.endStorage))
            {
                _packet.Write(inventory.Count);
                _packet.Write(storage.Count);
                _packet.Write(currentPos);

                for (int i = 0; i < inventory.Count; i++) { _packet.Write(inventory[i].name); }
                for (int i = 0; i < storage.Count; i++) { _packet.Write(storage[i].name); }

                ClientSend.SendTCPData(_packet);
            }

            isStoring = false;
            GameManager.ToggleUISection(0);

            currentPos = Vector3.zero;

            TweeningLibrary.FadeOut(GameManager.instance.Mobile.InteractButton.gameObject, 0.2f);

            Inventory.instance.currentObject = null;
            Inventory.instance.currentItemType = Inventory.CurrentItemTypeEnum.None;
            Inventory.instance.tempStorage = new List<string>();
        }
        else if (SavingManager.GameState == SavingManager.GameStateEnum.Singleplayer)
        {
            isStoring = false;
            GameManager.ToggleUISection(0);

            SavingManager.SaveFile.storage[currentPos].items.Clear();
            for (int i = 0; i < storage.Count; i++) { SavingManager.SaveFile.storage[currentPos].items.Add(storage[i].name); };

            Inventory.instance.items = new List<ItemSettings>(inventory);
            Inventory.instance.onItemChangedCallback.Invoke();
            UpdateStorageUI();

            SavingManager.SaveFile.inventoryItems.Clear();
            for (int i = 0; i < inventory.Count; i++) { SavingManager.SaveFile.inventoryItems.Add(inventory[i].name); }

            currentPos = Vector3.zero;
        }

        inventory.Clear();
        storage.Clear();
    }

    public void MultiplayerInteractWithStorage(List<string> storageList, Vector3 storagePos)
    {
        currentPos = storagePos;

        inventory.Clear();
        storage.Clear();

        GameManager.ToggleUISection(12);
        isStoring = true;

        inventory = new List<ItemSettings>(Inventory.instance.items);
        for (int i = 0; i < storageList.Count; i++) { storage.Add(Resources.Load<ItemSettings>("Prefabs/Interactable Items/" + storageList[i])); }
        UpdateStorageUI();
    }
}
