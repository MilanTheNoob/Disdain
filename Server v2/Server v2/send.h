#ifndef SEND_H
#define SEND_H

#include "packet.h"
#include "client.h"

void on_connect_send(int id, SOCKET* sock)
{
	packet send_packet = packet(0);
	send_packet.write_int32(id);

	send_packet.send_packet(sock);
}

#endif