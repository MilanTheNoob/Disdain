using System;
using System.Net;
using System.Net.Sockets;

public class Client
{
    public SaveFile SaveFile;

    public TCP tcp;
    public UDP udp;

    public int ID;
    public string Username;
    public int Skin;
    public bool LoggedIn;

    public Client(int clientId)
    {
        ID = clientId;
        tcp = new TCP(ID);
        udp = new UDP(ID);
    }

    public void SendIntoGame(string playerName, int skin)
    {
        /*
        Skin = skin;
        Server.LobbyClients.Add(this);

        foreach (Client client in Server.Clients.Values)
        {
            if (client.player != null)
            {
                if (client.ID != ID)
                {
                    ServerSend.PlayerSpawnL(ID, client.player, Skin);
                }
            }
        }

        foreach (Client client in Server.Clients.Values)
        {
            if (client.player != null)
            {
                ServerSend.PlayerSpawnL(client.ID, player, Skin);
            }
        }
        */
    }

    public void Disconnect()
    {
        ThreadManager.ExecuteOnMainThread(() =>
        {
            NetworkManager.instance.SavePlayerData(ID);

            if (Server.LobbyClients.Contains(this)) Server.LobbyClients.Remove(this);
            if (player != null) UnityEngine.Object.Destroy(player.gameObject);

            NetworkManager.Players.Remove(ID);
            ServerSend.PlayerLeaveL(ID);

            tcp = new TCP(ID);
            udp = new UDP(ID);
        });
    }
}
