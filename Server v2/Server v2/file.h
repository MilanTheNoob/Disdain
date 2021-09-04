#ifndef FILE_H
#define FILE_H

#include "packet.h"

struct file_data
{
	packet serialize_packet;
	bool success = false;
};

file_data* read_file(const char* file_name)
{
	file_data* data = new file_data();

	FILE* stream;
	int response = fopen_s(&stream, file_name, "rb");

	if (response == 0)
	{
		fseek(stream, 0, SEEK_END);
		int stream_size = ftell(stream);
		rewind(stream);

		if (stream_size > 0)
		{
			data->serialize_packet = packet(0, stream_size);
			response = fread(data->serialize_packet.buffer, 1, stream_size, stream);

			data->success = true;
		}
	}

	fclose(stream);
	return data;
}

void write_file(const char* file_name, packet* serialize_data)
{
	FILE* stream;
	fopen_s(&stream, file_name, "w");
	
	fwrite(serialize_data->buffer, 1, serialize_data->writePos, stream);
	fclose(stream);
}

#endif // !FILE_H
