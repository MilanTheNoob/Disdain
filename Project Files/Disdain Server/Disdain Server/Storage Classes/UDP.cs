using System.Net;

public class UDP
{
    public IPEndPoint EndPoint;

    private int ID;

    public UDP(int id)
    {
        ID = id;
    }

    /// <summary>Initializes the newly connected client's UDP-related info.</summary>
    /// <param name="endPoint">The IPEndPoint instance of the newly connected client.</param>
    public void Connect(IPEndPoint endPoint)
    {
        EndPoint = endPoint;
    }

    /// <summary>Sends data to the client via UDP.</summary>
    /// <param name="packet">The packet to send.</param>
    public void SendData(Packet packet)
    {
        Server.SendUDPData(EndPoint, packet);
    }

    /// <summary>Prepares received data to be used by the appropriate packet handler methods.</summary>
    /// <param name="packetData">The packet containing the recieved data.</param>
    public void HandleData(Packet packetData)
    {
        int packetLength = packetData.ReadInt();
        byte[] packetBytes = packetData.ReadBytes(packetLength);

        ThreadManager.ExecuteOnMainThread(() =>
        {
            using (Packet packet = new Packet(packetBytes))
            {
                int packetId = packet.ReadInt();
                Server.packetHandlers[packetId](ID, packet); // Call appropriate method to handle the packet
            }
        });
    }

    /// <summary>Cleans up the UDP connection.</summary>
    public void Disconnect()
    {
        EndPoint = null;
    }
}