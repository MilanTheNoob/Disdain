using UnityEngine.UI;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DataManager : MonoBehaviour
{
    public static readonly string IP = "127.0.0.1";
    public static readonly int Port = 26951;

    public static readonly string TypeVersion = "Closed Proof Of Concept";
    public static readonly int MainVersion = 1;
    public static readonly int ContentVersion = 1;
    public static readonly int BugFixVersion = 1;

    public static DataManager instance;

    public delegate void SaveGameDelegate();
    public static SaveGameDelegate SaveGameCallback;

    public delegate void SaveFileReceiveDelegate();
    public static SaveFileReceiveDelegate SaveFileReceiveCallback;

    public static SaveFileClass SaveFile;
    public static SaveDataStruct SaveData;

    public static string Username;
    public static int Skin;
    public static int ID;
    public static bool HasLocalData = false;

    public static TCP tcp;

    public static Dictionary<int, Packet> RecvingPackets = new Dictionary<int, Packet>();
    public static Dictionary<int, PlayerManager> Players = new Dictionary<int, PlayerManager>();

    static int currentSkin;

    public static void PacketHandler(Packet packet, int id)
    {
        switch (id)
        {
            case 0: instance.StartCoroutine(ClientHandle.OnConnect(packet)); break; // OnConnect
            case 1: instance.StartCoroutine(ClientHandle.LauncherData(packet)); break; // LauncherData

            case 2: ClientHandle.LoginResponse(packet); break;
            case 3: ClientHandle.BotCheckResponse(packet); break;
            case 4: ClientHandle.BotCheckSubmitResponse(packet); break;

            case 5: ClientHandle.EmailRecieveResponse(packet); break;
            case 6: ClientHandle.UsernameResponse(packet); break;

            case 8: ClientHandle.LoginBotCheckResponse(packet); break;
            case 9: ClientHandle.LoginBotCheckSubmitResponse(packet); break;
            case 10: ClientHandle.LoginEmailSendResponse(packet); break;
            case 11: ClientHandle.LoginPasswordResponse(packet); break;
            case 12: ClientHandle.LoginEmailReceiveResponse(packet); break;

            case 13: ClientHandle.EnterLobbyResponse(packet); break;
            case 15: ClientHandle.LobbyRenderData(packet); break;
        }
    }

    #region State Enums

    public static ControlStateEnum ControlState;
    public static GameStateEnum GameState;
    public static PlayerStateEnum PlayerState;
    public static ServerStateEnum ServerState;
    public static ConnectionStateEnum ConnectionState;

    #endregion
    #region Init

    void Awake()
    {
        #region Singleton & DDOL

        transform.parent = null;
        DontDestroyOnLoad(gameObject);

        instance = this;

        #endregion
        #region UI Init
        /*
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            ControlState = ControlStateEnum.Mobile;
        }
        else if (Application.platform == RuntimePlatform.XboxOne || Application.platform == RuntimePlatform.PS4 ||
            Application.platform == RuntimePlatform.PS5)
        {
            ControlState = ControlStateEnum.Console;
        }
        else
        {
            ControlState = ControlStateEnum.PC;
        }
        */

        ControlState = ControlStateEnum.PC;

        #endregion
        #region Multiplayer Init

        PlayerState = PlayerStateEnum.Menu;
        GameState = GameStateEnum.Login;
        ServerState = ServerStateEnum.Entry;

        tcp = new TCP();
        tcp.Connect();

        #endregion
    }

    #endregion

    #region Networking

    static void Disconnect()
    {
        if (ConnectionState != ConnectionStateEnum.Connected) return;
        ConnectionState = ConnectionStateEnum.Offline;

        tcp.socket.Close();
    }

    private void OnApplicationQuit() { Disconnect(); }

    public static void LoadLobby() { instance.StartCoroutine(ILoadLobby()); }
    static IEnumerator ILoadLobby()
    {
        DataUIManager.FadeIn();

        SceneManager.LoadSceneAsync(3, LoadSceneMode.Single);
        while (SceneManager.GetActiveScene().buildIndex != 3) { yield return new WaitForSeconds(0.2f); }

        Debug.Log("FUCKKKK");
        ClientSend.SendEmpty(15);
    }

    #endregion
    #region Communication Classes (TCP & UDP)

    public class TCP
    {
        public TcpClient socket;

        NetworkStream stream;
        Packet receivedData;
        byte[] receiveBuffer;

        public void Connect()
        {
            socket = new TcpClient { ReceiveBufferSize = 16384, SendBufferSize = 4096 };

            receiveBuffer = new byte[16384];
            socket.BeginConnect(IP, Port, ConnectCallback, socket);
        }

        void ConnectCallback(IAsyncResult result)
        {
            socket.EndConnect(result);
            if (!socket.Connected) { return; }

            stream = socket.GetStream();
            stream.BeginRead(receiveBuffer, 0, 16384, ReceiveCallback, null);

            receivedData = new Packet();
        }

        public void SendData(byte[] data)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(data, 0, data.Length, null, null);
                }
            }
            catch { }
        }

        void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = stream.EndRead(result);
                if (byteLength <= 0) { Disconnect(); return; }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                receivedData.Reset(HandleData(data));
                stream.BeginRead(receiveBuffer, 0, 16384, ReceiveCallback, null);
            }
            catch
            {
                Disconnect();
            }
        }

        private bool HandleData(byte[] data)
        {
            ThreadManager.ExecuteOnMainThread(() =>
            {
                int id = BitConverter.ToInt32(data, 0);
                bool singular = BitConverter.ToBoolean(data, 4);

                if (RecvingPackets.ContainsKey(id))
                {
                    RecvingPackets[id].AddChunk(data, PacketHandler);
                }
                else if (singular)
                {
                    Packet packet = new Packet(data, PacketHandler);
                }
                else
                {
                    RecvingPackets.Add(id, new Packet(data, PacketHandler));
                }
            });

            return true;
        }

        void Disconnect()
        {
            Disconnect();

            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    #endregion
}

