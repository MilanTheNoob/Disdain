using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    public Vector2 SampleCenter;

    public GameObject ChunkObject;
    public GameObject StorageParents;
    public GameObject[] PropParents;

    public MeshRenderer MeshR;
    public MeshFilter MeshF;

    public TerrainChunk(SaveFile.ChunkClass chunkData, GenerationSettings gs, Transform parent, Material material)
    {
        SampleCenter = chunkData.Coord * gs.MeshSetting.meshWorldSize / gs.MeshSetting.meshScale;
        Vector3 position = chunkData.Coord * gs.MeshSetting.meshWorldSize;

        ChunkObject = new GameObject("Terrain Chunk " + position.x + "-" + position.y);
        MeshR = ChunkObject.AddComponent<MeshRenderer>();
        MeshF = ChunkObject.AddComponent<MeshFilter>();

        MeshR.material = material;

        ChunkObject.transform.position = new Vector3(position.x, 0, position.y);
        ChunkObject.transform.parent = parent;

        MeshF.mesh = Generation.GenerateTerrainMesh(chunkData.HeightMap, gs.MeshSetting, 0);
    }
}