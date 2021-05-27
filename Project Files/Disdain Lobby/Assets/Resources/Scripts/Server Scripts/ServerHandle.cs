using System.Collections.Generic;
using UnityEngine;

public class ServerHandle
{
    public static void WelcomeReceived(int _fromClient, Packet _packet)
    {
        _packet.ReadInt();
        Server.clients[_fromClient].SendIntoGame(_packet.ReadString(), _packet.ReadInt());
    }

    public static void PlayerMovement(int _fromClient, Packet _packet)
    {
        float horizontal = _packet.ReadFloat();
        float vertical = _packet.ReadFloat();
        bool jump = _packet.ReadBool();

        Quaternion _rotation = _packet.ReadQuaternion();

        Server.clients[_fromClient].player.SetInput(horizontal, vertical, _rotation, jump);
    }
}
