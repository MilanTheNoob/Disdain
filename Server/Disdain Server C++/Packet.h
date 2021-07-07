#ifndef PACKET_CPP
#define PACKET_CPP

#include "Vectors.h"
#include <string>

struct Packet
{
public:
    int readPos = 0;
    int writePos = 0;
    char buffer[4096];

    Packet(bool write, int packetType) { memset(buffer, 0, 4096); if (write) { Write(packetType); } }

    int Length() { return sizeof(buffer); }
    int UnreadLength() { return Length() - readPos; }
    void Reset() { memset(buffer, 0, sizeof(buffer)); readPos = 0; writePos = 0; }

    void Write(int value) { memcpy(&buffer[writePos], &value, 4); writePos += 4; }
    void Write(long value) { memcpy(&buffer[writePos], &value, 8); writePos += 8; }
    void Write(float value) { memcpy(&buffer[writePos], &value, 4); writePos += 4; }
    void Write(bool value) { memcpy(&buffer[writePos], &value, 1); writePos += 1; }
    void Write(std::string value) { int l = sizeof(value); Write(l); memcpy(&buffer[writePos], &value, l); writePos += l; }

    void Write(Vector2 value) { Write(value.x); Write(value.y); }
    void Write(Vector3 value) { Write(value.x); Write(value.y); Write(value.z); }
    void Write(Quaternion value) { Write(value.x); Write(value.y); Write(value.z); Write(value.w); }

    void Write(std::string* value, int length)
    {
        Write(length);
        for (int i = 0; i < length; i++) { Write(value[i]); }
    }
    void Write(float* value, int length)
    {
        Write(length);
        for (int i = 0; i < length; i++) { Write(value[i]); }
    }
    void Write(Vector3* value, int length)
    {
        Write(length);
        for (int i = 0; i < length; i++) { Write(value[i]); }
    }

    char ReadChar() { readPos++; return buffer[readPos - 1]; }
    short ReadShort() { short i = 0; memcpy(&i, &buffer[readPos], 2); readPos += 2; return i; }
    int ReadInt() { int i = 0; memcpy(&i, &buffer[readPos], 4); readPos += 4; return i; }
    long ReadLong() { long i = 0; memcpy(&i, &buffer[readPos], 8); readPos += 8; return i; }
    float ReadFloat() { float i = 0; memcpy(&i, &buffer[readPos], 4); readPos += 4; return i; }
    bool ReadBool() { bool i = false; memcpy(&i, &buffer[readPos], 1); readPos += 1; return i; }
    std::string ReadString() { int l = ReadInt(); std::string i = ""; memset(&i, 1, l); memcpy(&i, &buffer[readPos], l); readPos += l; return i; }

    Vector2 ReadVector2() { return Vector2(ReadFloat(), ReadFloat()); }
    Vector3 ReadVector3() { return Vector3(ReadFloat(), ReadFloat(), ReadFloat()); }
    Quaternion ReadQuaternion() { return Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat()); }

    Vector3* ReadVector3Array()
    {
        int length = ReadInt();
        Vector3* list = new Vector3[length];

        for (int i = 0; i < length; i++) { list[i] = ReadVector3(); }
        return list;
    }
    Quaternion* ReadQuaternionArray()
    {
        int length = ReadInt();
        Quaternion* list = new Quaternion[length];

        for (int i = 0; i < length; i++) { list[i] = ReadQuaternion(); }
        return list;
    }
    int* ReadIntArray()
    {
        int length = ReadInt();
        int* list = new int[length];

        for (int i = 0; i < length; i++) { list[i] = ReadInt(); }
        return list;
    }
    float* ReadFloatArray()
    {
        int length = ReadInt();
        float* list = new float[length];

        for (int i = 0; i < length; i++) { list[i] = ReadFloat(); }
        return list;
    }
    std::string* ReadStringArray()
    {
        int length = ReadInt();
        std::string* list = new std::string[length];

        for (int i = 0; i < length; i++) { list[i] = ReadString(); }
        return list;
    }
};

#endif