using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    public Vector2 SampleCenter;

    public GameObject ChunkObject;
    public GameObject StorageParents;
    public GameObject PropParents;

    public MeshRenderer MeshR;
    public MeshFilter MeshF;

    public TerrainChunk(SaveFile.ChunkClass chunkData, Transform parent, 
        Material material, PropDataObject[] objectDict)
    {
        SampleCenter = chunkData.Coord * 64;

        ChunkObject = new GameObject("Terrain Chunk " + SampleCenter.x + "-" + SampleCenter.y);
        MeshR = ChunkObject.AddComponent<MeshRenderer>();
        MeshF = ChunkObject.AddComponent<MeshFilter>();

        MeshR.material = material;

        ChunkObject.transform.parent = parent;
        ChunkObject.transform.localPosition = new Vector3(SampleCenter.x, 0, SampleCenter.y);

        PropParents = new GameObject("Props Parent");
        PropParents.transform.parent = ChunkObject.transform;

        MeshF.mesh = Generation.GenerateTerrainMesh(chunkData.HeightMap);
        Generation.GenerateProps(chunkData.Props, objectDict, PropParents);
    }
}