using System;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend
{
    /*
    public static void Login(string username, string password)
    {
        using (Packet packet = new Packet((int)ClientPackets.Login))
        {
            packet.Write(username);
            packet.Write(password);

            SendTCPData(packet);
        }
    }
    public static void Signup(string username, string password, int skin)
    {
        using (Packet packet = new Packet((int)ClientPackets.Signup))
        {
            packet.Write(username);
            packet.Write(password);
            packet.Write(skin);

            SendTCPData(packet);
        }
    }

    public static void JoinLobby()
    {
        using (Packet packet = new Packet((int)ClientPackets.JoinLobby)) { SendTCPData(packet); }
    }

    public static void MovementL(float horizontal, float vertical, bool jump)
    {
        if (GameManager.ActivePlayer == null) { return; }

        using (Packet packet = new Packet((int)ClientPackets.MovementL))
        {
            packet.Write(horizontal);
            packet.Write(vertical);
            packet.Write(GameManager.ActivePlayer.transform.eulerAngles);
            packet.Write(jump);

            SendUDPData(packet);
        }
    }

    public static void SaveFileTransfer()
    {
        SaveFileClass saveFile = DataManager.SaveFile;

        using (Packet tempPacket = new Packet())
        {
            tempPacket.Write(saveFile.Seed);
            tempPacket.Write(saveFile.PlayerLocation);

            tempPacket.Write(saveFile.MapSize);
            tempPacket.Write(saveFile.VertsPerLine);

            tempPacket.Write(saveFile.Inventory);

            tempPacket.Write(saveFile.Vitals.Count);
            for (int i = 0; i < saveFile.Vitals.Count; i++) { tempPacket.Write(saveFile.Vitals[i]); }

            tempPacket.Write(saveFile.Chunks.Count);
            for (int i = 0; i < saveFile.Chunks.Count; i++)
            {
                tempPacket.Write(saveFile.Chunks[i].Coords);

                for (int x = 0; x < saveFile.VertsPerLine; x++)
                {
                    for (int y = 0; y < saveFile.VertsPerLine; y++)
                    {
                        tempPacket.Write(saveFile.Chunks[i].HeightMap[x, y]);
                    }
                }

                tempPacket.Write(saveFile.Chunks[i].PropTypes.Count);
                for (int b = 0; b < saveFile.Chunks[i].PropTypes.Count; b++)
                {
                    tempPacket.Write(saveFile.Chunks[i].PropTypes[b].Props.Count);
                    for (int j = 0; j < saveFile.Chunks[i].PropTypes[b].Props.Count; j++)
                    {
                        tempPacket.Write(saveFile.Chunks[i].PropTypes[b].Props[j].ID);
                        tempPacket.Write(saveFile.Chunks[i].PropTypes[b].Props[j].Pos);
                        tempPacket.Write(saveFile.Chunks[i].PropTypes[b].Props[j].Scale);
                        tempPacket.Write(saveFile.Chunks[i].PropTypes[b].Props[j].Euler);
                    }
                }

                tempPacket.Write(saveFile.Chunks[i].Storage.Count);
                for (int j = 0; j < saveFile.Chunks[i].Storage.Count; j++)
                {
                    tempPacket.Write(saveFile.Chunks[i].Storage[j].ID);
                    tempPacket.Write(saveFile.Chunks[i].Storage[j].Pos);
                    tempPacket.Write(saveFile.Chunks[i].Storage[j].Rot);

                    tempPacket.Write(saveFile.Chunks[i].Storage[j].Items);
                }
            }

            var saveChunks = BufferSplit(tempPacket.buffer.ToArray(), 4000);
            int count = 0;

            foreach (byte[] chunk in saveChunks)
            {
                using (Packet _packet = new Packet((int)ClientPackets.SaveFileTransfer))
                {
                    _packet.Write(count);
                    _packet.Write(chunk);

                    SendTCPData(_packet);
                }

                count++;
            }

            using (Packet _packet = new Packet((int)ClientPackets.SaveFileTransferEnd))
            {
                _packet.Write(count);
                SendTCPData(_packet);
            }
        }
    }

    public static void RequestChunk(Vector2 chunkCoord)
    {
        using (Packet packet = new Packet((int)ClientPackets.RequestChunk))
        {
            packet.Write(chunkCoord);
            SendTCPData(packet);
        }
    }

    public static void JoinSave()
    {
        using (Packet packet = new Packet((int)ClientPackets.JoinSave))
        {
            SendTCPData(packet);
        }
    }

    /*
    public static void SendPlayerMovement(float horizontal, float vertical, bool jump)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
        {
            _packet.Write(horizontal);
            _packet.Write(vertical);
            _packet.Write(jump);

            _packet.Write(GameManager.ActivePlayerManager.transform.rotation);
            _packet.Write(GameManager.ActiveCamera.transform.rotation);

            _packet.Write(GameManager.moving);

            SendUDPData(_packet);
        }
    }

    public static void UpdateVital(float healthChange, float hungerChange)
    {
        using (Packet _packet = new Packet((int)ClientPackets.modifyVital))
        {
            _packet.Write(healthChange);
            _packet.Write(hungerChange);

            SendTCPData(_packet);
        }
    }

    public static void AddStructure(GameObject structure)
    {
        using (Packet _packet = new Packet((int)ClientPackets.addStructure))
        {
            _packet.Write(structure.transform.position);
            _packet.Write(structure.name);
            _packet.Write(structure.transform.eulerAngles);

            SendTCPData(_packet);
        }
    }

    public static void Craft(CraftingVariant recipe)
    {
        using (Packet _packet = new Packet((int)ClientPackets.craft))
        {
            _packet.Write(recipe.Output.Length);
            for (int i = 0; i < recipe.Output.Length; i++) { _packet.Write(recipe.Output[i].name); }
            _packet.Write(recipe.Input.Length);
            for (int i = 0; i < recipe.Input.Length; i++) { _packet.Write(recipe.Input[i].name); }

            SendTCPData(_packet);
        }
    }
    */

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

    #region Send Data Funcs

    public static void SendTCPData(Packet _packet) { /*_packet.WriteLength();*/ DataManager.tcp.SendData(_packet); }
    public static void SendUDPData(Packet _packet) { /*_packet.WriteLength();*/ DataManager.udp.SendData(_packet);}

    #endregion
}