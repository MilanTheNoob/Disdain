using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEngine;
using System.Collections;

public class SavingManager : MonoBehaviour
{
    public Animator Mobile;
    public Animator PC;

    public static SavingManager instance;

    public delegate void SaveGameDelegate();
    public static SaveGameDelegate SaveGameCallback;

    public static SaveStruct SaveFile;
    public static SaveDataStruct SaveData;

    public static string ip;
    public static int port;
    public static string username;
    public static int skin;

    static bool loading;

    string _ip = "127.0.0.1";

    public enum GameStateEnum
    {
        Singleplayer,
        Multiplayer,
        Lobby,
        Login
    }
    static public GameStateEnum GameState;

    TcpClient socket;

    private NetworkStream stream;
    private Packet receivedData;
    private byte[] receiveBuffer;

    void Awake()
    {
        transform.parent = null;
        DontDestroyOnLoad(gameObject);

        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }

        if (BinarySerializer.HasSaved("DisdainSaveData.txt"))
        {
            SaveData = BinarySerializer.Load<SaveDataStruct>("DisdainSaveData.txt");
        }
        else
        {
            SaveData = new SaveDataStruct();
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            Mobile.gameObject.SetActive(true);
            PC.gameObject.SetActive(false);
        }
        else
        {
            Mobile.gameObject.SetActive(false);
            PC.gameObject.SetActive(true);
        }

        GameObject UI = GameObject.Find("UI");
        UI.SetActive(false);
        StartCoroutine(IEntry(UI));

        GameState = GameStateEnum.Login;
    }

    IEnumerator IEntry(GameObject UI)
    {
        yield return new WaitForSeconds(6f);

        UI.SetActive(true);
        TweeningLibrary.FadeIn(UI, 0.3f);
    }

    #region Scene Loading

    public static void ToLobby(string _ip, int _port, string _username, int _skin)
    {
        if (loading) return;
        loading = true;

        ip = _ip;
        port = _port;
        username = _username;
        skin = _skin;

        GameState = GameStateEnum.Lobby;
        TweeningLibrary.FadeOut(GameManager.ActiveUI.UI, 0.3f);

        instance.Mobile.SetTrigger("DefaultTransition");
        instance.PC.SetTrigger("DefaultTransition");
        instance.StartCoroutine(ILoadScene(1));
    }

    public static void ToSave()
    {
        if (loading) return;
        loading = true;

        SaveFile = new SaveStruct();
        GameState = GameStateEnum.Singleplayer;
        TweeningLibrary.FadeOut(GameManager.ActiveUI.UI, 0.3f);

        instance.Mobile.SetTrigger("DefaultTransition");
        instance.PC.SetTrigger("DefaultTransition");

        LobbyClient.instance.tcp.Disconnect();
        instance.StartCoroutine(ILoadScene(2));
    }

    static IEnumerator ILoadScene(int scene)
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);

        loading = false;
    }

    #endregion

    public static void SaveGame()
    {
        /*
        if (instance == null) { return; }

        instance.socket = new TcpClient
        {
            ReceiveBufferSize = 4096,
            SendBufferSize = 4096
        };

        instance.receiveBuffer = new byte[4096];
        instance.socket.BeginConnect(instance._ip, 26950, instance.ConnectCallback, instance.socket);
        */
    }

    #region Network Saving

    void ConnectCallback(IAsyncResult _result)
    {
        socket.EndConnect(_result);

        if (!socket.Connected)
        {
            return;
        }

        stream = socket.GetStream();
        receivedData = new Packet();

        using (Packet packet = new Packet(2))
        {
            packet.Write(JsonUtility.ToJson(SaveFile));
            SendData(packet);
        }
    }

    public void SendData(Packet _packet)
    {
        try
        {
            if (socket != null)
            {
                stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error sending data to server via TCP: {_ex}");
        }
    }

    #endregion

    void OnApplicationPause() { SaveGame(); }
    void OnApplicationQuit() { SaveGame(); }

    [Serializable]
    public class SaveStruct
    {
        public int seed = UnityEngine.Random.Range(0, 999999);
        public float funds = 0f;

        public List<string> inventoryItems = new List<string>();
        public List<StructureData> structures = new List<StructureData>();
        public List<float> vitals = new List<float>();

        public Dictionary<Vector2, ChunkData> Chunks = new Dictionary<Vector2, ChunkData>();
        public Dictionary<Vector3, StorageData> storage = new Dictionary<Vector3, StorageData>();

        public Vector3 playerPos = new Vector3(0f, 0f, 0f);
        public bool LQGeneration = false;
    }

    [Serializable]
    public class SaveDataStruct
    {
        public List<SavedServerStruct> Servers = new List<SavedServerStruct>();
        public GPlayDataStruct GPlayData = new GPlayDataStruct();
        public SettingsData SettingsData = new SettingsData();
    }

    [Serializable]
    public class GPlayDataStruct
    {
        public bool FirstTime = false;
        public bool Morning = false;
        public bool EarlyBird = false;
        public bool RIP = false;
        public bool RubyGem = false;
        public bool BuySkin = false;

        public int GameTime = 0;
        public bool OneHour = false;
        public bool FiveHour = false;

        public int LocalExploredChunks = 0;
        public int ExploredChunks = 0;

        public int LocalCraftedRubies = 0;
        public int CraftedRubies = 0;

        public int LocalDeaths = 0;
        public int Deaths = 0;
    }

    [Serializable]
    public class SettingsData
    {
        public int FPS = 30;
        public int RenderDistance = 80;

        public bool AA = true;
        public bool HDR = true;

        public bool Tonemapping = true;
        public bool DepthOfField = false;
        public bool MotionBlur = true;
        public bool Vignette = true;
        public bool Bloom = true;

        public float Sensitivity = 14;

        public float MainAudioLevel = 0f;
        public float SFAudioLevel = 0f;
        public float MusicAudioLevel = 0f;
    }
}

[Serializable]
public class ChunkData
{
    public List<PropData> Props = new List<PropData>();
    public List<PropData> Vehicles = new List<PropData>();
}

[Serializable]
public class PropData
{
    public string Name;
    public Vector3 Position = new Vector3();
    public Vector3 Scale = new Vector3();
    public Quaternion Rotation = new Quaternion();
}

[Serializable]
public class StorageData
{
    public List<string> items = new List<string>();
}

[Serializable]
public class StructureData
{
    public string name;

    public Vector2 coord;
    public Vector3 pos;
    public Quaternion rot;
}

[Serializable]
public class SavedServerStruct
{
    public string localName;
    public string serverIp;
    public int serverPort;
    public bool official;
}
