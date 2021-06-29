using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

/// <summary>Sent from server to client.</summary>
public enum ServerPackets
{
    // Basic Communication Packets
    OnConnect = 1,
    LoginResponse,
    SignupRespnse,
    BanHammer,
    Disconnect,
    // Lobby Packets
    PlayerSpawnL,
    PlayerLeaveL,
    PlayerDataL,
    // Singleplayer Packets
    SaveFileTransfer,
    SaveFileTransferEnd,
    AddChunk
}

/// <summary>Sent from client to server.</summary>
public enum ClientPackets
{
    // Basic Communication Packets
    Login = 1,
    Signup,
    // Joining & Leaving Packets
    JoinLobby,
    JoinSave,
    JoinServer,
    // Lobby Packets
    MovementL,
    // Singleplayer Packets
    SaveFileTransfer,
    SaveFileTransferEnd,
    RequestChunk
}

public class Packet : IDisposable
{
    public List<byte> buffer;
    public byte[] readableBuffer;
    public int readPos;

    #region Init

    /// <summary>Creates a new empty packet (without an ID).</summary>
    public Packet()
    {
        buffer = new List<byte>(); // Initialize buffer
        readPos = 0; // Set readPos to 0
    }

    /// <summary>Creates a new packet with a given ID. Used for sending.</summary>
    /// <param name="id">The packet ID.</param>
    public Packet(int id)
    {
        buffer = new List<byte>(); // Initialize buffer
        readPos = 0; // Set readPos to 0

        Write(id); // Write packet id to the buffer
    }

    /// <summary>Creates a packet from which data can be read. Used for receiving.</summary>
    /// <param name="data">The bytes to add to the packet.</param>
    public Packet(byte[] data)
    {
        buffer = new List<byte>(); // Initialize buffer
        readPos = 0; // Set readPos to 0

        SetBytes(data);
    }

    #endregion
    #region Functions
    /// <summary>Sets the packet's content and prepares it to be read.</summary>
    /// <param name="data">The bytes to add to the packet.</param>
    public void SetBytes(byte[] data)
    {
        Write(data);
        readableBuffer = buffer.ToArray();
    }

    /// <summary>Inserts the length of the packet's content at the start of the buffer.</summary>
    public void WriteLength()
    {
        buffer.InsertRange(0, BitConverter.GetBytes(buffer.Count)); // Insert the byte length of the packet at the very beginning
    }

    /// <summary>Inserts the given int at the start of the buffer.</summary>
    /// <param name="value">The int to insert.</param>
    public void InsertInt(int value)
    {
        buffer.InsertRange(0, BitConverter.GetBytes(value)); // Insert the int at the start of the buffer
    }

    /// <summary>Gets the packet's content in array form.</summary>
    public byte[] ToArray()
    {
        readableBuffer = buffer.ToArray();
        return readableBuffer;
    }

    /// <summary>Gets the length of the packet's content.</summary>
    public int Length()
    {
        return buffer.Count; // Return the length of buffer
    }

    /// <summary>Gets the length of the unread data contained in the packet.</summary>
    public int UnreadLength()
    {
        return Length() - readPos; // Return the remaining length (unread)
    }

    /// <summary>Resets the packet instance to allow it to be reused.</summary>
    /// <param name="shouldReset">Whether or not to reset the packet.</param>
    public void Reset(bool shouldReset = true)
    {
        if (shouldReset)
        {
            buffer.Clear(); // Clear buffer
            readableBuffer = null;
            readPos = 0; // Reset readPos
        }
        else
        {
            readPos -= 4; // "Unread" the last read int
        }
    }
    #endregion

    #region Write Data

    #region Basic Values

