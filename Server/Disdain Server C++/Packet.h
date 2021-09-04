#ifndef PACKET_H
#define PACKET_H

#include <iostream>
#include <string>

#include "vectors.h"
#include <exception>

struct packet
{
    bool read_or_write = false;
    int packet_id = 0;
    int size_estimate = 16380;
    bool singular = true;

    int current_chunks = 1;
    int readPos = 0;
    int writePos = 0;

    bool disposed;
    char* buffer;

#pragma region write functions

    void write_uint8(uint8_t value) { buffer[writePos] = value; writePos = writePos + 1; }
    void write_int8(int8_t value) { buffer[writePos] = value; writePos = writePos + 1; }

    void write_uint16(uint16_t value) { memcpy(&buffer[writePos], &value, 2); writePos = writePos + 2; }
    void write_int16(int16_t value) { memcpy(&buffer[writePos], &value, 2); writePos = writePos + 2; }

    void write_uint32(uint32_t value) { memcpy(&buffer[writePos], &value, 4); writePos = writePos + 4; }
    void write_int32(int value) { memcpy(&buffer[writePos], &value, 4); writePos = writePos + 4; }

    //void write_uint64(uint64_t value) { memcpy(&buffer[writePos], &value, 8); writePos = writePos + 8; }
    //void write_int64(int64_t value) { memcpy(&buffer[writePos], &value, 8); writePos = writePos + 8; }

    void write_float(float value) { memcpy(&buffer[writePos], &value, 4); writePos = writePos + 4; }
    void write_bool(bool value) { memcpy(&buffer[writePos], &value, 1); writePos = writePos + 1; }

    void write_vector2(vector2 value) { write_float(value.x); write_float(value.y); }
    void write_vector3(vector3 value) { write_float(value.x); write_float(value.y); write_float(value.z); }
    // quaternion here

    void write_string(std::string value) 
    { 
        int l = value.size(); 
        write_uint32(l); 
        
        memcpy(&buffer[writePos], &value, l); 
        writePos = writePos + l;
    }

#pragma endregion
#pragma region read functions

    uint8_t read_uint8() { readPos = readPos + 1; return buffer[readPos - 1]; }
    int8_t read_int8() { readPos = readPos + 1; return buffer[readPos - 1]; }

    uint16_t read_uint16() { uint16_t i = 0; memcpy(&i, &buffer[readPos], 2); readPos = readPos + 2; return i; }
    int16_t read_int16() { int16_t i = 0; memcpy(&i, &buffer[readPos], 2); readPos = readPos + 2; return i; }

    int read_int32() { int i = 0; memcpy(&i, &buffer[readPos], 4); readPos = readPos + 4; return i; }
    uint32_t read_uint32() { uint32_t i = 0; memcpy(&i, &buffer[readPos], 4); readPos = readPos + 4; return i; }

    float read_float() { float i = 0; memcpy(&i, &buffer[readPos], 4); readPos = readPos + 4; return i; }
    bool read_bool() { bool i = false; memcpy(&i, &buffer[readPos], 1); readPos = readPos + 1; return i; }

    vector2 read_vector2() { return vector2(read_float(), read_float()); }
    vector3 read_vector3() { return vector3(read_float(), read_float(), read_float()); }
    quaternion read_quaternion() { return quaternion(read_float(), read_float(), read_float(), read_float()); }

    std::string read_string() 
    { 
        int l = read_int32();
        std::string i;
        i.resize(l);
        for (int j = 0; j < l; j++)
        {
            i[j] = buffer[readPos];
            readPos = readPos + 1;
        }
        return i;
    }

#pragma endregion

#pragma region initializing

    /// <summary>
    /// A bland empty packet cause the C++ compiler is a bitch -_-
    /// </summary>
    packet() { disposed = true; }

    /// <summary>
    /// Creates a new packet for reading
    /// </summary>
    /// <param name="estimated_size">The estimated size of data to hold</param>
    /// <param name="id"> The 8 bit value representing the packet ID</param> 
    packet(int id, int estimated_size = 16379)
    {
        read_or_write = true;

        if (estimated_size > 2147483647)
        {
            estimated_size = 2147483647;
            std::cout << std::endl << "Packet cannot hold estimated amount of data!" << std::endl << std::endl;
        }
        if (estimated_size < 16380)
        {
            estimated_size = 16379;
            singular = true;
        }
        else
        {
            singular = false;
        }

        packet_id = id;
        disposed = false;

        buffer = new char[estimated_size];
        memset(buffer, 0, estimated_size);
    }
    /// <summary>
    /// Creates a packet for reading from.
    /// 
    /// NOTE - The function does not use and/or dispose of the inputted buffer,
    /// delete it yourself!
    /// </summary>
    /// <param name="value">The pointer to the first received buffer of serialized data</param>
    packet(char* new_buffer)
    {
        read_or_write = false;
        buffer = new char[4091];

        memcpy(&packet_id, &new_buffer[0], 4);
        memcpy(buffer, &new_buffer[5], 4091);
    }

#pragma endregion
#pragma region chunk adding

    /// <summary>
    /// Adds a chunk of serialized data to the packet.
    /// 
    /// NOTE - The function does not use and/or dispose of the inputted buffer,
    /// delete it yourself!
    /// </summary>
    /// <param name="new_buffer">The pointer to the to be added buffer</param>
    void add_chunk(char* new_buffer)
    {
        char* end_buffer = new char[sizeof(buffer) + 4091];

        memcpy(&end_buffer[0], &buffer[0], sizeof(buffer));
        memcpy(&end_buffer[current_chunks * 4091], &new_buffer[5], 4091);

        if (!disposed)
        {
            delete[] buffer;
            disposed = true;
        }
        buffer = end_buffer;
    }

#pragma endregion
#pragma region sending

    void send_packet(SOCKET* sock, bool dispose = true)
    {
        if (singular == true || writePos <= 16379) 
        { 
            char send_buffer[16384] = { 0 };

            memcpy(&send_buffer[0], &packet_id, 4);
            memcpy(&send_buffer[4], &singular, 1);
            memcpy(&send_buffer[5], &buffer[0], writePos);

            send((*sock), &send_buffer[0], 16384, 0);
        }
        else if (singular == false)
        {
            bool not_last = false;
            bool last = true;

            char first_buffer[16384] = { 0 };
            int unwritten_chars = writePos;

            memcpy(&first_buffer[0], &packet_id, 4);
            memcpy(&first_buffer[4], &singular, 1);
            memcpy(&first_buffer[5], &buffer[0], 16379);

            send((*sock), first_buffer, 16384, 0);
            unwritten_chars = unwritten_chars - 16379;

            int i = 1;
            while (unwritten_chars > 16379)
            {
                char iteration_buffer[16384] = { 0 };

                memcpy(&iteration_buffer[0], &packet_id, 4);
                memcpy(&iteration_buffer[4], &not_last, 1);
                memcpy(&iteration_buffer[5], &buffer[i * 16379], 16379);

                send((*sock), iteration_buffer, 16384, 0);

                unwritten_chars = unwritten_chars - 16379;
                i++;
            }

            char end_buffer[16384] = { 0 };

            memcpy(&end_buffer[0], &packet_id, 4);
            memcpy(&end_buffer[4], &last, 1);
            memcpy(&end_buffer[5], &buffer[writePos - unwritten_chars], unwritten_chars);

            send((*sock), end_buffer, 16384, 0);
        }
    }

#pragma endregion

    void dispose()
    {
        if (!disposed)
        {
            delete[] buffer;
            disposed = true;
        }
    }
};

#endif