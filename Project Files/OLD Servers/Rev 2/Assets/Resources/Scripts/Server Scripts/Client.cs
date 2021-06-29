using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client
{
    public Player player;
    public SaveFileClass SaveFile;

    public TCP tcp;
    public UDP udp;

    public int ID;
    public string Username;
    public int Skin;
    public bool LoggedIn;

    public Client(int _clientId)
    {
        ID = _clientId;
        tcp = new TCP(ID);
        udp = new UDP(ID);
    }

    public class TCP
    {
        public TcpClient socket;

        private readonly int id;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public TCP(int _id)
        {
            id = _id;
        }

        public void Connect(TcpClient _socket)
        {
            socket = _socket;
            socket.ReceiveBufferSize = 4096;
            socket.SendBufferSize = 4096;

            stream = socket.GetStream();

            receivedData = new Packet();
            receiveBuffer = new byte[4096];

            stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);

            ServerSend.OnConnect(id);
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
                Debug.Log($"Error sending data to player {id} via TCP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    Server.Clients[id].Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error receiving TCP data: {_ex}");
                Server.Clients[id].Disconnect();
            }
        }

        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4)
            {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    return true;
                }
            }

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        Server.packetHandlers[_packetId](id, _packet);
                    }
                });

                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (_packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        public void Disconnect()
        {
            socket.Close();
            socket = null;

            stream = null;
            receivedData = null;
            receiveBuffer = null;
        }
    }
    public class UDP
    {
        public IPEndPoint endPoint;

        private int id;

        public UDP(int _id)
        {
            id = _id;
        }

        /// <summary>Initializes the newly connected client's UDP-related info.</summary>
        /// <param name="_endPoint">The IPEndPoint instance of the newly connected client.</param>
        public void Connect(IPEndPoint _endPoint)
        {
            endPoint = _endPoint;
        }

        /// <summary>Sends data to the client via UDP.</summary>
        /// <param name="_packet">The packet to send.</param>
        public void SendData(Packet _packet)
        {
            Server.SendUDPData(endPoint, _packet);
        }

        /// <summary>Prepares received data to be used by the appropriate packet handler methods.</summary>
        /// <param name="_packetData">The packet containing the recieved data.</param>
        public void HandleData(Packet _packetData)
        {
            int _packetLength = _packetData.ReadInt();
            byte[] _packetBytes = _packetData.ReadBytes(_packetLength);

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();
                    Server.packetHandlers[_packetId](id, _packet); // Call appropriate method to handle the packet
                }
            });
        }

        /// <summary>Cleans up the UDP connection.</summary>
        public void Disconnect()
        {
            endPoint = null;
        }
    }

    public void SendIntoGame(string _playerName, int _skin)
    {
        player = NetworkManager.instance.InstantiatePlayer(ID);
        Skin = _skin;
        Server.LobbyClients.Add(this);

        foreach (Client _client in Server.Clients.Values)
        {
            if (_client.player != null)
            {
                if (_client.ID != ID)
                {
                    ServerSend.PlayerSpawnL(ID, _client.player, Skin);
                }
            }
        }

        foreach (Client _client in Server.Clients.Values)
        {
            if (_client.player != null)
            {
                ServerSend.PlayerSpawnL(_client.ID, player, Skin);
            }
        }
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