    /// <summary>Adds a byte to the packet.</summary>
    /// <param name="value">The byte to add.</param>
    public void Write(byte value)
    {
        buffer.Add(value);
    }
    /// <summary>Adds an array of bytes to the packet.</summary>
    /// <param name="value">The byte array to add.</param>
    public void Write(byte[] value)
    {
        buffer.AddRange(value);
    }
    /// <summary>Adds a short to the packet.</summary>
    /// <param name="value">The short to add.</param>
    public void Write(short value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }
    /// <summary>Adds an int to the packet.</summary>
    /// <param name="value">The int to add.</param>
    public void Write(int value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }
    /// <summary>Adds a long to the packet.</summary>
    /// <param name="value">The long to add.</param>
    public void Write(long value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }
    /// <summary>Adds a float to the packet.</summary>
    /// <param name="value">The float to add.</param>
    public void Write(float value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }
    /// <summary>Adds a bool to the packet.</summary>
    /// <param name="value">The bool to add.</param>
    public void Write(bool value)
    {
        buffer.AddRange(BitConverter.GetBytes(value));
    }
    /// <summary>Adds a string to the packet.</summary>
    /// <param name="value">The string to add.</param>
    public void Write(string value)
    {
        Write(value.Length); // Add the length of the string to the packet
        buffer.AddRange(Encoding.ASCII.GetBytes(value)); // Add the string itself
    }

    #endregion
    #region Numerics

    /// <summary>
    /// Adds a Vector2 to the packet
    /// </summary>
    /// <param name="value">The Vector2 to add</param>
    public void Write(Vector2 value)
    {
        Write(value.X);
        Write(value.Y);
    }

    /// <summary>Adds a Vector3 to the packet.</summary>
    /// <param name="value">The Vector3 to add.</param>
    public void Write(Vector3 value)
    {
        Write(value.X);
        Write(value.Y);
        Write(value.Z);
    }
    /// <summary>Adds a Quaternion to the packet.</summary>
    /// <param name="value">The Quaternion to add.</param>
    public void Write(Quaternion value)
    {
        Write(value.X);
        Write(value.Y);
        Write(value.Z);
        Write(value.W);
    }

    #endregion
    #region Lists

    /// <summary>
    /// Adds a String List to the packet.
    /// </summary>
    /// <param name="value">The String List to add.</param>
    public void Write(List<string> value)
    {
        Write(value.Count);
        for (int i = 0; i < value.Count; i++) { Write(value[i]); }
    }

    /// <summary>
    /// Adds a Float List to the packet
    /// </summary>
    /// <param name="value">The Float List to add</param>
    public void Write(List<float> value)
    {
        Write(value.Count);
        for (int i = 0; i < value.Count; i++) { Write(value[i]); }
    }

    #endregion

    #endregion
    #region Read Data

    #region Basic Values

