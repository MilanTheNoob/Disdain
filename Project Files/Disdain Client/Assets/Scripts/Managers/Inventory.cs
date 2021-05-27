using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;

    [HideInInspector]
    public List<ItemSettings> items = new List<ItemSettings>();
    [HideInInspector]
    public List<string> tempStorage = new List<string>();
    [HideInInspector]
    public GameObject currentObject;
    [HideInInspector]
    public Vector3 tempStoragePos;

    public enum CurrentItemTypeEnum
    { 
        None,
        Normal,
        Tree,
        Storage
    }
    [HideInInspector]
    public CurrentItemTypeEnum currentItemType;

    void Awake()
    {
        instance = this;
        onItemChangedCallback += UpdateUI;

        if (SavingManager.GameState == SavingManager.GameStateEnum.Singleplayer)
        {
            items.Clear();
            for (int i = 0; i < SavingManager.SaveFile.inventoryItems.Count; i++) { items.Add(Resources.Load<ItemSettings>("Interactable Items/" + SavingManager.SaveFile.inventoryItems[i])); }

            StartCoroutine(CallbackAfterStartI());
        }
        else if (SavingManager.GameState == SavingManager.GameStateEnum.Multiplayer)
        {
            GameManager.instance.Mobile.InteractButton.onClick.AddListener(Interact);
        }
    }

    public bool Add(ItemSettings itemSettings)
    {
        if (items.Count <= 28)
        {
            items.Add(itemSettings);
            onItemChangedCallback.Invoke();

            SavingManager.SaveFile.inventoryItems.Add(itemSettings.name);

            return true;
        }
        else { return false; }
    }

    public void Remove(ItemSettings itemSettings)
    {
        if (!items.Contains(itemSettings)) { return; }

        if (SavingManager.GameState == SavingManager.GameStateEnum.Singleplayer)
        {
            if (itemSettings.gameObject != null && !itemSettings.dontDrop)
            {
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    BuildingManager.instance.StartDropItem(itemSettings);
                });
            }

            if (SavingManager.GameState == SavingManager.GameStateEnum.Singleplayer) { SavingManager.SaveFile.inventoryItems.Remove(itemSettings.name); }
            onItemChangedCallback.Invoke();
        }
        else if (SavingManager.GameState == SavingManager.GameStateEnum.Multiplayer)
        {
            using (Packet _packet = new Packet((int)ClientPackets.removeInventory))
            {
                _packet.Write(1);
                _packet.Write(itemSettings.name);

                ClientSend.SendTCPData(_packet);
            }
        }
    }

    public void Destroy(ItemSettings itemSettings)
    {
        if (SavingManager.GameState == SavingManager.GameStateEnum.Singleplayer)
        {
            SavingManager.SaveFile.inventoryItems.Remove(itemSettings.name);
            items.Remove(itemSettings);
            onItemChangedCallback.Invoke();
        }
        else if (SavingManager.GameState == SavingManager.GameStateEnum.Multiplayer)
        {
            using (Packet _packet = new Packet((int)ClientPackets.removeInventory))
            {
                _packet.Write(items.Count);
                for (int i = 0; i < items.Count; i++) { _packet.Write(items[i].name); }

                ClientSend.SendTCPData(_packet);
            }
        }
    }

    public void DestroyAll()
    {
        SavingManager.SaveFile.inventoryItems.Clear();
        items.Clear();
        onItemChangedCallback.Invoke();
    }

    IEnumerator CallbackAfterStartI() { yield return 0; onItemChangedCallback.Invoke(); }

    public void UpdateUI()
    {
        for (int i = 0; i < 28; i++) 
        { 
            GameManager.ActiveUI.Inventory[i].ClearSlot();
            GameManager.ActiveUI.StoragePlayer[i].ClearSlot(); 
        }
        for (int i = 0; i < items.Count; i++) 
        {
            GameManager.ActiveUI.Inventory[i].AddItem(items[i]);
            GameManager.ActiveUI.StoragePlayer[i].AddItem(items[i]); 
        }
    }

    #region Multiplayer Code

    private void FixedUpdate()
    {
        if (currentItemType != CurrentItemTypeEnum.None)
        {
            if (Input.GetKeyDown(KeyCode.E)) { Interact(); }
        }
    }

    public static void StartInteract(Packet _packet)
    {
        TweeningLibrary.FadeIn(GameManager.instance.Mobile.InteractButton.gameObject, 0.2f);
        instance.currentItemType = CurrentItemTypeEnum.Normal;
    }

    public static void StartTreeInteract(Packet _packet)
    {
        //if (!instance.items.Contains(instance.axe)) { return; }

        string interactTxt = _packet.ReadString();
        string interactNameTxt = _packet.ReadString();
        Vector3 pos = _packet.ReadVector3();

        instance.currentObject = MultiplayerTerrainGenerator.GetNearestChunk(pos).propDict[pos];
        instance.currentItemType = CurrentItemTypeEnum.Tree;
        
        TweeningLibrary.FadeIn(GameManager.instance.Mobile.InteractButton.gameObject, 0.2f);
    }

    public static void StartStorageInteract(Packet _packet)
    {
        string interactTxt = _packet.ReadString();
        string interactNameTxt = _packet.ReadString();

        instance.currentItemType = CurrentItemTypeEnum.Storage;

        TweeningLibrary.FadeIn(GameManager.instance.Mobile.InteractButton.gameObject, 0.2f);

        int storageSize = _packet.ReadInt();
        instance.tempStorage.Clear();

        if (storageSize > 0)
        {
            for (int i = 0; i < storageSize; i++)
            {
                instance.tempStorage.Add(_packet.ReadString());
                print(instance.tempStorage[i]);
            }
        }

        instance.tempStoragePos = _packet.ReadVector3();
    }

    public void Interact()
    {
        if (currentItemType == CurrentItemTypeEnum.Normal)
        {
            using (Packet _packet = new Packet((int)ClientPackets.interact)) { ClientSend.SendTCPData(_packet); }

            TweeningLibrary.FadeOut(GameManager.instance.Mobile.InteractButton.gameObject, 0.2f);

            currentObject = null;
            currentItemType = CurrentItemTypeEnum.None;
        }
        else if (currentItemType == CurrentItemTypeEnum.Tree)
        {
            TweeningLibrary.FadeOut(GameManager.instance.Mobile.InteractButton.gameObject, 0.2f);

            AudioManager.PlayChop();
            //ToolsManager.instance.SwingAnim();

            StartCoroutine(DestroyTreeI(currentObject.transform.position));

            Rigidbody rb = currentObject.GetComponent<Rigidbody>();
            if (rb == null) { rb = currentObject.AddComponent<Rigidbody>(); }

            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.AddForce(Vector3.forward, ForceMode.Impulse);

            currentItemType = CurrentItemTypeEnum.None;
        }
        else if (currentItemType == CurrentItemTypeEnum.Storage)
        {
            StorageManager.instance.MultiplayerInteractWithStorage(tempStorage, tempStoragePos);
        }
    }

    public static void StopInteract(Packet _packet)
    {
        if (instance.currentItemType == CurrentItemTypeEnum.None) { return; }

        TweeningLibrary.FadeOut(GameManager.instance.Mobile.InteractButton.gameObject, 0.2f);

        instance.currentObject = null;
        instance.currentItemType = CurrentItemTypeEnum.None;
    }

    public static void UpdateInventory(Packet _packet)
    {
        int count = _packet.ReadInt();

        instance.items.Clear();
        for (int i = 0; i < count; i++) { instance.items.Add(Resources.Load<ItemSettings>("Interactable Items/" + _packet.ReadString())); }
        instance.onItemChangedCallback.Invoke();
    }

    IEnumerator DestroyTreeI(Vector3 pos)
    {
        yield return new WaitForSeconds(2f);

        ThreadManager.ExecuteOnMainThread(() =>
        {
            using (Packet _packet = new Packet((int)ClientPackets.addProp))
            {
                _packet.Write(new Vector3(pos.x, pos.y + 0.5f, pos.z));
                _packet.Write(new Vector3(Random.Range(-5f, -5f), Random.Range(-180f, 180f), Random.Range(-5f, -5f)));
                _packet.Write(9);
                _packet.Write(1);

                ClientSend.SendUDPData(_packet);
            }
        });

        using (Packet _packet = new Packet((int)ClientPackets.destroyProp)) { _packet.Write(pos); ClientSend.SendTCPData(_packet); }
        PropsGeneration.instance.AddToPropPool(currentObject);
        currentObject = null;
    }

    #endregion
}