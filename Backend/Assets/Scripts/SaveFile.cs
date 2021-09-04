using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveFile
{
    public int Seed = UnityEngine.Random.Range(0, 999999999);
    public Vector3 PlayerLocation;

    public int MapSize;
    public int VertsPerLine;

    public List<string> Inventory = new List<string>();
    public List<float> Vitals = new List<float>();

    public List<ChunkClass> Chunks = new List<ChunkClass>();
    public List<string> PropTypes = new List<string>();

    #region Nested Classes

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

    #endregion
    #region Funcs

    /// <summary>
    /// Finds what chunk the given location falls in
    /// NOTE - Does not mean the chunk location exists/been generated yet!
    /// </summary>
    /// <param name="pos">The global position</param>
    /// <returns>The chunk coords</returns>
    public Vector2 GetChunk(Vector3 pos)
    {
        float meshSize = 64; // TerrainGenerator.instance.GenerationSettings.MeshSetting.meshWorldSize;
        return new Vector2(Mathf.RoundToInt(pos.x / meshSize), Mathf.RoundToInt(pos.x / meshSize));
    }

    /// <summary>
    /// Returns chunk data based off inputted coords
    /// </summary>
    /// <param name="coord">The chunk's coordinates</param>
    /// <returns>The chunk data</returns>
    public ChunkClass ReturnChunk(Vector2 coord)
    {
        for (int i = 0; i < Chunks.Count; i++)
        {
            if (Chunks[i].Coord == coord) { return Chunks[i]; }
        }

        return null;
    }

    /// <summary>
    /// Finds a chunk based off given coords
    /// </summary>
    /// <param name="coord">The coords of the chunk (e.g. {0, 1}, {2, 5}, etc</param>
    /// <returns>The index of the chunk in the Save File</returns>
    public int FindChunk(Vector2 coord)
    {
        for (int i = 0; i < Chunks.Count; i++)
        {
            if (Chunks[i].Coord == coord)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Finds the respective chunk based off a given location
    /// </summary>
    /// <param name="pos">The global position</param>
    /// <returns>The index of the chunk in the Save File</returns>
    public int FindChunk(Vector3 pos) { return FindChunk(GetChunk(pos)); }

    /// <summary>
    /// Finds storage from the save file at specified location
    /// </summary>
    /// <param name="coord">The global position of the storage</param>
    /// <returns>Returns the index of the storage in its respective chunk</returns>
    public int FindStorage(Vector3 coord)
    {
        int chunk = FindChunk(GetChunk(coord));

        for (int i = 0; i < Chunks[chunk].Storage.Count; i++)
        {
            if (Chunks[chunk].Storage[i].Pos == coord)
            {
                return i;
            }
        }

        return -1;
    }

    #endregion
}