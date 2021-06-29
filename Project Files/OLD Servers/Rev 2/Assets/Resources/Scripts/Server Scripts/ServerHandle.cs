using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServerHandle
{
    #region Crappy Passwords

    public static string[] CrappyPasswords = {"123456", "123456789", "qwerty", "qwertyuiop", "password", "1234567", "12345678",
    "12345", "iloveyou", "11111", "22222", "123123", "abc123", "qwerty123", "admin", "lovely", "77777", "88888", "welcome",
    "princess", "dragon", "password1", "123qwe", "letmein", "football", "sunshine", "monkey", "baseball", "qazwsx", "starwars",
    "passw0rd", "ashley", "bailey", "hottie", "loveme", "trustno1"};

    #endregion

    public static Dictionary<int, SavePacketsClass> PlayerPackets = new Dictionary<int, SavePacketsClass>();

    public static void Login(int _fromClient, Packet _packet)
    {
        string username = _packet.ReadString();
        string password = _packet.ReadString();

        if (NetworkManager.ServerData.Accounts.ContainsKey(username))
        {
            if (NetworkManager.ServerData.Accounts[username].Password == password)
            {
                Server.Clients[_fromClient].LoggedIn = true;
                Server.Clients[_fromClient].Username = username;
                Server.Clients[_fromClient].Skin = NetworkManager.ServerData.Accounts[username].Skin;

                NetworkManager.instance.GetPlayerData(_fromClient);

                ServerSend.LoginResponse(_fromClient, true);
                ServerSend.SaveFileTransfer(_fromClient, Server.Clients[_fromClient].SaveFile);
            }
            else
            {
                ServerSend.LoginResponse(_fromClient, false, "Incorrect password man :/");
            }
        }
        else
        {
            ServerSend.LoginResponse(_fromClient, false, "No account found with the name '" + username + "'");
        }
    }

    public static void Signup(int _fromClient, Packet _packet)
    {
        string username = _packet.ReadString();
        string password = _packet.ReadString();
        int skin = _packet.ReadInt();

        if (!NetworkManager.ServerData.Accounts.ContainsKey(username))
        {
            if (password.Length > 4)
            {
                if (CrappyPasswords.Contains(password.ToLower()))
                {
                    ServerSend.SignupResponse(_fromClient, false, "Cmon dude, its 2021. Get a better password -_-");
                }
                else
                {
                    ServerDataClass.Account account = new ServerDataClass.Account
                    {
                        Password = password,
                        Skin = skin
                    };

                    Server.Clients[_fromClient].LoggedIn = true;
                    Server.Clients[_fromClient].Username = username;
                    Server.Clients[_fromClient].Skin = skin;

                    NetworkManager.ServerData.Accounts.Add(username, account);
                    ServerSend.SignupResponse(_fromClient, true, "");

                    Server.Clients[_fromClient].SaveFile = new SaveFileClass(_fromClient, true);
                }
            }
            else
            {
                ServerSend.SignupResponse(_fromClient, false, "Password too short man! Min of 5 characters");
            }
        }
        else
        {
            ServerSend.SignupResponse(_fromClient, false, "Username already exists :/");
        }
    }

    public static void JoinLobby(int _fromClient, Packet _packet)
    {
        if (!Server.Clients[_fromClient].LoggedIn) { Server.Ban(_fromClient); return; }
        Server.Clients[_fromClient].SendIntoGame(Server.Clients[_fromClient].Username, 
            Server.Clients[_fromClient].ID);
    }

    public static void MovementL(int _fromClient, Packet packet)
    {
        if (Server.Clients[_fromClient].player == null) { return; }
        Server.Clients[_fromClient].player.SetInput(packet.ReadFloat(), packet.ReadFloat(), packet.ReadVector3(), packet.ReadBool());
    }

    public static void SaveFileTransfer(int _fromClient, Packet packet)
    {
        if (PlayerPackets.ContainsKey(_fromClient))
        {
            List<byte> chunk = packet.readableBuffer.ToList();
            int count = packet.ReadInt();

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

            PlayerPackets[_fromClient].chunks.Add(count, chunk);
        }
        else
        {
            PlayerPackets.Add(_fromClient, new SavePacketsClass());
            List<byte> chunk = packet.readableBuffer.ToList();
            int count = packet.ReadInt();

            // DO NOT UNCOMMENT CODE! WILL BRICK GAME
            //for (int i = 0; i < chunk.Count; i++) { print(chunk[i]); }
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

            PlayerPackets[_fromClient].chunks.Add(count, chunk);
        }
    }
    public static void SaveFileTransferEnd(int _fromClient, Packet _packet)
    {
        NetworkManager.instance.StartCoroutine(ISaveFileTransferEnd(_fromClient, _packet));
    }
    static IEnumerator ISaveFileTransferEnd(int _fromClient, Packet _packet)
    {
        int count = _packet.ReadInt();

        while (!PlayerPackets.ContainsKey(_fromClient)) { yield return new WaitForSeconds(0.1f); }
        while (PlayerPackets[_fromClient].chunks.Count < count) { yield return new WaitForSeconds(0.1f); }

        Packet packet = new Packet();
        for (int i = 0; i < PlayerPackets[_fromClient].chunks.Count; i++) { packet.Write(PlayerPackets[_fromClient].chunks[i].ToArray()); }

        Server.Clients[_fromClient].SaveFile = new SaveFileClass(_fromClient, false, packet.buffer.ToArray(), 0);
    }

    public static void JoinSave(int clientId, Packet packet)
    {
        Client client = Server.Clients[clientId];

        Object.Destroy(client.player.gameObject);
        client.player = null;
        if (Server.LobbyClients.Contains(client)) { Server.LobbyClients.Remove(client); }

        NetworkManager.Players.Remove(clientId);
        ServerSend.PlayerLeaveL(clientId);
    }

    public static void RequestChunk(int client, Packet packet)
    {
        SaveFileClass.ChunkClass chunkData = Server.Clients[client].SaveFile.ReturnChunk(packet.ReadVector2(false));

        if (chunkData != null)
        {
            ServerSend.AddChunk(client, chunkData);
        }
        else
        {
            Server.Clients[client].SaveFile.GenerateChunk(client, packet.ReadVector2());
        }
    }

    public class SavePacketsClass
    {
        public Dictionary<int, List<byte>> chunks = new Dictionary<int, List<byte>>();
    }
}
