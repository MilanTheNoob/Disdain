using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Handle
{
    public static Dictionary<int, SavePacketsClass> PlayerPackets = new Dictionary<int, SavePacketsClass>();

    public static void Login(int client, Packet packet)
    {
        string username = packet.ReadString();
        string password = packet.ReadString();

        if (!Server.Accounts.ContainsKey(username))
        {
            ServerSend.LoginResponse(client, false, "Username doesn't exist");
            return;
        }
        if (Server.Accounts[username] != password)
        {
            ServerSend.LoginResponse(client, false, "Incorrect password");
            return;
        }

        Client c = Server.Clients[client];
        c.LoggedIn = true;

        if (NetworkManager.ServerData.Accounts.ContainsKey(username))
        {
            if (NetworkManager.ServerData.Accounts[username].Password == password)
            {
                Server.Clients[client].LoggedIn = true;
                Server.Clients[client].Username = username;
                Server.Clients[client].Skin = NetworkManager.ServerData.Accounts[username].Skin;

                NetworkManager.instance.GetPlayerData(client);

                ServerSend.LoginResponse(client, true);
                ServerSend.SaveFileTransfer(client, Server.Clients[client].SaveFile);
            }
            else
            {
                ServerSend.LoginResponse(client, false, "Incorrect password man :/");
            }
        }
        else
        {
            ServerSend.LoginResponse(client, false, "No account found with the name '" + username + "'");
        }
    }

    public static void Signup(int client, Packet packet)
    {
        string username = packet.ReadString();
        string password = packet.ReadString();
        int skin = packet.ReadInt();

        if (!NetworkManager.ServerData.Accounts.ContainsKey(username))
        {
            if (password.Length > 4)
            {
                if (CrappyPasswords.Contains(password.ToLower()))
                {
                    ServerSend.SignupResponse(client, false, "Cmon dude, its 2021. Get a better password --");
                }
                else
                {
                    ServerDataClass.Account account = new ServerDataClass.Account
                    {
                        Password = password,
                        Skin = skin
                    };

                    Server.Clients[client].LoggedIn = true;
                    Server.Clients[client].Username = username;
                    Server.Clients[client].Skin = skin;

                    NetworkManager.ServerData.Accounts.Add(username, account);
                    ServerSend.SignupResponse(client, true, "");

                    Server.Clients[client].SaveFile = new SaveFileClass(client, true);
                }
            }
            else
            {
                ServerSend.SignupResponse(client, false, "Password too short man! Min of 5 characters");
            }
        }
        else
        {
            ServerSend.SignupResponse(client, false, "Username already exists :/");
        }
    }

    public static void JoinLobby(int client, Packet packet)
    {
        if (!Server.Clients[client].LoggedIn) { Server.Ban(client); return; }
        Server.Clients[client].SendIntoGame(Server.Clients[client].Username,
            Server.Clients[client].ID);
    }

    public static void MovementL(int client, Packet packet)
    {
        if (Server.Clients[client].player == null) { return; }
        Server.Clients[client].player.SetInput(packet.ReadFloat(), packet.ReadFloat(), packet.ReadVector3(), packet.ReadBool());
    }

    public static void SaveFileTransfer(int client, Packet packet)
    {
        if (PlayerPackets.ContainsKey(client))
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

            PlayerPackets[client].chunks.Add(count, chunk);
        }
        else
        {
            PlayerPackets.Add(client, new SavePacketsClass());
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

            PlayerPackets[client].chunks.Add(count, chunk);
        }
    }
    public static void SaveFileTransferEnd(int client, Packet packet)
    {
        NetworkManager.instance.StartCoroutine(ISaveFileTransferEnd(client, packet));
    }
    static IEnumerator ISaveFileTransferEnd(int client, Packet packet)
    {
        int count = packet.ReadInt();

        while (!PlayerPackets.ContainsKey(client)) { yield return new WaitForSeconds(0.1f); }
        while (PlayerPackets[client].chunks.Count < count) { yield return new WaitForSeconds(0.1f); }

        Packet packet = new Packet();
        for (int i = 0; i < PlayerPackets[client].chunks.Count; i++) { packet.Write(PlayerPackets[client].chunks[i].ToArray()); }

        Server.Clients[client].SaveFile = new SaveFileClass(client, false, packet.buffer.ToArray(), 0);
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