    /// <summary>Reads a byte from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public byte ReadByte(bool moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            byte value = readableBuffer[readPos];
            if (moveReadPos) readPos += 1;
            return value;
        }
        else
        {
            throw new Exception("Could not read value of type 'byte'!");
        }
    }

    /// <summary>Reads an array of bytes from the packet.</summary>
    /// <param name="length">The length of the byte array.</param>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public byte[] ReadBytes(int length, bool moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            byte[] value = buffer.GetRange(readPos, length).ToArray();
            if (moveReadPos) readPos += length;
            return value;
        }
        else
        {
            throw new Exception("Could not read value of type 'byte[]'!");
        }
    }

    /// <summary>Reads a short from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public short ReadShort(bool moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            short value = BitConverter.ToInt16(readableBuffer, readPos);
            if (moveReadPos) readPos += 2;
            return value;
        }
        else
        {
            throw new Exception("Could not read value of type 'short'!");
        }
    }

    /// <summary>Reads an int from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public int ReadInt(bool moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            int value = BitConverter.ToInt32(readableBuffer, readPos);
            if (moveReadPos) readPos += 4;
            return value;
        }
        else
        {
            throw new Exception("Could not read value of type 'int'!");
        }
    }

    /// <summary>Reads a long from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public long ReadLong(bool moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            long value = BitConverter.ToInt64(readableBuffer, readPos);
            if (moveReadPos) readPos += 8;
            return value;
        }
        else
        {
            throw new Exception("Could not read value of type 'long'!");
        }
    }

    public ulong ReadULong(bool moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            long value = BitConverter.ToUInt64(readableBuffer, readPos);
            if (moveReadPos) readPos += 8;
            return value;
        }
        else
        {
            throw new Exception("Could not read value of type 'long'!");
        }
    }

    /// <summary>Reads a float from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public float ReadFloat(bool moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            float value = BitConverter.ToSingle(readableBuffer, readPos);
            if (moveReadPos) readPos += 4;
            return value;
        }
        else
        {
            throw new Exception("Could not read value of type 'float'!");
        }
    }

    /// <summary>Reads a bool from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public bool ReadBool(bool moveReadPos = true)
    {
        if (buffer.Count > readPos)
        {
            bool value = BitConverter.ToBoolean(readableBuffer, readPos);
            if (moveReadPos) readPos += 1;
            return value;
        }
        else
        {
            throw new Exception("Could not read value of type 'bool'!");
        }
    }

    /// <summary>Reads a string from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public string ReadString(bool moveReadPos = true)
    {
        try
        {
            int length = ReadInt();
            string value = Encoding.ASCII.GetString(readableBuffer, readPos, length);
            if (moveReadPos && value.Length > 0) readPos += length;
            return value;
        }
        catch
        {
            throw new Exception("Could not read value of type 'string'!");
        }
    }

    #endregion
    #region Numerics

    /// <summary>
    /// Reads a Vector2 from the packet
    /// </summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public Vector2 ReadVector2(bool moveReadPos = true)
    {
        return new Vector2(ReadFloat(moveReadPos), ReadFloat(moveReadPos));
    }

    /// <summary>Reads a Vector3 from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public Vector3 ReadVector3(bool moveReadPos = true)
    {
        return new Vector3(ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos));
    }

    /// <summary>Reads a Quaternion from the packet.</summary>
    /// <param name="moveReadPos">Whether or not to move the buffer's read position.</param>
    public Quaternion ReadQuaternion(bool moveReadPos = true)
    {
        return new Quaternion(ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos), ReadFloat(moveReadPos));
    }

    #endregion
    #region Lists

    public List<Vector3> ReadVector3List(bool _moveReadPos = true)
    {
        List<Vector3> list = new List<Vector3>();
        int _length = ReadInt();
        for (int i = 0; i < _length; i++) { list.Add(ReadVector3(_moveReadPos)); }
        return list;
    }
    public List<Quaternion> ReadQuaternionList(bool _moveReadPos = true)
    {
        List<Quaternion> list = new List<Quaternion>();
        int _length = ReadInt();
        for (int i = 0; i < _length; i++) { list.Add(ReadQuaternion(_moveReadPos)); }
        return list;
    }
    public List<int> ReadIntList(bool _moveReadPos = true)
    {
        List<int> list = new List<int>();
        int _length = ReadInt();
        for (int i = 0; i < _length; i++) { list.Add(ReadInt(_moveReadPos)); }
        return list;
    }
    public List<float> ReadFloatList(bool _moveReadPos = true)
    {
        List<float> list = new List<float>();
        int _length = ReadInt();
        for (int i = 0; i < _length; i++) { list.Add(ReadFloat(_moveReadPos)); }
        return list;
    }
    public List<string> ReadStringList(bool _moveReadPos = true)
    {
        List<string> list = new List<string>();
        int _length = ReadInt();
        for (int i = 0; i < _length; i++) { list.Add(ReadString(_moveReadPos)); }
        return list;
    }

    #endregion
    #region Arrays

    #endregion

    #endregion

    #region Misc

    private bool disposed = false;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                buffer = null;
                readableBuffer = null;
                readPos = 0;
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}