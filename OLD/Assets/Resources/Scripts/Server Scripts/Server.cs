using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server : MonoBehaviour
{
    public static int MaxPlayers = 50;
    public static int Port = 26951;

    public static Dictionary<int, Client> Clients = new Dictionary<int, Client>();
    public static List<Client> LobbyClients = new List<Client>();

    public delegate void PacketHandler(int _fromClient, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    private static TcpListener tcpListener;
    private static UdpClient udpListener;

    public static void Startup()
    {
        InitializeServerData();
        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPReceiveCallback, null);
    }

    private static void TCPConnectCallback(IAsyncResult _result)
    {
        TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
        Debug.Log($"Incoming connection from {_client.Client.RemoteEndPoint}");

        for (int i = 1; i <= MaxPlayers; i++)
        {
            if (Clients[i].tcp.socket == null)
            {
                Clients[i] = new Client(i);
                Clients[i].tcp.Connect(_client);
                return;
            }
        }

        Debug.Log($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
    }

    /// <summary>Receives incoming UDP data.</summary>
    private static void UDPReceiveCallback(IAsyncResult _result)
    {
        try
        {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (_data.Length < 4)
            {
                return;
            }

            using (Packet _packet = new Packet(_data))
            {
                int _clientId = _packet.ReadInt();

                if (_clientId == 0)
                {
                    return;
                }

                if (Clients[_clientId].udp.endPoint == null)
                {
                    Clients[_clientId].udp.Connect(_clientEndPoint);
                    return;
                }

                if (Clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                {
                    Clients[_clientId].udp.HandleData(_packet);
                }
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error receiving UDP data: {_ex}");
        }
    }

    public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
    {
        try
        {
            if (_clientEndPoint != null)
            {
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
        }
    }

    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
    }

    public static void Ban(int client)
    {
        // To be implemented
    }

    static void InitializeServerData()
    {
        for (int i = 1; i <= MaxPlayers; i++)
        {
            Clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            // Basic Communication
            { (int)ClientPackets.Login, ServerHandle.Login },
            { (int)ClientPackets.Signup, ServerHandle.Signup },
            { (int)ClientPackets.JoinLobby, ServerHandle.JoinLobby },
            { (int)ClientPackets.JoinSave, ServerHandle.JoinSave },
            // Lobby Packets
            { (int)ClientPackets.MovementL, ServerHandle.MovementL },
            // Singleplayer Packets
            { (int)ClientPackets.SaveFileTransfer, ServerHandle.SaveFileTransfer },
            { (int)ClientPackets.SaveFileTransferEnd, ServerHandle.SaveFileTransferEnd },
            { (int)ClientPackets.RequestChunk, ServerHandle.RequestChunk }
        };
    }
}
