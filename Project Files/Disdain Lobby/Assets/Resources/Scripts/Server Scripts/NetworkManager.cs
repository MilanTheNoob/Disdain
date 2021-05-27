using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefab;
    public LayerMask itemsLayer;

    public Dictionary<int, Player> players = new Dictionary<int, Player>();

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;

        instance = this;
        Server.Start(50, 26950);
    }

    void OnApplicationQuit()
    {
        Server.Stop();
    }

    public Player InstantiatePlayer(int id, string ip)
    {
        Player player = Instantiate(playerPrefab, new Vector3(0f, 25f, 0f), Quaternion.identity).GetComponent<Player>();

        players.Add(id, player);
        return player;
    }
}

[System.Serializable]
public class StorageData
{
    public Vector3 pos;
    public List<string> items = new List<string>();
}

[System.Serializable]
public class SaveData
{
    public List<string> playerIps = new List<string>();
    public List<PlayerData> playerDatas = new List<PlayerData>();

    public List<SerializableChunkDataStruct> chunkData = new List<SerializableChunkDataStruct>();
    public List<StorageData> storage = new List<StorageData>();
}

[System.Serializable]
public class PlayerData
{
    public List<string> inventory = new List<string>();

    public float health;
    public float hunger;

    public Vector3 pos;
}

[System.Serializable]
public class SerializablePropDataStruct
{
    public Vector3 pos;
    public Vector3 rot;
    public int group;
    public int prop;
}

[System.Serializable]
public class SerializableStructureDataStruct
{
    public Vector3 pos;
    public Vector3 rot;
    public string structure;
}

[System.Serializable]
public class SerializableChunkDataStruct
{
    public Vector2 coord;
    public HeightMap heightMap;

    public List<SerializablePropDataStruct> props = new List<SerializablePropDataStruct>();
    public List<SerializableStructureDataStruct> structures = new List<SerializableStructureDataStruct>();
}