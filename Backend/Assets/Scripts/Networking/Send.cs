using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Send : MonoBehaviour
{
    public static void BackendLogin()
    {
        Packet packet = new Packet(100);
        packet.Write("11ac1c39-1957-44c0-aa3e-2ed005581592");

        packet.Send();
    }

    public static void BackendPreview(GenerateData generateData, int previewSize)
    {
        Packet packet = new Packet(101);

        packet.Write(previewSize);
        packet = generateData.Serialize(packet);

        packet.Send();
    }

    public static void BackendSave(GenerateData gd)
    {
        Packet packet = new Packet(102);
        packet = gd.Serialize(packet);

        packet.Send();
    }
    public static void BackendExit() { new Packet(103).Send(); }
}
