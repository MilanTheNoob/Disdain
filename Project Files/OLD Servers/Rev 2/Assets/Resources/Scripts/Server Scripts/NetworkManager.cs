using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public static ServerDataClass ServerData;

    public static List<string> OnlinePlayers = new List<string>();
    public static Dictionary<int, Player> Players = new Dictionary<int, Player>();

    public GameObject playerPrefab;
    public LayerMask itemsLayer;

    [Space]

    public GenerationSettings GS;

    void Awake()
    {
        if (File.Exists(Application.persistentDataPath + "/Autosave.txt"))
        {
            ServerData = new ServerDataClass(File.ReadAllBytes(Application.persistentDataPath + "/Autosave.txt"));
        }
        else
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Autosave/");
            ServerData = new ServerDataClass();
        }

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;

        instance = this;
        Server.Startup();
    }

    void OnApplicationQuit()
    {
        File.WriteAllBytes(Application.persistentDataPath + "/Autosave.txt", ServerData.Serialize());
        for (int i = 0; i < Server.Clients.Count; i++) { SavePlayerData(i); }

        Server.Stop();
    }

    #region Client Actions

    public Player InstantiatePlayer(int id)
    {
        Player player = Instantiate(playerPrefab, new Vector3(0f, 500f, 0f), Quaternion.identity).GetComponent<Player>();

        Players.Add(id, player);
        return player;
    }

    public void GetPlayerData(int id)
    {
        if (ServerData.Accounts.ContainsKey(Server.Clients[id].Username))
        {
            byte[] data = File.ReadAllBytes(Application.persistentDataPath + "/Autosave/" + Server.Clients[id].Username + ".txt");
            Server.Clients[id].SaveFile = new SaveFileClass(id, true, data, 0);
        }
    }

    public void SavePlayerData(int id)
    {
        Client client = Server.Clients[id];
        ServerData.Accounts[client.Username].Skin = client.Skin;
        File.WriteAllBytes(Application.persistentDataPath + "/Autosave/" + client.Username + ".json",
            client.SaveFile.Serialize());
    }

    #endregion

    private void FixedUpdate()
    {
        ServerSend.PlayerDataL(Server.LobbyClients);
    }
}

[Serializable]
public class ServerDataClass
{
    public Dictionary<string, Account> Accounts = new Dictionary<string, Account>();

    #region Funcs

    public ServerDataClass(byte[] data)
    {
        Packet packet = new Packet(data);

        int accountsLength = packet.ReadInt();
        for (int i = 0; i < accountsLength; i++)
        {
            Accounts.Add(packet.ReadString(), new Account
            {
                Skin = packet.ReadInt(),
                Password = packet.ReadString()
            });

        }
    }

    public ServerDataClass() { }

    public byte[] Serialize()
    {
        Packet packet = new Packet();

        packet.Write(Accounts.Count);
        for (int i = 0; i < Accounts.Count; i++)
        {
            packet.Write(Accounts.ElementAt(i).Key);
            packet.Write(Accounts.ElementAt(i).Value.Skin);
            packet.Write(Accounts.ElementAt(i).Value.Password);
        }

        return packet.buffer.ToArray();
    }

    #endregion
    #region Nested Classes

    public class Account
    {
        public int Skin;
        public string Password;
    }

    #endregion
}

[Serializable]
public class SaveFileClass
{
    public bool Generated;

    public int Seed;
    public Vector3 PlayerLocation;

    public int MapSize;
    public int VertsPerLine;

    public List<string> Inventory = new List<string>();
    public List<float> Vitals = new List<float>();

    public List<ChunkClass> Chunks = new List<ChunkClass>();

    public Noise BiomeNoise = new Noise();
    public Noise GenerationNoise = new Noise();

    #region Init

    /// <summary>
    /// Will load serialized data into a new save file
    /// </summary>
    /// <param name="client">The client index in ther Server's Dictionary</param>
    /// <param name="sendToClient">Whether or not to send the save file to the client</param>
    /// <param name="data">The save data in its serialized form</param>
    /// <param name="readPos">Where from to start reading the data in the array</param>
    public SaveFileClass(int client, bool sendToClient, byte[] data, int readPos)
    {
        Packet packet = new Packet(data);
        packet.readPos = readPos;

        Seed = packet.ReadInt();
        PlayerLocation = packet.ReadVector3();

        GenerationNoise.Seed = Seed;
        BiomeNoise.Seed = Seed + 1;

        MapSize = packet.ReadInt();
        VertsPerLine = packet.ReadInt();

        Inventory = packet.ReadStringList();
        Vitals = packet.ReadFloatList();

        int chunkLength = packet.ReadInt();
        for (int i = 0; i < chunkLength; i++)
        {
            ChunkClass chunk = new ChunkClass
            {
                Coords = packet.ReadVector2(),
                HeightMap = new float[VertsPerLine, VertsPerLine]
            };

            int b = 0;
            for (int x = 0; x < VertsPerLine; x++)
            {
                for (int y = 0; y < VertsPerLine; y++)
                {
                    chunk.HeightMap[x, y] = packet.ReadFloat();
                    b++;
                }
            }

            int propTypesLength = packet.ReadInt();
            for (int q = 0; q < propTypesLength; q++)
            {
                PropTypeData propType = new PropTypeData();

                int propLength = packet.ReadInt();
                for (int j = 0; j < propLength; j++)
                {
                    PropData prop = new PropData
                    {
                        ID = packet.ReadInt(),
                        Pos = packet.ReadVector3(),
                        Scale = packet.ReadVector3(),
                        Euler = packet.ReadVector3()
                    };
                    propType.Props.Add(prop);
                }
                chunk.PropTypes.Add(propType);
            }

            for (int j = 0; j < packet.ReadInt(); j++)
            {
                StorageClass prop = new StorageClass
                {
                    ID = packet.ReadString(),
                    Pos = packet.ReadVector3(),
                    Rot = packet.ReadVector3(),
                    Items = packet.ReadStringList()
                };
                chunk.Storage.Add(prop);
            }

            Chunks.Add(chunk);
        }

        if (sendToClient) ServerSend.SaveFileTransfer(client, this);
    }

