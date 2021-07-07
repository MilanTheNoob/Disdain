using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    public Vector2 Coord;
    public Vector2 SampleCenter;

    public GameObject ChunkObject;
    public GameObject StorageParents;
    public GameObject[] PropParents;

    public Bounds Bounds;

    public MeshRenderer MeshR;
    public MeshFilter MeshF;

    public LODMesh LOD;

    public event System.Action<TerrainChunk, bool> VisibilityChanged;
    public SaveFileClass.ChunkClass ChunkData;

    public TerrainChunk(Vector2 coord, SaveFileClass.ChunkClass chunkData, GenerationSettings gs, Transform parent, Material material)
    {
        this.Coord = coord;

        SampleCenter = Coord * gs.MeshSetting.meshWorldSize / gs.MeshSetting.meshScale;
        Vector3 position = coord * gs.MeshSetting.meshWorldSize;
        Bounds = new Bounds(position, Vector2.one * gs.MeshSetting.meshWorldSize);

        ChunkObject = new GameObject("Terrain Chunk " + position.x + "-" + position.y);
        MeshR = ChunkObject.AddComponent<MeshRenderer>();
        MeshF = ChunkObject.AddComponent<MeshFilter>();

        MeshR.material = material;

        ChunkObject.transform.position = new Vector3(position.x, 0, position.y);
        ChunkObject.transform.parent = parent;

        ChunkObject.layer = 8;
        ChunkObject.tag = "TerrainChunk";

        //BuildingManager.instance.LoadData();
        SetVisible(false);

        #region Mesh Setup

        ChunkData = chunkData;
        LOD = new LODMesh(0, ChunkData.HeightMap, gs.MeshSetting);

        #endregion
        #region Init Prop Parents

        List<string> propTypes = null;
        PropParents = new GameObject[propTypes.Count];

        for (int i = 0; i < propTypes.Count; i++)
        {
            PropParents[i] = new GameObject(propTypes[i]);

            PropParents[i].transform.parent = ChunkObject.transform;
            PropParents[i].transform.localPosition = Vector3.zero;
        }

        StorageParents = new GameObject("Storage");
        StorageParents.transform.parent = ChunkObject.transform;
        StorageParents.transform.localPosition = Vector3.zero;

        #endregion
    }

    public void UpdateTerrainChunk()
    {
        if (!LOD.generated) return;
        MeshF.mesh = LOD.mesh;
    }

    //public void RemoveChunk() { PropsGeneration.instance.RemoveFromChunk(this); Object.Destroy(ChunkObject); }
    public void SetVisible(bool visible) { ChunkObject.SetActive(visible); }
    public bool IsVisible() { if (ChunkObject != null) { return ChunkObject.activeSelf; } else { return false; } }
}

public class LODMesh
{
    public Mesh mesh;
    public event System.Action updateCallback;

    public bool generated;

    public LODMesh(int lod, float[,] heightMap, MeshSettings meshSettings)
    {
        ThreadedDataRequester.RequestData(() => Generation.GenerateTerrainMesh(heightMap, meshSettings, lod), OnMeshDataRecieved);
    }

    public void OnMeshDataRecieved(object meshDataObject)
    {
        mesh = ((MeshData)meshDataObject).CreateMesh();
        generated = true;

        updateCallback();
    }
}