#region Storage Classes

[Serializable]
public class SerializedSaveDataStruct
{
    public string Data;
}

public class SaveDataStruct
{
    public string Username;
    public string Password;

    public int FPS = 30;
    public int RenderDistance = 800;

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

[Serializable]
public class SaveFileClass
{
    public int Seed = UnityEngine.Random.Range(0, 999999999);
    public Vector3 PlayerLocation;

    public int MapSize;
    public int VertsPerLine;

    public List<string> Inventory = new List<string>();
    public List<float> Vitals = new List<float>();

    #region Funcs

    /// <summary>
    /// Finds what chunk the given location falls in
    /// NOTE - Does not mean the chunk location exists/been generated yet!
    /// </summary>
    /// <param name="pos">The global position</param>
    /// <returns>The chunk coords</returns>
    public Vector2 GetChunk(Vector3 pos)
    {
        return new Vector2(Mathf.RoundToInt(pos.x / 64), Mathf.RoundToInt(pos.x / 64));
    }
    /*
    /// <summary>
    /// Returns chunk data based off inputted coords
    /// </summary>
    /// <param name="coord">The chunk's coordinates</param>
    /// <returns>The chunk data</returns>
    public ChunkClass ReturnChunk(Vector2 coord)
    {
        for (int i = 0; i < Chunks.Count; i++)
        {
            if (Chunks[i].Coords == coord) { return Chunks[i]; }
        }

        return null;
    }

    /// <summary>
    /// Finds a chunk based off given coords
    /// </summary>
    /// <param name="coord">The coords of the chunk (e.g. {0, 1}, {2, 5}, etc</param>
    /// <returns>The index of the chunk in the Save File</returns>
    public int FindChunk(Vector2 coord)
    {
        for (int i = 0; i < Chunks.Count; i++)
        {
            if (Chunks[i].Coords == coord)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Finds the respective chunk based off a given location
    /// </summary>
    /// <param name="pos">The global position</param>
    /// <returns>The index of the chunk in the Save File</returns>
    public int FindChunk(Vector3 pos) { return FindChunk(GetChunk(pos)); }

    /// <summary>
    /// Finds storage from the save file at specified location
    /// </summary>
    /// <param name="coord">The global position of the storage</param>
    /// <returns>Returns the index of the storage in its respective chunk</returns>
    public int FindStorage(Vector3 coord)
    {
        int chunk = FindChunk(GetChunk(coord));

        for (int i = 0; i < Chunks[chunk].Storage.Count; i++)
        {
            if (Chunks[chunk].Storage[i].Pos == coord)
            {
                return i;
            }
        }

        return -1;
    }
    */
    #endregion
}

#endregion
#region Global Enums

public enum ControlStateEnum
{
    Mobile,
    Console,
    PC
}

public enum GameStateEnum
{
    Placebo,
    Launcher,
    Login,
    Singleplayer,
    Multiplayer,
    Lobby
}

public enum PlayerStateEnum
{
    Uknown,
    Loading,
    Idle,
    Moving,
    Interacting,
    Menu
}

public enum ServerStateEnum
{
    Entry,
    Lobby,
    PrivateServer,
    ThirdPartyServer,
    Singleplayer
}

public enum ConnectionStateEnum
{
    Offline,
    Connected,
    Failed
}
#endregion
