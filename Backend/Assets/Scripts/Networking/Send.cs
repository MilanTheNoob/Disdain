using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Send : MonoBehaviour
{
    public static void BackendLogin(string password)
    {
        Packet packet = new Packet((int)ClientPackets.BackendLogin);
        packet.Write("11ac1c39-1957-44c0-aa3e-2ed005581592");
        packet.Write(password);

        DataManager.tcp.SendData(packet);
    }
}
