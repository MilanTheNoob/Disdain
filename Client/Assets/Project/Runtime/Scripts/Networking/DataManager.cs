using UnityEngine.UI;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEngine;
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
    //public static UDP udp;

    public static Dictionary<int, PlayerManager> Players = new Dictionary<int, PlayerManager>();

    delegate void PacketHandler(Packet packet);
    static Dictionary<int, PacketHandler> packetHandlers;

    static int currentSkin;

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

        #endregion
        #region Multiplayer Init

        PlayerState = PlayerStateEnum.Menu  ;
        GameState = GameStateEnum.Login;
        ServerState = ServerStateEnum.Entry;

        tcp = new TCP();
        //udp = new UDP();
        tcp.Connect();

        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            // Basic Communication
            { (int)ServerPackets.OnConnect, ClientHandle.OnConnect },
            { (int)ServerPackets.LauncherData, ClientHandle.LauncherData }
            /*
            { (int)ServerPackets.LoginResponse, ClientHandle.LoginResponse },
            { (int)ServerPackets.SignupRespnse, ClientHandle.SignupResponse },
            // Lobby Packets
            { (int)ServerPackets.PlayerSpawnL, ClientHandle.PlayerSpawnL },
            { (int)ServerPackets.PlayerDataL, ClientHandle.PlayerDataL },
            { (int)ServerPackets.PlayerLeaveL, ClientHandle.PlayerLeaveL },
            // Singleplayer Packets
            { (int)ServerPackets.SaveFileTransfer, ClientHandle.SaveFileTransfer },
            { (int)ServerPackets.SaveFileTransferEnd, ClientHandle.SaveFileTransferEnd },
            { (int)ServerPackets.AddChunk, TerrainGenerator.AddChunk }
            // Private Game Server Packets
            */
        };

        #endregion
    }

    #endregion

    #region Scene Loading

    public static void ToLobby()
    {
        if (PlayerState == PlayerStateEnum.Loading) return;

        PlayerState = PlayerStateEnum.Loading;
        GameState = GameStateEnum.Lobby;

        //ActiveUIClass.LoadingAnim.SetTrigger("DefaultTransition");
        instance.StartCoroutine(ILoadScene(1));
    }

    public static void ToSave()
    {
        if (PlayerState == PlayerStateEnum.Loading) return;
        PlayerState = PlayerStateEnum.Loading;

        GameState = GameStateEnum.Singleplayer;
        TweeningLibrary.FadeOut(GameManager.ActiveUI.UI, 0.3f);

        //ClientSend.JoinSave();

        //ActiveUIClass.LoadingAnim.SetTrigger("DefaultTransition");
        instance.StartCoroutine(ILoadScene(2));
    }

    static IEnumerator ILoadScene(int scene)
    {
        yield return new WaitForSeconds(3f);
        //SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);

        PlayerState = PlayerStateEnum.Idle;
    }

    #endregion
    #region Networking

    static void Disconnect()
    {
        //ClientSend.SaveFileTransfer();

        if (ConnectionState != ConnectionStateEnum.Connected) return;
        ConnectionState = ConnectionStateEnum.Offline;

        tcp.socket.Close();
        //udp.socket.Close();
    }

    //public void Login() { ClientSend.Login(ActiveUIClass.LoginUsername.text, ActiveUIClass.LoginPassword.text); }
    //public void Signup() { ClientSend.Signup(ActiveUIClass.SignupUsername.text, ActiveUIClass.SignupPassword.text, currentSkin); }

    private void OnApplicationQuit() { Disconnect(); }
    //private void OnApplicationPause() { ClientSend.SaveFileTransfer(); }

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
            socket = new TcpClient
            {
                ReceiveBufferSize = 4096,
                SendBufferSize = 4096
            };

            receiveBuffer = new byte[4096];
            socket.BeginConnect(IP, Port, ConnectCallback, socket);
        }

        void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result);
            if (!socket.Connected) { return; }

            stream = socket.GetStream();
            stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);

            receivedData = new Packet();
        }

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
            }
            catch { }
        }

        void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                print("cmon");

                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0) { Disconnect(); return; }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);
            }
            catch
            {
                Disconnect();
            }
        }

        private bool HandleData(byte[] _data)
        {
            print("PLS");

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet())
                {
                    _packet.SetBytes(_data);
                    int _packetId = _packet.ReadInt();
                    packetHandlers[_packetId](_packet);
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

    /*

    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP() { endPoint = new IPEndPoint(IPAddress.Parse(DataManager.ip), DataManager.port); }

        public void Connect(int _localPort)
        {
            socket = new UdpClient(_localPort);

            socket.Connect(endPoint);
            socket.BeginReceive(ReceiveCallback, null);

            using (Packet _packet = new Packet())
            {
                SendData(_packet);
            };
        }

        //Sends data to the client via UDP
        public void SendData(Packet _packet)
        {
            try
            {
                _packet.InsertInt(ID);

                if (socket != null) { socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null); }
            }
            catch { }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                byte[] _data = socket.EndReceive(_result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (_data.Length < 4) { Disconnect(); return; }

                HandleData(_data);
            }
            catch { Disconnect(); }
        }

        private void HandleData(byte[] _data)
        {
            using (Packet _packet = new Packet(_data))
            {
                int _packetLength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_data))
                {
                    int _packetId = _packet.ReadInt();
                    packetHandlers[_packetId](_packet);
                }
            });
        }

        private void Disconnect()
        {
            Disconnect();

            endPoint = null;
            socket = null;
        }
    }
    
    */

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

    public List<ChunkClass> Chunks = new List<ChunkClass>();
    public List<string> PropTypes = new List<string>();

    #region Nested Classes

    [Serializable]
    public class InventorySlotClass
    {
        public string Item;
        public int Quantity;
    }
    [Serializable]
    public class ChunkClass
    {
        public List<PropTypeClass> PropTypes = new List<PropTypeClass>();
        public List<StorageClass> Storage = new List<StorageClass>();

        public float[,] HeightMap;
        public Vector2 Coords;
    }
    [Serializable]
    public class PropTypeClass
    {
        public List<PropClass> Props = new List<PropClass>();
    }
    [Serializable]
    public class PropClass
    {
        public int ID;

        public Vector3 Pos;
        public Vector3 Scale;
        public Vector3 Euler;
    }
    [Serializable]
    public class StorageClass
    {
        public string ID;

        public Vector3 Pos;
        public Vector3 Rot;

        public List<string> Items;
    }

    #endregion
    #region Funcs

    /// <summary>
    /// Finds what chunk the given location falls in
    /// NOTE - Does not mean the chunk location exists/been generated yet!
    /// </summary>
    /// <param name="pos">The global position</param>
    /// <returns>The chunk coords</returns>
    public Vector2 GetChunk(Vector3 pos)
    {
        float meshSize = TerrainGenerator.instance.GenerationSettings.MeshSetting.meshWorldSize;
        return new Vector2(Mathf.RoundToInt(pos.x / meshSize), Mathf.RoundToInt(pos.x / meshSize));
    }

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
