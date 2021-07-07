using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System;

public class ServerSend : MonoBehaviour
{
    #region Send Funcs

    public static void SendTCPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.Clients[_toClient].tcp.SendData(_packet);
    }

    public static void SendUDPData(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.Clients[_toClient].udp.SendData(_packet);
    }

    public static void SendTCPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.Clients[i].tcp.SendData(_packet);
        }
    }
    public static void SendTCPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.Clients[i].tcp.SendData(_packet);
            }
        }
    }

    public static void SendUDPDataToAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            Server.Clients[i].udp.SendData(_packet);
        }
    }
    public static void SendUDPDataToAll(int _exceptClient, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 1; i <= Server.MaxPlayers; i++)
        {
            if (i != _exceptClient)
            {
                Server.Clients[i].udp.SendData(_packet);
            }
        }
    }

    #endregion

    public static void OnConnect(int _toClient)
    {
        using (Packet _packet = new Packet((int)ServerPackets.OnConnect))
        {
            _packet.Write(_toClient);
            SendTCPData(_toClient, _packet);
        }
    }

    public static void LoginResponse(int _toClient, bool _success, string _response = "")
    {
        using (Packet _packet = new Packet((int)ServerPackets.LoginResponse))
        {
            _packet.Write(_success);
            if (_success)
            {
                _packet.Write(Server.Clients[_toClient].Username);
                _packet.Write(Server.Clients[_toClient].Skin);
            }
            else
            {
                _packet.Write(_response);
            }

            SendTCPData(_toClient, _packet);
        }
    }

    public static void SignupResponse(int _toClient, bool _success, string _response = "")
    {
        using (Packet _packet = new Packet((int)ServerPackets.SignupRespnse))
        {
            _packet.Write(_success);
            if (_success)
            {
                _packet.Write(Server.Clients[_toClient].Username);
                _packet.Write(Server.Clients[_toClient].Skin);
            }
            else
            {
                _packet.Write(_response);
            }

            SendTCPData(_toClient, _packet);
        }
    }

    public static void SaveFileTransfer(int _toClient, SaveFileClass saveFile)
    {
        if (saveFile == null || !saveFile.Generated) return;

        var saveChunks = BufferSplit(saveFile.Serialize(), 4000);
        int count = 0;

        foreach (byte[] chunk in saveChunks)
        {
            using (Packet _packet = new Packet((int)ServerPackets.SaveFileTransfer))
            {
                _packet.Write(count);
                _packet.Write(chunk);

                SendTCPData(_toClient, _packet);
            }

            count++;
        }

        using (Packet _packet = new Packet((int)ServerPackets.SaveFileTransferEnd))
        {
            _packet.Write(count);
            SendTCPData(_toClient, _packet);
        }
    }

    public static void AddChunk(int toClient, SaveFileClass.ChunkClass chunkData)
    {
        using (Packet sendPacket = new Packet((int)ServerPackets.AddChunk))
        {
            sendPacket.Write(chunkData.Coords);
            int VertsPerLine = NetworkManager.instance.GS.MeshSetting.numVertsPerLine;

            int c = 0;
            for (int x = 0; x < VertsPerLine; x++)
            {
                for (int y = 0; y < VertsPerLine; y++)
                {
                    sendPacket.Write(chunkData.HeightMap[x, y]);
                    c++;
                }
            }

            sendPacket.Write(chunkData.PropTypes.Count);
            for (int b = 0; b < chunkData.PropTypes.Count; b++)
            {
                sendPacket.Write(chunkData.PropTypes[b].Props.Count);
                for (int j = 0; j < chunkData.PropTypes[b].Props.Count; j++)
                {
                    sendPacket.Write(chunkData.PropTypes[b].Props[j].ID);
                    sendPacket.Write(chunkData.PropTypes[b].Props[j].Pos);
                    sendPacket.Write(chunkData.PropTypes[b].Props[j].Scale);
                    sendPacket.Write(chunkData.PropTypes[b].Props[j].Euler);
                }
            }

            sendPacket.Write(chunkData.Storage.Count);
            for (int j = 0; j < chunkData.Storage.Count; j++)
            {
                sendPacket.Write(chunkData.Storage[j].ID);
                sendPacket.Write(chunkData.Storage[j].Pos);
                sendPacket.Write(chunkData.Storage[j].Rot);

                sendPacket.Write(chunkData.Storage[j].Items);
            }

            SendTCPData(toClient, sendPacket);
        }
    }

    public static void PlayerSpawnL(int _toClient, Player _player, int _skin)
    {
        using (Packet _packet = new Packet((int)ServerPackets.PlayerSpawnL))
        {
            _packet.Write(Server.Clients[_toClient].ID);
            _packet.Write(Server.Clients[_toClient].Username);
            _packet.Write(_player.transform.position);
            _packet.Write(_player.transform.rotation);
            _packet.Write(_skin);

            SendTCPData(_toClient, _packet);
        }
    }
    public static void PlayerDataL(List<Client> clients)
    {
        using (Packet _packet = new Packet((int)ServerPackets.PlayerDataL))
        {
            for (int i = 0; i < clients.Count; i++)
            {
                _packet.Write(clients[i].player.transform.position);
                _packet.Write(clients[i].player.transform.eulerAngles);
            }
            SendUDPDataToAll(_packet);
        }
    }

    public static void PlayerLeaveL(int _playerId)
    {
        using (Packet _packet = new Packet((int)ServerPackets.PlayerLeaveL))
        {
            _packet.Write(_playerId);
            SendTCPDataToAll(_packet);
        }
    }

    public static byte[][] BufferSplit(byte[] buffer, int blockSize)
    {
        byte[][] blocks = new byte[(buffer.Length + blockSize - 1) / blockSize][];

        for (int i = 0, j = 0; i < blocks.Length; i++, j += blockSize)
        {
            blocks[i] = new byte[Math.Min(blockSize, buffer.Length - j)];
            Array.Copy(buffer, j, blocks[i], 0, blocks[i].Length);
        }

        return blocks;
    }
}

public static class EnumerableEx
{
    public static IEnumerable<string> SplitBy(this string str, int chunkLength)
    {
        if (String.IsNullOrEmpty(str)) throw new ArgumentException();
        if (chunkLength < 1) throw new ArgumentException();

        for (int i = 0; i < str.Length; i += chunkLength)
        {
            if (chunkLength + i > str.Length)
                chunkLength = str.Length - i;

            yield return str.Substring(i, chunkLength);
        }
    }
}