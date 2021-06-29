using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class TCP
{
    public TcpClient Socket;
    public readonly int ID;

    NetworkStream stream;
    Packet receivedData;
    byte[] receiveBuffer;

    public TCP(int id) { ID = id; }

    public void Connect(TcpClient socket)
    {
        Socket = socket;
        stream = socket.GetStream();

        receivedData = new Packet();
        receiveBuffer = new byte[4096];

        stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);

        ServerSend.OnConnect(ID);
    }

    public void SendData(Packet packet)
    {
        try
        {
            if (Socket != null)
            {
                stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending data to player {ID} via TCP: {ex}");
        }
    }

    private void ReceiveCallback(IAsyncResult result)
    {
        try
        {
            int byteLength = stream.EndRead(result);
            if (byteLength <= 0)
            {
                Server.Clients[ID].Disconnect();
                return;
            }

            byte[] data = new byte[byteLength];
            Array.Copy(receiveBuffer, data, byteLength);

            receivedData.Reset(HandleData(data));
            stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error receiving TCP data: {ex}");
            Server.Clients[ID].Disconnect();
        }
    }

    private bool HandleData(byte[] data)
    {
        int packetLength = 0;

        receivedData.SetBytes(data);

        if (receivedData.UnreadLength() >= 4)
        {
            packetLength = receivedData.ReadInt();
            if (packetLength <= 0)
            {
                return true;
            }
        }

        while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
        {
            byte[] packetBytes = receivedData.ReadBytes(packetLength);
            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet packet = new Packet(packetBytes))
                {
                    int packetId = packet.ReadInt();
                    Server.packetHandlers[packetId](ID, packet);
                }
            });

            packetLength = 0;
            if (receivedData.UnreadLength() >= 4)
            {
                packetLength = receivedData.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }
        }

        if (packetLength <= 1)
        {
            return true;
        }

        return false;
    }

    public void Disconnect()
    {
        Socket.Close();
        Socket = null;

        stream = null;
        receivedData = null;
        receiveBuffer = null;
    }
}