    /// <summary>
    /// Will initialize and generate a new save file from scratch
    /// </summary>
    /// <param name="client">The client index in the Server's Dictionary</param>
    /// <param name="sendToClient">Whether or not to send the save file to the client</param>
    public SaveFileClass(int client, bool sendToClient)
    {
        Generate(client);
        Generated = true;
        if (sendToClient) ServerSend.SaveFileTransfer(client, this);
    }

    #endregion
    #region Serializing

    public byte[] Serialize()
    {
        Packet packet = new Packet();

        packet.Write(Seed);
        packet.Write(PlayerLocation);

        packet.Write(MapSize);
        packet.Write(VertsPerLine);

        packet.Write(Inventory);

        packet.Write(Vitals.Count);
        for (int i = 0; i < Vitals.Count; i++) { packet.Write(Vitals[i]); }

        packet.Write(Chunks.Count);
        for (int i = 0; i < Chunks.Count; i++)
        {
            packet.Write(Chunks[i].Coords);

            int c = 0;
            for (int x = 0; x < VertsPerLine; x++)
            {
                for (int y = 0; y < VertsPerLine; y++)
                {
                    packet.Write(Chunks[i].HeightMap[x, y]);
                    c++;
                }
            }

            packet.Write(Chunks[i].PropTypes.Count);
            for (int b = 0; b < Chunks[i].PropTypes.Count; b++)
            {
                packet.Write(Chunks[i].PropTypes[b].Props.Count);
                for (int j = 0; j < Chunks[i].PropTypes[b].Props.Count; j++)
                {
                    packet.Write(Chunks[i].PropTypes[b].Props[j].ID);
                    packet.Write(Chunks[i].PropTypes[b].Props[j].Pos);
                    packet.Write(Chunks[i].PropTypes[b].Props[j].Scale);
                    packet.Write(Chunks[i].PropTypes[b].Props[j].Euler);
                }
            }

            packet.Write(Chunks[i].Storage.Count);
            for (int j = 0; j < Chunks[i].Storage.Count; j++)
            {
                packet.Write(Chunks[i].Storage[j].ID);
                packet.Write(Chunks[i].Storage[j].Pos);
                packet.Write(Chunks[i].Storage[j].Rot);

                packet.Write(Chunks[i].Storage[j].Items);
            }
        }

        return packet.buffer.ToArray();
    }

    #endregion

    #region Generation

    public void Generate(int client)
    {
        Seed = UnityEngine.Random.Range(0, 999999999);

        GenerationSettings gs = NetworkManager.instance.GS;
        MapSize = Mathf.RoundToInt(1000 / gs.MeshSetting.meshWorldSize);
        VertsPerLine = gs.MeshSetting.numVertsPerLine;

        GenerationNoise.Seed = Seed;
        BiomeNoise.Seed = Seed + 1;

        for (int x = -MapSize; x < MapSize; x++)
        {
            for (int y = -MapSize; y < MapSize; y++)
            {
                Vector2 coord = new Vector2(x, y);
                BiomeClass biome = gs.GetBiome(coord, BiomeNoise);

                ChunkClass chunk = new ChunkClass
                {
                    Coords = coord,
                    HeightMap = Generation.GenerateHeightMap(gs, coord * gs.MeshSetting.meshWorldSize / gs.MeshSetting.meshScale, coord, biome, GenerationNoise)
                };
                chunk.PropTypes = GenerateProps(chunk, gs.MeshSetting, biome);

                Chunks.Add(chunk);
            }
        }
    }

    public void GenerateChunk(int client, Vector2 coord)
    {
        if (FindChunk(coord) != -1) return;

        GenerationSettings gs = NetworkManager.instance.GS;
        Noise noise = new Noise();
        BiomeClass biome = gs.GetBiome(coord, noise);

        ChunkClass chunk = new ChunkClass
        {
            Coords = coord,
            HeightMap = Generation.GenerateHeightMap(gs, coord * gs.MeshSetting.meshWorldSize / gs.MeshSetting.meshScale, coord, biome, noise)
        };
        chunk.PropTypes = GenerateProps(chunk, gs.MeshSetting, biome);

        Chunks.Add(chunk);
        ServerSend.AddChunk(client, chunk);
    }

