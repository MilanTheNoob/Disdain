using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;

public class DataManager : MonoBehaviour
{
    #region Singleton

    public static DataManager instance;
    void Awake() { instance = this; }

    #endregion

    public static TCP tcp;
    public static Dictionary<int, Packet> RecvingPackets = new Dictionary<int, Packet>();

    public static void PacketHandler(Packet packet, int id)
    {
        switch (id)
        {
            case 1: Handle.OnConnect(packet); break; // OnConnect

            case 100: Handle.BackendLogin(packet); break; // Backend Login
            case 101: Handle.BackendPreview(packet); break; // Backend Preview
        }
    }

    #region Connecting

    public static void Connect(string ip, int port)
    {
        tcp = new TCP();
        tcp.Connect("127.0.0.1", 26951);

        instance.StartCoroutine(IConnect());
    }

    static IEnumerator IConnect()
    {
        yield return new WaitForSeconds(0.5f);
        Send.BackendLogin();
    }

    #endregion

    #region Communication Classes (TCP & UDP)

    public class TCP
    {
        public TcpClient socket;

        NetworkStream stream;
        Packet receivedData;
        byte[] receiveBuffer;

        public void Connect(string ip, int port)
        {
            socket = new TcpClient
            {
                ReceiveBufferSize = 16384,
                SendBufferSize = 4096
            };

            receiveBuffer = new byte[16384];
            socket.BeginConnect(ip, port, ConnectCallback, socket);
        }

        void ConnectCallback(IAsyncResult result)
        {
            socket.EndConnect(result);
            if (!socket.Connected) { return; }

            stream = socket.GetStream();
            stream.BeginRead(receiveBuffer, 0, 16384, ReceiveCallback, null);

            receivedData = new Packet();
        }

        public void SendData(byte[] data)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(data, 0, data.Length, null, null);
                }
            }
            catch { }
        }

        void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                int byteLength = stream.EndRead(result);
                if (byteLength <= 0) { Disconnect(); return; }

                byte[] data = new byte[byteLength];
                Array.Copy(receiveBuffer, data, byteLength);

                receivedData.Reset(HandleData(data));
                stream.BeginRead(receiveBuffer, 0, 16384, ReceiveCallback, null);
            }
            catch
            {
                Disconnect();
            }
        }

        private bool HandleData(byte[] data)
        {
            ThreadedDataRequester.ExecuteOnMainThread(() =>
            {
                int id = BitConverter.ToInt32(data, 0);
                bool singular = BitConverter.ToBoolean(data, 4);

                if (RecvingPackets.ContainsKey(id))
                {
                    RecvingPackets[id].AddChunk(data, PacketHandler);
                }
                else if (singular)
                {
                    Packet packet = new Packet(data, PacketHandler);
                }
                else
                {
                    RecvingPackets.Add(id, new Packet(data, PacketHandler));
                }
            });

            return true;
        }

        void Disconnect()
        {
            Disconnect();

            stream = null;
            receivedData = null;
            receiveBuffer = null;
            socket = null;
        }
    }

    #endregion
}
