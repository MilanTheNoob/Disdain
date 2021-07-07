using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveFileClass
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

    [Serializable]
    public class InventorySlotClass
    {
        public string Item;
        public int Quantity;
    }
    [Serializable]
    public class ChunkClass
    {
        public List<PropTypeClass> PropTypes = new List<PropTypeClass>();
        public List<StorageClass> Storage = new List<StorageClass>();

        public float[,] HeightMap;
        public Vector2 Coords;
    }
    [Serializable]
    public class PropTypeClass
    {
        public List<PropClass> Props = new List<PropClass>();
    }
    [Serializable]
    public class PropClass
    {
        public int ID;

        public Vector3 Pos;
        public Vector3 Scale;
        public Vector3 Euler;
    }
    [Serializable]
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
            if (Chunks[i].Coords == coord) { return Chunks[i]; }
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
            if (Chunks[i].Coords == coord)
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