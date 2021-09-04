using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum PacketType
{
    Plain,
    NetRead,
    NetWrite
}

public class Packet : IDisposable
{
    // Abbreviations used in packet
    // rp - Read Pos
    // rb - Readable Buffer

    public List<byte> buffer = new List<byte>();
    public byte[] rb;
    
    int rp = 0;
    int packetId = 0;

    bool singular = true;
    PacketType packetType;

    #region Initializing

    /// <summary>
    /// A bland empty packet for (de)serializing data!
    /// </summary>
    public Packet() { packetType = PacketType.Plain; }
    /// <summary>
    /// Creates a new packet for writing to (and ofc for networking)
    /// </summary>
    /// <param name="id">The id for the packet</param>
    public Packet(int id) { packetType = PacketType.NetWrite; packetId = id; }
    /// <summary>
    /// Creates a packet for reading from (and ofc for networking)
    /// </summary>
    /// <param name="data">The first (and possibly only) block of data</param>
    /// <param name="handlerCallback">The callback function to deserialize the data</param>
    public Packet(byte[] data, Action<Packet, int> handlerCallback)
    {
        SetBytes(data);
        rp = 5;

        packetType = PacketType.NetRead;

        packetId = BitConverter.ToInt32(data, 0);
        singular = BitConverter.ToBoolean(data, 4);

        if (singular)
        {
            handlerCallback(this, packetId);
            Dispose();
        }
    }

    #endregion
    #region Chunk Adding

    public void AddChunk(byte[] newBuffer, Action<Packet, int> handlerCallback)
    {
        if (singular) { return; }

        for (int i = 5; i < 16384; i++)
            Write(newBuffer[i]);

        if (BitConverter.ToBoolean(newBuffer, 4))
        {
            rb = buffer.ToArray();
            handlerCallback(this, BitConverter.ToInt32(newBuffer, 0));
            DataManager.RecvingPackets.Remove(packetId);

            singular = true;
            Dispose();
        }
    }

    #endregion

    #region Sending

    public void Send()
    {
        if (buffer.Count <= 4091)
        {
            buffer.InsertRange(0, BitConverter.GetBytes(packetId));
            buffer.InsertRange(4, BitConverter.GetBytes(true));

            DataManager.tcp.SendData(buffer.ToArray());
            Dispose();
        }
        else
        {
            byte[] firstBuffer = new byte[4096];
            int unwritten_bytes = buffer.Count;

            byte[] id_data = BitConverter.GetBytes(packetId);
            for (int i = 0; i < 4; i++)
                firstBuffer[i] = id_data[i];

            firstBuffer[4] = BitConverter.GetBytes(false)[0];
            buffer.CopyTo(0, firstBuffer, 5, 4091);

            DataManager.tcp.SendData(firstBuffer);
            unwritten_bytes -= 4091;

            int b = 1;
            while (unwritten_bytes > 4091)
            {
                byte[] midBuffer = new byte[4096];

                for (int j = 0; j < 4; j++)
                    midBuffer[j] = id_data[j];

                midBuffer[4] = BitConverter.GetBytes(false)[0];
                buffer.CopyTo(b * 4091, midBuffer, 5, 4091);

                DataManager.tcp.SendData(midBuffer);


                b++;
            }

            byte[] endBuffer = new byte[4096];

            for (int j = 0; j < 4; j++)
                endBuffer[j] = id_data[j];

            endBuffer[4] = BitConverter.GetBytes(false)[0];
            buffer.CopyTo(buffer.Count - unwritten_bytes, endBuffer, 5, unwritten_bytes);

            DataManager.tcp.SendData(endBuffer);
        }
    }

    #endregion
    #region Functions

