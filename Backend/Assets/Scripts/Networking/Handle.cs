using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Handle : MonoBehaviour
{
    public static void OnConnect(Packet packet) { }

    public static void BackendLogin(Packet packet)
    {
        print("Client has recieved a response from the server");

        if (packet.ReadInt() == 69)
        {
            MapPreviewUI.GD = new GenerateData(packet);
            MenuManager.instance.SwitchTab(1);

            print("Client has successfully logged in to the server");
        }
    }

    public static void BackendPreview(Packet packet)
    {
        byte size = packet.ReadByte();
        int count = 0;
        SaveFile.ChunkClass[] chunks = new SaveFile.ChunkClass[size * size];

        print("Receiving preview data from the server\nLoading " + (size * size) + " chunks from packet");

        for (byte x = 0; x < size; x++)
        {
            for (byte y = 0; y < size; y++)
            {
                chunks[count] = new SaveFile.ChunkClass();
                chunks[count].Deserialize(packet, new Vector2(y, -x));

                count++;
            }
        }

        MapPreview.instance.PreviewChunks(chunks);
    }
}
