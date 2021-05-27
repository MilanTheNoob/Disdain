using System.Net;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class LobbyHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        int _myId = _packet.ReadInt();
        print(_myId);
        LobbyClient.instance.myId = _myId;
        LobbySend.WelcomeReceived();
    }

    public static void SpawnPlayer(Packet _packet)
    {
        LobbyManager.instance.SpawnPlayer(_packet.ReadInt(), _packet.ReadString(), _packet.ReadVector3(), _packet.ReadQuaternion(), 0);
    }

    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        if (!LobbyManager.players.ContainsKey(_id)) { return; }
        LobbyManager.players[_id].transform.position = _packet.ReadVector3();
    }

    public static void PlayerDisconnected(Packet _packet)
    {
        int _id = _packet.ReadInt();

        Destroy(LobbyManager.players[_id].gameObject);
        LobbyManager.players.Remove(_id);
    }
}
