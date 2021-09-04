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

    BaseInventorySlot[] Storage;
    BaseInventorySlot[] Player;

    bool isStoring;
    bool canTrade;
    Vector3 currentPos;

    int chunkLoc;
    int storageLoc;

    private void Start()
    {
        Storage = GameManager.ActiveUI.StorageContainer;
        Player = GameManager.ActiveUI.StoragePlayer;

        for (int i = 0; i < Storage.Length; i++) { Storage[i].UseInventory = false; }
        for (int i = 0; i < Player.Length; i++) { Player[i].UseInventory = false; }
    }

    private void FixedUpdate()
    {
        if (isStoring)
        {
            for (int i = 0; i < Storage.Length; i++) { if (Storage[i].Clicked) { ToInventory(i); } }
            for (int i = 0; i < Player.Length; i++) {  if (Player[i].Clicked) { ToContainer(i); } }
        }
    }

    public void ToContainer(int slot)
    {
        if (storage.Count < 20 && isStoring && Player[slot].itemSettings != null && !canTrade)
        {
            storage.Add(Player[slot].itemSettings);
            inventory.Remove(Player[slot].itemSettings);

            canTrade = true;
            StartCoroutine(ResetTradeI());

            UpdateStorageUI();
        }
    }

    public void ToInventory(int slot)
    {
        if (inventory.Count < 28 && isStoring && Storage[slot].itemSettings != null && !canTrade)
        {
            inventory.Add(Storage[slot].itemSettings);
            storage.Remove(Storage[slot].itemSettings);

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
        /*
        if (isStoring || DataManager.GameState != GameStateEnum.Singleplayer) { return; }

        inventory = new List<ItemSettings>(Inventory.instance.items);
        GameManager.ToggleUISection(2);
        storage.Clear();

        isStoring = true;
        currentPos = pos;

        chunkLoc = DataManager.SaveFile.FindChunk(currentPos);
        storageLoc = DataManager.SaveFile.FindStorage(currentPos);

        if (storageLoc != -1)
        {
            for (int i = 0; i < DataManager.SaveFile.Chunks[chunkLoc].Storage[storageLoc].Items.Count; i++)
            {
                storage.Add(Resources.Load<ItemSettings>("Interactables/" + DataManager.SaveFile.Chunks[chunkLoc].Storage[storageLoc].Items[i])); 
            }
        }
        else
        {
            DataManager.SaveFile.Chunks[chunkLoc].Storage.Add(new SaveFileClass.StorageClass());
            storage = new List<ItemSettings>();
        }

        UpdateStorageUI();
        */
    }

    public void StopInteractWithStorage()
    {
        if (DataManager.GameState == GameStateEnum.Multiplayer)
        {
            /*
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
            */
        }
        else if (DataManager.GameState == GameStateEnum.Singleplayer)
        {
            /*
            isStoring = false;
            GameManager.ToggleUISection(0);

            DataManager.SaveFile.Chunks[chunkLoc].Storage[storageLoc].Items.Clear();
            for (int i = 0; i < storage.Count; i++) 
            { 
                DataManager.SaveFile.Chunks[chunkLoc].Storage[storageLoc].Items.Add(storage[i].name); 
            }

            Inventory.instance.items = new List<ItemSettings>(inventory);
            Inventory.instance.onItemChangedCallback.Invoke();
            UpdateStorageUI();

            DataManager.SaveFile.Inventory.Clear();
            for (int i = 0; i < inventory.Count; i++) { DataManager.SaveFile.Inventory.Add(inventory[i].name); }

            currentPos = Vector3.zero;
            */
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
