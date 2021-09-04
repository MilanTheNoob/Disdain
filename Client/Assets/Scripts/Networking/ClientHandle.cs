﻿using System.Net;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class ClientHandle : MonoBehaviour
{
    #region Init

    public static IEnumerator OnConnect(Packet packet)
    {
        DataManager.ID = packet.ReadInt();
        ClientSend.OnConnect();

        print("Client has connected received the 'OnConnect' packet from the server");
        bool loadToLogin = false;

        if (File.Exists(Application.persistentDataPath + "/SaveData.d"))
        {
            try
            {
                DataManager.SaveData = JsonUtility.FromJson<SaveDataStruct>(BinarySerializer.Load
                    <SerializedSaveDataStruct>(Application.persistentDataPath + "/SaveData.d").Data);

                DataManager.HasLocalData = true;
                loadToLogin = true;
            }
            catch (Exception x)
            {
                //DataUIManager.PopUpError("Failed to load save data from local PC. Likely caused to corrupted data, please login in again", false);
                DataManager.HasLocalData = false;
            }
        }
        else
        {
            DataManager.HasLocalData = false;
        }

        if (loadToLogin)
        {
            ClientSend.Login(DataManager.SaveData.Username, DataManager.SaveData.Password);
        }
        else
        {
            DataManager.PlayerState = PlayerStateEnum.Loading;
            DataManager.GameState = GameStateEnum.Login;

            SceneManager.LoadSceneAsync(2, LoadSceneMode.Single);
            while (SceneManager.GetActiveScene().buildIndex != 2) { yield return new WaitForSeconds(0.1f); }

            GameManager.InstantToggleUISection(0);
            DataUIManager.FadeOut();
        }
    }

    #endregion
    #region Launcher

    public static IEnumerator LauncherData(Packet packet)
    {
        LauncherDataClass launcherData = new LauncherDataClass
        {
            UpdateNeeded = packet.ReadBool(),
            CurrentVersion = DataManager.TypeVersion + " v" + DataManager.MainVersion +
            ", vContent - v" + DataManager.ContentVersion + ", vBugfix - v" + DataManager.BugFixVersion
        };

        if (launcherData.UpdateNeeded)
        {
            launcherData.UpdateVersion = packet.ReadString() + " v" + DataManager.MainVersion +
                ", vContent - v" + DataManager.ContentVersion + ", vBugfix - v" + DataManager.BugFixVersion;
        }
        int patches = packet.ReadInt();
        launcherData.UpdatePatchNotes = new LauncherDataClass.UpdatePatchNotesClass[patches];
        for (int i = 0; i < patches; i++)
        {
            LauncherDataClass.UpdatePatchNotesClass patchNote = new LauncherDataClass.UpdatePatchNotesClass();
            patchNote.Title = packet.ReadString();

            int paragraphs = packet.ReadInt();
            patchNote.Paragraphs = new string[paragraphs];
            for (int j = 0; j < paragraphs; j++) { patchNote.Paragraphs[j] = packet.ReadString(); }

            launcherData.UpdatePatchNotes[i] = patchNote;
        }

        DataManager.PlayerState = PlayerStateEnum.Loading;
        DataManager.GameState = GameStateEnum.Launcher;

        SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
        while (SceneManager.GetActiveScene().buildIndex != 1) { yield return new WaitForSeconds(0.1f); }

        LauncherManager.SetLauncherData(launcherData);
        DataUIManager.FadeOut();
    }

    #endregion

    #region Signup

    public static void LoginResponse(Packet packet)
    {
        if (packet.ReadBool())
        {
            ClientSend.LauncherData();
        }
        else
        {
            DataManager.instance.StartCoroutine(ILoginResponse());
        }
    }

    static IEnumerator ILoginResponse()
    {
        DataManager.PlayerState = PlayerStateEnum.Loading;
        DataManager.GameState = GameStateEnum.Login;

        SceneManager.LoadSceneAsync(2, LoadSceneMode.Single);
        while (SceneManager.GetActiveScene().buildIndex != 2) { yield return new WaitForSeconds(0.1f); }

        GameManager.ToggleUISection(0);
        DataUIManager.FadeOut();
    }

    public static void BotCheckResponse(Packet packet)
    {
        if (packet.ReadByte() == 1)
        {
            int size = packet.ReadInt();

            byte[] textureData = new byte[size];
            packet.buffer.CopyTo(10, textureData, 0, size);

            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(textureData);
            texture.Apply();

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));

            LoginManager.instance.SetBotCheckImg(sprite);
        }
    }

    public static void BotCheckSubmitResponse(Packet packet)
    {
        if (packet.ReadBool())
        {
            GameManager.ToggleUISection(3);
        }
        else
        {
            int tries = packet.ReadByte();

            LoginManager.instance.BotCheck.TriesLeft.text = "Tries Left: " + tries;
            if (tries > 3 || tries < 1) { Application.Quit(); }
        }
    }

    public static void EmailRecieveResponse(Packet packet)
    {
        if (packet.ReadBool())
        {
            GameManager.ToggleUISection(6);
        }
        else
        {
            LoginManager.instance.EmailReceive.Error.gameObject.SetActive(true);
            LoginManager.instance.EmailReceive.Error.text = "Na bro, that secret code isn't right.";
        }
    }

    public static void UsernameResponse(Packet packet)
    {
        if (packet.ReadBool())
        {
            GameManager.ToggleUISection(7);
        }
        else
        {
            LoginManager.instance.SetUsername.Error.gameObject.SetActive(true);
            LoginManager.instance.SetUsername.Error.text = packet.ReadString();
        }
    }

    #endregion
    #region Login

    public static void LoginBotCheckResponse(Packet packet)
    {
        if (packet.ReadBool())
        {
            int size = packet.ReadInt();

            byte[] textureData = new byte[size];
            packet.buffer.CopyTo(10, textureData, 0, size);

            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(textureData);
            texture.Apply();

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));

            LoginManager.instance.SetLoginBotCheckImg(sprite);
        }
    }

    public static void LoginBotCheckSubmitResponse(Packet packet)
    {
        if (packet.ReadBool())
        {
            GameManager.ToggleUISection(12);
        }
        else
        {
            int tries = packet.ReadByte();

            LoginManager.instance.LoginBotCheck.TriesLeft.text = "Tries Left: " + tries;
            if (tries > 3 || tries < 1) { Application.Quit(); }
        }
    }
    
    public static void LoginEmailSendResponse(Packet packet)
    {
        if (packet.ReadBool())
        {
            GameManager.ToggleUISection(13);
        }
        else
        {
            LoginManager.instance.LoginPassword.Error.text = "Huh, you errr, don't have an account attached to this email";
        }
    }

    public static void LoginPasswordResponse(Packet packet)
    {
        if (packet.ReadBool())
        {
            GameManager.ToggleUISection(14);
        }
        else
        {
            LoginManager.instance.LoginPassword.Error.gameObject.SetActive(true);
            LoginManager.instance.LoginPassword.Error.text = "Password no correct";
        }
    }

    public static void LoginEmailReceiveResponse(Packet packet)
    {
        if (packet.ReadBool())
        {
            GameManager.ToggleUISection(9);
        }
        else
        {
            LoginManager.instance.LoginEmailSend.Error.gameObject.SetActive(true);
            LoginManager.instance.LoginEmailSend.Error.text = "Bruh, this secret code isn't right";
        }
    }

    #endregion

    #region Lobby

    public static void EnterLobbyResponse(Packet packet)
    {
        if (packet.ReadBool())
        {
            DataUIManager.FadeOut();
            
            GameObject player = Instantiate(Resources.Load<GameObject>("Player"), packet.ReadVector3(), Quaternion.identity);
            GameManager.ActivePlayer = player;
            GameManager.ActiveCamera = player.GetComponentInChildren<Camera>();
        }
    }

    public static void LobbyRenderData(Packet packet)
    {
        for (int x = -5; x < 5; x++)
        {
            for (int y = -5; y < 5; y++)
            {
                Vector2 coord = new Vector2(packet.ReadInt(), packet.ReadInt());
                if (packet.ReadBool()) TerrainGenerator.AddChunk(packet, coord);
            }
        }
    }

    #endregion

    #region Unimplemented
    /*
    public static void LoginResponse(Packet packet)
    {
        if (packet.ReadBool())
        {
            DataManager.Username = packet.ReadString();
            DataManager.Skin = packet.ReadInt();
        }
        else
        {
            //DataManager.ActiveUIClass.Error.text = packet.ReadString();
        }
    }

    public static void SignupResponse(Packet packet)
    {
        if (packet.ReadBool())
        {
            DataManager.Username = packet.ReadString();
            DataManager.Skin = packet.ReadInt();
        }
        else
        {
            //DataManager.ActiveUIClass.Error.text = packet.ReadString();
        }
    }

    public static void PlayerSpawnL(Packet _packet)
    {
        int id = _packet.ReadInt();
        string username = _packet.ReadString();
        GameObject player = Instantiate(Resources.Load<GameObject>("Player"), _packet.ReadVector3(), _packet.ReadQuaternion());

        if (id == DataManager.ID)
        {
            player.transform.name = "Player";

            GameManager.ActivePlayer = player;
            GameManager.ActiveCamera = player.GetComponentInChildren<Camera>();
        }
        else
        {
            player.transform.name = "External Player - " + username; 
            Destroy(player.GetComponentInChildren<Camera>().gameObject);
        }

        PlayerManager pm = player.GetComponent<PlayerManager>();
        pm.id = id;
        pm.username = username;

        DataManager.Players.Add(id, pm);
    }

    public static void PlayerDataL(Packet _packet)
    {
        for (int i = 0; i < DataManager.Players.Count; i++)
        {
            DataManager.Players.ElementAt(i).Value.transform.position = _packet.ReadVector3();
            if (DataManager.Players.ElementAt(i).Key == DataManager.ID)
            {
                _packet.ReadVector3();
            }
            else
            {
                DataManager.Players.ElementAt(i).Value.transform.eulerAngles = _packet.ReadVector3();
            }
        }
    }

    public static void PlayerLeaveL(Packet _packet)
    {
        Destroy(DataManager.Players[_packet.ReadInt(false)].gameObject);
        DataManager.Players.Remove(_packet.ReadInt());
    }

    public static void SaveFileTransfer(Packet _packet)
    {
        List<byte> chunk = _packet.readableBuffer.ToList();
        int count = _packet.ReadInt();

        #region Removing
        chunk.RemoveAt(0);
        chunk.RemoveAt(0);
        chunk.RemoveAt(0);
        chunk.RemoveAt(0);

        chunk.RemoveAt(0);
        chunk.RemoveAt(0);
        chunk.RemoveAt(0);
        chunk.RemoveAt(0);

        #endregion
        chunks.Add(count, chunk);
    }

    public static void SaveFileTransferEnd(Packet _packet)
    {
        DataManager.instance.StartCoroutine(ISaveFileTransferEnd(_packet));
    }
    static IEnumerator ISaveFileTransferEnd(Packet _packet)
    {
        int count = _packet.ReadInt();

        while (chunks.Count < count)
        {
            yield return new WaitForSeconds(0.1f);
        }
        Packet packet = new Packet();

        for (int i = 0; i < chunks.Count; i++) { packet.Write(chunks[i].ToArray()); }
        packet.readableBuffer = packet.buffer.ToArray();
        chunks.Clear();

        SaveFileClass saveFile = new SaveFileClass();

        saveFile.Seed = packet.ReadInt();
        saveFile.PlayerLocation = packet.ReadVector3();
        saveFile.MapSize = packet.ReadInt();
        saveFile.VertsPerLine = packet.ReadInt();
        saveFile.Inventory = packet.ReadStringList();

        int vitalsLength = packet.ReadInt();
        for (int i = 0; i < vitalsLength; i++) { saveFile.Vitals.Add(packet.ReadFloat()); }
        int chunkLength = packet.ReadInt();
        for (int i = 0; i < chunkLength; i++)
        {
            SaveFileClass.ChunkClass chunk = new SaveFileClass.ChunkClass
            {
                Coords = packet.ReadVector2(),
                HeightMap = new float[saveFile.VertsPerLine, saveFile.VertsPerLine]
            };

            for (int x = 0; x < saveFile.VertsPerLine; x++)
            {
                for (int y = 0; y < saveFile.VertsPerLine; y++)
                {
                    chunk.HeightMap[x, y] = packet.ReadFloat();
                }
            }

            int propTypesLength = packet.ReadInt();
            print(propTypesLength);
            for (int b = 0; b < propTypesLength; b++)
            {
                SaveFileClass.PropTypeClass propType = new SaveFileClass.PropTypeClass();

                int propLength = packet.ReadInt();
                print(propLength);
                for (int j = 0; j < propLength; j++)
                {
                    SaveFileClass.PropClass prop = new SaveFileClass.PropClass
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

            int storageLength = packet.ReadInt();
            for (int j = 0; j < storageLength; j++)
            {
                SaveFileClass.StorageClass prop = new SaveFileClass.StorageClass
                {
                    ID = packet.ReadString(),
                    Pos = packet.ReadVector3(),
                    Rot = packet.ReadVector3(),
                    Items = packet.ReadStringList()
                };
                chunk.Storage.Add(prop);
            }

            saveFile.Chunks.Add(chunk);

            DataManager.SaveFile = saveFile;
            DataManager.ToLobby();
            //DataManager.SaveFileReceiveCallback.Invoke();
        }
    }
    /*
    public static void PlayerRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        MultiplayerGameManager.players[_id].transform.rotation = _rotation;
    }

    public static void ChunkData(Packet _packet)
    {
        ChunkDataStruct chunkData = new ChunkDataStruct();

        chunkData.coord = _packet.ReadVector2();
        chunkData.heightMap = _packet.ReadHeightMap();

        int propsCount = _packet.ReadInt();

        for (int i = 0; i < propsCount; i++)
        {
            PropDataStruct prop = new PropDataStruct
            {
                group = _packet.ReadInt(),
                prop = _packet.ReadInt(),
                rot = _packet.ReadVector3()
            };
            chunkData.props.Add(_packet.ReadVector3(), prop);
        }

        int structuresCount = _packet.ReadInt();

        for (int i = 0; i < structuresCount; i++)
        {
            StructureDataStruct structure = new StructureDataStruct
            {
                structure = _packet.ReadString(),
                rot = _packet.ReadVector3()
            };
            chunkData.structures.Add(_packet.ReadVector3(), structure);
        }

        MultiplayerTerrainGenerator.instance.SpawnChunk(chunkData);
    }

    public static void PropChunkUpdate(Packet _packet)
    {
        Dictionary<Vector3, PropDataStruct> props = new Dictionary<Vector3, PropDataStruct>();

        Vector2 coord = _packet.ReadVector2();
        int length = _packet.ReadInt();

        MultiplayerTerrainGenerator.instance.terrainChunkDictionary[coord].chunkData.props = props;
        GameObject propHolder = MultiplayerTerrainGenerator.instance.terrainChunkDictionary[coord].props;

        Transform[] children = propHolder.GetComponentsInChildren<Transform>();
        for (int j = 0; j < children.Length; j++) { PropsGeneration.instance.RemoveProp(children[j]); }

        Dictionary<Vector3, GameObject> propsDict = new Dictionary<Vector3, GameObject>();

        for (int i = 0; i < length; i++)
        {
            PropDataStruct prop = new PropDataStruct
            {
                group = _packet.ReadInt(),
                prop = _packet.ReadInt(),
                rot = _packet.ReadVector3()
            };
            Vector3 pos = _packet.ReadVector3();
            props.Add(pos, prop);

            GameObject gi = PropsGeneration.instance.PropSettings.PropGroups[prop.group].Props[prop.prop].PrefabVariants[0];
            GameObject g = PropsGeneration.instance.Pools[gi.transform.name + " Pool"].PropVariants[0].Props[0];
            PropsGeneration.instance.Pools[gi.transform.name + " Pool"].PropVariants[0].Props.RemoveAt(0);

            g.transform.parent = MultiplayerTerrainGenerator.instance.terrainChunkDictionary[coord].props.transform;
            g.transform.localPosition = pos;
            g.transform.eulerAngles = prop.rot;
            g.SetActive(true);

            propsDict.Add(g.transform.position, g);
        }

        MultiplayerTerrainGenerator.instance.terrainChunkDictionary[coord].propDict = propsDict;
    }

    public static void StructureChunkUpdate(Packet _packet)
    {
        Dictionary<Vector3, StructureDataStruct> structures = new Dictionary<Vector3, StructureDataStruct>();

        Vector2 coord = _packet.ReadVector2();
        int length = _packet.ReadInt();

        MultiplayerTerrainGenerator.instance.terrainChunkDictionary[coord].chunkData.structures = structures;
        GameObject structuresHolder = MultiplayerTerrainGenerator.instance.terrainChunkDictionary[coord].structures;

        Transform[] children = structuresHolder.GetComponentsInChildren<Transform>();
        for (int j = 0; j < children.Length; j++) { PropsGeneration.instance.RemoveProp(children[j]); }

        Dictionary<Vector3, GameObject> structuresDict = new Dictionary<Vector3, GameObject>();

        for (int i = 0; i < length; i++)
        {
            StructureDataStruct structure = new StructureDataStruct()
            {
                structure = _packet.ReadString(),
                rot = _packet.ReadVector3()
            };
            Vector3 pos = _packet.ReadVector3();
            structures.Add(pos, structure);

            ItemSettings item = Resources.Load<ItemSettings>("Prefabs/Interactable Items/" + structure.structure);
            GameObject g = Instantiate(item.gameObject);

            g.transform.name = item.name;
            g.transform.parent = MultiplayerTerrainGenerator.instance.terrainChunkDictionary[coord].props.transform;
            g.transform.position = pos;
            g.transform.eulerAngles = structure.rot;
            g.SetActive(true);

            structuresDict.Add(g.transform.position, g);
        }

        MultiplayerTerrainGenerator.instance.terrainChunkDictionary[coord].structureDict = structuresDict;
    }

    public static void UpdateVitals(Packet _packet)
    {
        VitalsManager.instance.MultiplayerModifyVitalAmount(0, _packet.ReadFloat());
        VitalsManager.instance.MultiplayerModifyVitalAmount(1, _packet.ReadFloat());
    }
    */

    #endregion
}