    public List<PropTypeData> GenerateProps(ChunkClass chunk, MeshSettings ms, BiomeClass Biome)
    {
        GenerationSettings gs = NetworkManager.instance.GS;
        List<PropTypeData> PropTypes = new List<PropTypeData>();
        Vector2 topLeft = new Vector2(-1, 1) * gs.MeshSetting.meshWorldSize / 2f;

        for (int i = 0; i < Biome.PropTypes.Length; i++)
        {
            PropTypes.Add(new PropTypeData());

            for (int j = 0; j < Biome.PropTypes[i].Props.Length; j++)
            {
                if (Biome.PropTypes[i].Props[j].GenerateType == GenerateTypeEnum.Perlin)
                {
                    for (int x = 0; x < ms.numVertsPerLine - 3; x++)
                    {
                        for (int y = 0; y < ms.numVertsPerLine - 3; y++)
                        {
                            if (Mathf.PerlinNoise(x, y) > Biome.PropTypes[i].Props[j].PerlinChance)
                            {
                                PropTypes[i].Props.Add(AddProp(new Vector3(x, y, chunk.HeightMap[x, y]) / ms.meshScale, Biome.PropTypes[i].Props[j]));

                                Vector2 percent = new Vector2(x - 1, y - 1) / (gs.MeshSetting.numVertsPerLine - 3);
                                Vector2 vertexPosition2D = topLeft + new Vector2(percent.x, -percent.y) * gs.MeshSetting.meshWorldSize;
                                    
                                PropTypes[i].Props.Add(AddProp(new Vector3(vertexPosition2D.x, chunk.HeightMap[x, y], vertexPosition2D.y),
                                    Biome.PropTypes[i].Props[j]));
                            }
                        }
                    }
                }
                else if (Biome.PropTypes[i].Props[j].GenerateType == GenerateTypeEnum.Random)
                {
                    int count = 0;
                    int amount = UnityEngine.Random.Range(Biome.PropTypes[i].Props[j].PerChunkMin, Biome.PropTypes[i].Props[j].PerChunkMax);

                    while (count < amount)
                    {
                        int randX = UnityEngine.Random.Range(0, ms.numVertsPerLine - 3);
                        int randY = UnityEngine.Random.Range(0, ms.numVertsPerLine - 3);

                        Vector2 percent = new Vector2(randX - 1, randY - 1) / (gs.MeshSetting.numVertsPerLine - 3);
                        Vector2 vertexPosition2D = topLeft + new Vector2(percent.x, -percent.y) * gs.MeshSetting.meshWorldSize;

                        PropTypes[i].Props.Add(AddProp(new Vector3(vertexPosition2D.x, chunk.HeightMap[randX, randY], vertexPosition2D.y),
                            Biome.PropTypes[i].Props[j]));
                        count++;
                    }
                }
                else if (Biome.PropTypes[i].Props[j].GenerateType == GenerateTypeEnum.RandomChance)
                {
                    if (Biome.PropTypes[i].Props[j].Chance > UnityEngine.Random.Range(0f, 1f))
                    {
                        int randX = UnityEngine.Random.Range(0, ms.numVertsPerLine - 3);
                        int randY = UnityEngine.Random.Range(0, ms.numVertsPerLine - 3);

                        Vector2 percent = new Vector2(randX - 1, randY - 1) / (gs.MeshSetting.numVertsPerLine - 3);
                        Vector2 vertexPosition2D = topLeft + new Vector2(percent.x, -percent.y) * gs.MeshSetting.meshWorldSize;

                        PropTypes[i].Props.Add(AddProp(new Vector3(vertexPosition2D.x, chunk.HeightMap[randX, randY], vertexPosition2D.y),
                            Biome.PropTypes[i].Props[j]));
                    }
                }
            }
        }
        return PropTypes;
    }

    public PropData AddProp(Vector3 pos, PropClass propClass)
    {
        float rScale = UnityEngine.Random.Range(propClass.MinScale, propClass.MaxScale);
        return new PropData
        {
            ID = propClass.ID,
            Pos = pos,
            Scale = new Vector3(rScale, rScale, rScale),
            Euler = new Vector3(UnityEngine.Random.Range(propClass.MinRot.x, propClass.MaxRot.x),
            UnityEngine.Random.Range(propClass.MinRot.y, propClass.MaxRot.y),
            UnityEngine.Random.Range(propClass.MinRot.z, propClass.MaxRot.z))
        };
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
        float meshSize = NetworkManager.instance.GS.MeshSetting.meshWorldSize;
        return new Vector2(Mathf.RoundToInt(pos.x / meshSize), Mathf.RoundToInt(pos.x / meshSize));
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
        public List<PropTypeData> PropTypes = new List<PropTypeData>();
        public List<StorageClass> Storage = new List<StorageClass>();

        public float[,] HeightMap;
        public Vector2 Coords;
    }
    [Serializable]
    public class PropTypeData
    {
        public List<PropData> Props = new List<PropData>();
    }
    [Serializable]
    public class PropData
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
}