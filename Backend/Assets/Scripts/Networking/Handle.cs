using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Handle : MonoBehaviour
{
    public static void OnConnect(Packet packet) { }
    public static void BackendLogin(Packet packet)
    {
        print("Client has recieved a response from the server");
        if (packet.ReadBool())
        {
            print("Client has successfully logged in to the server");
            MenuManager.instance.SwitchTab(1);
            MapPreview.GenerateData = new GenerateData(packet);
        }
    }
}
