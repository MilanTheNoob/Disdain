using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace GameServer
{
    class Client
    {
        public TcpClient socket;

        private readonly int id;
        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public void Connect(TcpClient _socket)
        {
            socket = _socket;
            socket.ReceiveBufferSize = 4096;
            socket.SendBufferSize = 4096;

            stream = socket.GetStream();

            receivedData = new Packet();
            receiveBuffer = new byte[4096];

            stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);
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
                Console.WriteLine($"Error sending data to player {id} via TCP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data));
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error receiving TCP data: {_ex}");
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

                using (Packet _packet = new Packet(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();
                    string name = _packet.ReadString();
                    string password = _packet.ReadString();

                    if (_packetId == (int)ClientPackets.login)
                    {
                        if (Program.AccountData.Accounts.ContainsKey(name))
                        {
                            if (Program.AccountData.Accounts[name].password == password)
                            {
                                using (Packet packet = new Packet((int)ServerPackets.loginResponse))
                                {
                                    packet.Write(true);
                                    packet.Write(Program.LobbyIP);
                                    packet.Write(Program.LobbyPort);
                                    packet.Write(Program.AccountData.Accounts[name].skin);
                                    packet.Write(Program.AccountData.Accounts[name].HasGenerated);
                                    if (Program.AccountData.Accounts[name].HasGenerated)
                                    {
                                        packet.Write(Program.AccountData.Accounts[name].Save);
                                    }
                                    packet.WriteLength();

                                    try
                                    {
                                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                                    }
                                    catch { }
                                }
                            }
                            else
                            {
                                using (Packet packet = new Packet((int)ServerPackets.loginResponse))
                                {
                                    packet.Write(false);
                                    packet.Write(0);
                                    packet.WriteLength();

                                    try
                                    {
                                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                                    }
                                    catch { }
                                }
                            }
                        }
                        else
                        {
                            using (Packet packet = new Packet((int)ServerPackets.loginResponse))
                            {
                                packet.Write(false);
                                packet.Write(0);
                                packet.WriteLength();

                                try
                                {
                                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                                }
                                catch { }
                            }
                        }
                    }
                    else if (_packetId == (int)ClientPackets.signup)
                    {
                        Console.WriteLine(name);
                        if (!Program.AccountData.Accounts.ContainsKey(name))
                        {
                            Account account = new Account
                            {
                                password = password,
                                skin = _packet.ReadInt()
                            };
                            Program.AccountData.Accounts.Add(name, account);

                            using (Packet packet = new Packet((int)ServerPackets.signupResponse))
                            {
                                packet.Write(true);
                                packet.Write(Program.LobbyIP);
                                packet.Write(Program.LobbyPort);
                                packet.Write(account.skin);
                                packet.Write(false);
                                packet.WriteLength();

                                Console.WriteLine("The skin is " + account.skin);

                                try
                                {
                                    Console.WriteLine(packet.UnreadLength());
                                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                                }
                                catch { }
                            }
                        }
                        else
                        {
                            using (Packet packet = new Packet((int)ServerPackets.signupResponse))
                            {
                                packet.Write(false);
                                packet.Write(1);
                                packet.WriteLength();

                                try
                                {
                                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                                }
                                catch { }
                            }
                        }
                    }
                    else if (_packetId == (int)ClientPackets.save)
                    {
                        string username = _packet.ReadString();
                        if (Program.AccountData.Accounts.ContainsKey(username))
                        {
                            try
                            {
                                Program.AccountData.Accounts[username].Save = _packet.ReadString();
                                Program.AccountData.Accounts[username].HasGenerated = true;
                            }
                            catch
                            {
                                Program.AccountData.Accounts[username].HasGenerated = false;
                                Program.AccountData.Accounts[username].Save = "";
                            }
                        }
                    }
                }

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

            socket.Close();

            if (_packetLength <= 1)
            {
                return true;
            }

            return false;
        }
    }
}
