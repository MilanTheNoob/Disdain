using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Net.Sockets;
using System;

public class DataManager : MonoBehaviour
{
    #region Singleton

    public static DataManager instance;
    void Awake() { instance = this; }

    #endregion

    delegate void PacketHandler(Packet packet);
    static Dictionary<byte, PacketHandler> packetHandlers;

    public static TCP tcp;

    void Start()
    {
        packetHandlers = new Dictionary<byte, PacketHandler>()
        {
            { 1, Handle.OnConnect },
            { 11, Handle.BackendLogin }
        };
    }

    #region Connecting

    public static void Connect(string ip, int port, string password)
    {
        tcp = new TCP();
        tcp.Connect(ip, port);

        instance.StartCoroutine(IConnect("PragmaticMilan85$"));
    }

    static IEnumerator IConnect(string password)
    {
        yield return new WaitForSeconds(0.5f);
        Send.BackendLogin(password);
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
                ReceiveBufferSize = 4096,
                SendBufferSize = 4096
            };

            receiveBuffer = new byte[4096];
            socket.BeginConnect(ip, port, ConnectCallback, socket);
        }

        void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result);
            if (!socket.Connected) { return; }

            stream = socket.GetStream();
            stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);

            receivedData = new Packet();
        }

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
            }
            catch { }
        }

        void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0) { Disconnect(); return; }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);
            }
            catch
            {
                Disconnect();
            }
        }

        private bool HandleData(byte[] _data)
        {
            ThreadedDataRequester.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet())
                {
                    _packet.SetBytes(_data);
                    byte _packetId = _packet.ReadByte();
                    if (_packetId != 0) { print(_packetId); packetHandlers[_packetId](_packet); }
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
