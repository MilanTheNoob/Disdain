using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

static class Server
{
    public const int MaxPlayers = 50;
    public const int Port = 26951;
    public const string SavePath = "C:/Disdain Server";

    public static Client[] Clients = new Client[MaxPlayers];
    public static int[] LobbyClients = new int[MaxPlayers];
    public static int[] SaveClients = new int[MaxPlayers];
    public static int[] RoomClients = new int[MaxPlayers];

    public delegate void PacketHandler(int fromClient, Packet packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    public static Dictionary<string, string> Accounts = new Dictionary<string, string>();

    static TcpListener TCPListener;
    static UdpClient UDPListener;

    static void Main(string[] args)
    {
        #region Data Loading

        #endregion

        #region Basic Init

        for (int i = 0; i < MaxPlayers; i++) { Clients[i] = new Client(i); }

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

        #endregion
        #region TCP & UDP Init

        #endregion
    }
}