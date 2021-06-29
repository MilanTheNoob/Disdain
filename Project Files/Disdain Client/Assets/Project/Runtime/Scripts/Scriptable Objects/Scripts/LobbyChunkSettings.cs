using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "New Lobby Chunk Settings")]
public class LobbyChunkSettings : ScriptableObject
{
    public List<LobbyChunk> Chunks = new List<LobbyChunk>();
    public int VertsPerLine;

    public float[,] GetChunk(Vector2 coord)
    {
        for (int i = 0; i < Chunks.Count; i++)
        {
            if (Chunks[i].Coord == coord)
            {
                float[,] hm = new float[VertsPerLine, VertsPerLine];
                int count = 0;

                for (int x = 0; x < VertsPerLine; x++)
                {
                    for (int y = 0; y < VertsPerLine; y++)
                    {
                        hm[x, y] = Chunks[i].Heightmap[count];
                        count++;
                    }
                }

                return hm;
            }
        }

        return null;
    }

    [Serializable]
    public class LobbyChunk
    {
        public Vector2 Coord;
        public float[] Heightmap;
    }
}
