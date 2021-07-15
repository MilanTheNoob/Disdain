using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateData
{
    public List<BiomeData> Biomes = new List<BiomeData>();
    public List<HeightmapData> BaseHeightmaps = new List<HeightmapData>();

    public HeightmapData BiomeHeightmap;

    #region Init

    public GenerateData() { }
    /// <summary>
    /// Creates a new GenerateData from serialized data
    /// </summary>
    /// <param name="packet"></param>
    public GenerateData(Packet packet)
    {
        for (byte i = 0; i < packet.ReadByte(); i++)
        {
            BiomeData biome = new BiomeData();

            for (byte j = 0; j < packet.ReadByte(); j++)
            {
                List<PropData> propGroup = new List<PropData>();

                for (byte b = 0; b < packet.ReadByte(); b++)
                {
                    PropData prop = new PropData
                    {
                        RotateX = packet.ReadShort(),
                        RotateY = packet.ReadShort(),
                        RotateZ = packet.ReadShort(),

                        MaxScale = packet.ReadByte(),
                        MinScale = packet.ReadByte(),

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
                        prop.Perlin = packet.ReadByte();

                        prop.PerlinMap = new HeightmapData
                        {
                            Scale = packet.ReadShort(),
                            HeightMultiplier = packet.ReadShort(),
                            Octaves = packet.ReadByte(),

                            Persistence = packet.ReadByte(),
                            Lacunarity = packet.ReadByte()
                        };
                    }

                    prop.UseBounds = packet.ReadBool();
                    if (prop.UseBounds)
                    {
                        prop.BoundsX = packet.ReadShort();
                        prop.BoundsY = packet.ReadShort();
                        prop.BoundsZ = packet.ReadShort();
                    }

                    propGroup.Add(prop);
                }

                biome.Props.Add(propGroup);
            }

            for (byte j = 0; j < packet.ReadByte(); j++)
            {
                biome.Heightmaps.Add(new HeightmapData
                {
                    Scale = packet.ReadShort(),
                    HeightMultiplier = packet.ReadShort(),
                    Octaves = packet.ReadByte(),

                    Persistence = packet.ReadByte(),
                    Lacunarity = packet.ReadByte(),

                    UseChance = packet.ReadBool(),
                    Chance = packet.ReadByte()
                });
            }

            Biomes.Add(biome);
        }

        for (byte i = 0; i < packet.ReadByte(); i++)
        {
            BaseHeightmaps.Add(new HeightmapData
            {
                Scale = packet.ReadShort(),
                HeightMultiplier = packet.ReadShort(),
                Octaves = packet.ReadByte(),

                Persistence = packet.ReadByte(),
                Lacunarity = packet.ReadByte(),

                UseChance = packet.ReadBool(),
                Chance = packet.ReadByte()
            });
        }

        BiomeHeightmap = new HeightmapData
        {
            Scale = packet.ReadShort(),
            HeightMultiplier = packet.ReadShort(),
            Octaves = packet.ReadByte(),

            Persistence = packet.ReadByte(),
            Lacunarity = packet.ReadByte(),

            UseChance = packet.ReadBool(),
            Chance = packet.ReadByte()
        };
    }

    #endregion
    #region Serializing

    public void Serialize(Packet packet)
    {
        packet.Write((byte)Biomes.Count);
        for (byte i = 0; i < Biomes.Count; i++)
        {
            packet.Write((byte)Biomes[i].Props.Count);
            for (byte j = 0; j < Biomes[i].Props.Count; j++)
            {
                packet.Write((byte)Biomes[i].Props[j].Count);
                for (byte b = 0; b < packet.ReadByte(); b++)
                {
                    packet.Write(Biomes[i].Props[j][b].RotateX);
                    packet.Write(Biomes[i].Props[j][b].RotateY);
                    packet.Write(Biomes[i].Props[j][b].RotateZ);

                    packet.Write(Biomes[i].Props[j][b].MaxScale);
                    packet.Write(Biomes[i].Props[j][b].MinScale);

                    packet.Write(Biomes[i].Props[j][b].GenerateType);
                    if (Biomes[i].Props[j][b].GenerateType == 0)
                    {
                        packet.Write(Biomes[i].Props[j][b].PerChunkMin);
                        packet.Write(Biomes[i].Props[j][b].PerChunkMax);
                    }
                    else if (Biomes[i].Props[j][b].GenerateType == 1)
                    {
                        packet.Write(Biomes[i].Props[j][b].Chance);
                    }
                    else if (Biomes[i].Props[j][b].GenerateType == 2)
                    {
                        packet.Write(Biomes[i].Props[j][b].Perlin);

                        packet.Write(Biomes[i].Props[j][b].PerlinMap.Scale);
                        packet.Write(Biomes[i].Props[j][b].PerlinMap.HeightMultiplier);
                        packet.Write(Biomes[i].Props[j][b].PerlinMap.Octaves);

                        packet.Write(Biomes[i].Props[j][b].PerlinMap.Persistence);
                        packet.Write(Biomes[i].Props[j][b].PerlinMap.Lacunarity);
                    }

                    packet.Write(Biomes[i].Props[j][b].UseBounds);
                    packet.Write(Biomes[i].Props[j][b].BoundsX);
                    packet.Write(Biomes[i].Props[j][b].BoundsY);
                    packet.Write(Biomes[i].Props[j][b].BoundsZ);
                }
            }

            packet.Write((byte)Biomes[i].Heightmaps.Count);
            for (byte j = 0; j < Biomes[i].Heightmaps.Count; j++)
            {
                packet.Write(Biomes[i].Heightmaps[j].Scale);
                packet.Write(Biomes[i].Heightmaps[j].HeightMultiplier);
                packet.Write(Biomes[i].Heightmaps[j].Octaves);

                packet.Write(Biomes[i].Heightmaps[j].Persistence);
                packet.Write(Biomes[i].Heightmaps[j].Lacunarity);

                packet.Write(Biomes[i].Heightmaps[j].UseChance);
                packet.Write(Biomes[i].Heightmaps[j].Chance);
            }
        }

        packet.Write((byte)BaseHeightmaps.Count);
        for (byte j = 0; j < BaseHeightmaps.Count; j++)
        {
            packet.Write(BaseHeightmaps[j].Scale);
            packet.Write(BaseHeightmaps[j].HeightMultiplier);
            packet.Write(BaseHeightmaps[j].Octaves);

            packet.Write(BaseHeightmaps[j].Persistence);
            packet.Write(BaseHeightmaps[j].Lacunarity);
        }

        packet.Write(BiomeHeightmap.Scale);
        packet.Write(BiomeHeightmap.HeightMultiplier);
        packet.Write(BiomeHeightmap.Octaves);

        packet.Write(BiomeHeightmap.Persistence);
        packet.Write(BiomeHeightmap.Lacunarity);

        packet.Write(BiomeHeightmap.UseChance);
        packet.Write(BiomeHeightmap.Chance);
    }

    #endregion

    #region Nested Classes

    public class HeightmapData
    {
        public short Scale = 50;
        public short HeightMultiplier = 10;
        public byte Octaves = 7;

        public byte Persistence = 6; // Decimal Number 1dp
        public byte Lacunarity = 144; // Decimal Number 2dp

        public bool UseChance;
        public byte Chance;
    }

    public class PropData
    {
        public short RotateX;
        public short RotateY;
        public short RotateZ;

        public byte MaxScale; // Decimal Number 1dp
        public byte MinScale; // Decimal Number 2dp

        public byte GenerateType;

        public byte PerChunkMin;
        public byte PerChunkMax;

        public byte Chance;

        public byte Perlin;
        public HeightmapData PerlinMap;

        public bool UseBounds;
        public short BoundsX;
        public short BoundsY;
        public short BoundsZ;
    }

    public class BiomeData
    {
        public List<List<PropData>> Props = new List<List<PropData>>();
        public List<HeightmapData> Heightmaps = new List<HeightmapData>();
    }

    #endregion
}