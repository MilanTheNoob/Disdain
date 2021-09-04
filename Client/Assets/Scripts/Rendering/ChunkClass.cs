using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkClass
{
    public List<PropClass> Props = new List<PropClass>();
    public List<StorageClass> Storage = new List<StorageClass>();

    public float[] HeightMap;
    public Vector2 Coord;

    public void Deserialize(Packet packet, Vector2 coord)
    {
        Coord = coord;
        HeightMap = new float[4096];
        for (int i = 0; i < 4096; i++) { HeightMap[i] = packet.ReadShort() / 10f; }

        ushort pl = packet.ReadUshort();
        for (int i = 0; i < pl; i++)
        {
            PropClass prop = new PropClass
            {
                PropGroup = packet.ReadByte(),
                PropId = packet.ReadByte(),

                Euler = new Vector3(packet.ReadShort(), packet.ReadShort(), packet.ReadShort())
            };

            int x = packet.ReadByte();
            int z = packet.ReadByte();
            float y = HeightMap[(x * 64) + z];

            float scale = packet.ReadByte() / 10f;
            prop.Scale = new Vector3(scale, scale, scale);

            prop.Pos = new Vector3(x, y, z);
            Props.Add(prop);
        }
    }
}
public class PropClass
{
    public byte PropGroup;
    public byte PropId;

    public Vector3 Pos;
    public Vector3 Scale;
    public Vector3 Euler;
}
public class StorageClass
{
    public string ID;

    public Vector3 Pos;
    public Vector3 Rot;

    public List<string> Items;
}