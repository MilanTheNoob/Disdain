using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GenerateData
{
    public List<BiomeData> Biomes = new List<BiomeData>();
    public HeightmapData BiomeHeightmap;

    #region Init

    public GenerateData() { }
    /// <summary>
    /// Creates a new GenerateData from serialized data
    /// </summary>
    /// <param name="packet"></param>
    public GenerateData(Packet packet)
    {
        byte bl = packet.ReadByte();
        for (int i = 0; i < bl; i++)
        {
            BiomeData biome = new BiomeData();

            byte pgl = packet.ReadByte();
            for (int j = 0; j < pgl; j++)
            {
                PropData prop = new PropData
                {
                    GroupId = packet.ReadByte(),
                    PropId = packet.ReadByte(),

                    Rotation = packet.ReadVector3(),

                    MaxScale = packet.ReadFloat(),
                    MinScale = packet.ReadFloat(),

                    GenerateType = packet.ReadByte()
                };

                if (prop.GenerateType == 0)
                {
                    prop.PerChunkMin = packet.ReadByte();
                    prop.PerChunkMax = packet.ReadByte();
                }
                else if (prop.GenerateType == 1)
                {
                    prop.Chance = packet.ReadByte();
                }
                else if (prop.GenerateType == 2)
                {
                    prop.Perlin = packet.ReadFloat();
                    prop.PerlinMap = ReadHeightmap(packet);
                }

                prop.Bounds = packet.ReadVector2();

                biome.Props.Add(prop);
            }

            byte bhl = packet.ReadByte();
            for (int j = 0; j < bhl; j++) biome.Heightmaps.Add(ReadHeightmap(packet));

            Biomes.Add(biome);
        }

        BiomeHeightmap = ReadHeightmap(packet);
    }

    #endregion
    #region Serializing

    public Packet Serialize(Packet packet)
    {
        packet.Write((byte)Biomes.Count);
        for (int i = 0; i < Biomes.Count; i++)
        {
            packet.Write((byte)Biomes[i].Props.Count);
            for (int j = 0; j < Biomes[i].Props.Count; j++)
            {
                packet.Write(Biomes[i].Props[j].GroupId);
                packet.Write(Biomes[i].Props[j].PropId);

                packet.Write(Biomes[i].Props[j].Rotation);

                packet.Write(Biomes[i].Props[j].MaxScale);
                packet.Write(Biomes[i].Props[j].MinScale);

                packet.Write(Biomes[i].Props[j].GenerateType);
                if (Biomes[i].Props[j].GenerateType == 0)
                {
                    packet.Write(Biomes[i].Props[j].PerChunkMin);
                    packet.Write(Biomes[i].Props[j].PerChunkMax);
                }
                else if (Biomes[i].Props[j].GenerateType == 1)
                {
                    packet.Write(Biomes[i].Props[j].Chance);
                }
                else if (Biomes[i].Props[j].GenerateType == 2)
                {
                    packet.Write(Biomes[i].Props[j].Perlin);

                    packet.Write(Biomes[i].Props[j].PerlinMap.Scale);
                    packet.Write(Biomes[i].Props[j].PerlinMap.HeightMultiplier);
                    packet.Write(Biomes[i].Props[j].PerlinMap.Octaves);

                    packet.Write(Biomes[i].Props[j].PerlinMap.Persistence);
                    packet.Write(Biomes[i].Props[j].PerlinMap.Lacunarity);
                }

                packet.Write(Biomes[i].Props[j].Bounds);
            }

            packet.Write((byte)Biomes[i].Heightmaps.Count);
            for (int j = 0; j < Biomes[i].Heightmaps.Count; j++)
            {
                packet.Write(Biomes[i].Heightmaps[j].Scale);
                packet.Write(Biomes[i].Heightmaps[j].HeightMultiplier);
                packet.Write(Biomes[i].Heightmaps[j].Octaves);

                packet.Write(Biomes[i].Heightmaps[j].Persistence);
                packet.Write(Biomes[i].Heightmaps[j].Lacunarity);
            }
        }

        packet.Write(BiomeHeightmap.Scale);
        packet.Write(BiomeHeightmap.HeightMultiplier);
        packet.Write(BiomeHeightmap.Octaves);

        packet.Write(BiomeHeightmap.Persistence);
        packet.Write(BiomeHeightmap.Lacunarity);

        return packet;
    }

    #endregion

    #region Misc Funcs

    HeightmapData ReadHeightmap(Packet packet)
    {
        return new HeightmapData
        {
            Scale = packet.ReadInt(),
            HeightMultiplier = packet.ReadInt(),
            Octaves = packet.ReadInt(),

            Persistence = packet.ReadFloat(),
            Lacunarity = packet.ReadFloat()
        };
    }

    #endregion
    #region Nested Classes

    [Serializable]
    public class HeightmapData
    {
        public int Scale = 50;
        public int HeightMultiplier = 10;
        public int Octaves = 7;

        public float Persistence = 6; // Decimal Number 1dp
        public float Lacunarity = 144; // Decimal Number 2dp

        public bool UseChance;
        public byte Chance;
    }

    [Serializable]
    public class PropData
    {
        public byte GroupId;
        public byte PropId;

        public Vector3 Rotation;

        public float MaxScale; // Decimal Number 1dp
        public float MinScale; // Decimal Number 2dp

        public byte GenerateType;

        public byte PerChunkMin;
        public byte PerChunkMax;

        public byte Chance;

        public float Perlin;
        public HeightmapData PerlinMap;

        public bool UseBounds;
        public Vector2 Bounds;
    }

    [Serializable]
    public class BiomeData
    {
        public List<PropData> Props = new List<PropData>();
        public List<HeightmapData> Heightmaps = new List<HeightmapData>();
    }

    #endregion
}