    public void SetBytes(byte[] _data) { Write(_data); rb = buffer.ToArray(); }
    public void WriteLength() { buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count)); }
    public void InsertInt(int value) { buffer.InsertRange(0, BitConverter.GetBytes(value)); }
    public byte[] ToArray() { rb = buffer.ToArray(); return rb; }
    public int Length() { return buffer.Count; }
    public int UnreadLength() { return Length() - rp; }

    public void Reset(bool _shouldReset = true)
    {
        if (_shouldReset)
        {
            buffer.Clear();
            rb = null;
            rp = 0;
        }
        else { rp -= 4; }
    }

    #endregion

    #region Write Data

    public void Write(byte value) { buffer.Add(value); }
    public void Write(sbyte value) { Write((byte)value); }

    public void Write(short value) { buffer.AddRange(BitConverter.GetBytes(value)); }
    public void Write(ushort value) { buffer.AddRange(BitConverter.GetBytes(value)); }

    public void Write(int value) { buffer.AddRange(BitConverter.GetBytes(value)); }
    public void Write(uint value) { buffer.AddRange(BitConverter.GetBytes(value)); }

    public void Write(float value) { buffer.AddRange(BitConverter.GetBytes(value)); }
    public void Write(bool value) { buffer.AddRange(BitConverter.GetBytes(value)); }
    public void Write(string value) { Write(value.Length); buffer.AddRange(Encoding.ASCII.GetBytes(value)); }

    public void Write(Vector2 value) { Write(value.x); Write(value.y); }
    public void Write(Vector3 value) { Write(value.x); Write(value.y); Write(value.z); }
    public void Write(Quaternion value) { Write(value.x); Write(value.y); Write(value.z); Write(value.w); }

    public void Write(byte[] value) { buffer.AddRange(value); }

    public void Write(List<Vector3> value)
    {
        Write(value.Count);
        for (int i = 0; i < value.Count; i++) { Write(value[i]); }
    }
    public void Write(List<Quaternion> value)
    {
        Write(value.Count);
        for (int i = 0; i < value.Count; i++) { Write(value[i]); }
    }
    public void Write(List<int> value)
    {
        Write(value.Count);
        for (int i = 0; i < value.Count; i++) { Write(value[i]); }
    }
    public void Write(List<string> value)
    {
        Write(value.Count);
        for (int i = 0; i < value.Count; i++) { Write(value[i]); }
    }
    #endregion
    #region Read Data

    public sbyte ReadSbyte() { rp += 1; return (sbyte)rb[rp - 1]; }
    public byte ReadByte() { rp += 1; return rb[rp - 1]; }

    public short ReadShort() { rp += 2; return BitConverter.ToInt16(rb, rp - 2); }
    public ushort ReadUshort() { rp += 2; return BitConverter.ToUInt16(rb, rp - 2); }

    public int ReadInt() { rp += 4; return BitConverter.ToInt32(rb, rp - 4); }
    public uint ReadUint() { rp += 4; return BitConverter.ToUInt32(rb, rp - 4); }

    public float ReadFloat() { rp += 4; return BitConverter.ToSingle(rb, rp - 4); }
    public bool ReadBool() { rp += 1; return BitConverter.ToBoolean(rb, rp - 1); }

    public string ReadString()
    {
        try
        {
            int length = ReadInt();
            rp += length;
            return Encoding.ASCII.GetString(rb, rp, length);
        }
        catch { throw new Exception("Could not read value of type 'string'!"); }
    }

    public Vector2 ReadVector2() { return new Vector2(ReadFloat(), ReadFloat()); }
    public Vector3 ReadVector3() { return new Vector3(ReadFloat(), ReadFloat(), ReadFloat()); }
    public Quaternion ReadQuaternion() { return new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat()); }

    public List<Vector3> ReadVector3List()
    {
        List<Vector3> list = new List<Vector3>();
        int length = ReadInt();
        for (int i = 0; i < length; i++) { list.Add(ReadVector3()); }
        return list;
    }
    public List<Quaternion> ReadQuaternionList()
    {
        List<Quaternion> list = new List<Quaternion>();
        int length = ReadInt();
        for (int i = 0; i < length; i++) { list.Add(ReadQuaternion()); }
        return list;
    }
    public List<int> ReadIntList()
    {
        List<int> list = new List<int>();
        int length = ReadInt();
        for (int i = 0; i < length; i++) { list.Add(ReadInt()); }
        return list;
    }
    public List<string> ReadStringList()
    {
        List<string> list = new List<string>();
        int length = ReadInt();
        for (int i = 0; i < length; i++) { list.Add(ReadString()); }
        return list;
    }

    public byte[] ReadBytes(int _length, bool mrp = true) { if (mrp) { rp += _length; } return buffer.GetRange(rp, _length).ToArray(); }


    #endregion

    #region GC Code

    private bool disposed = false;

    void Dispose(bool _disposing)
    {
        if (!disposed)
        {
            if (_disposing)
            {
                buffer = null;
                rb = null;
                rp = 0;
            }

            disposed = true;
        }
    }
    public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }

    #endregion
}
