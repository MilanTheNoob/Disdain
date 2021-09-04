using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    public Vector2 Coord;
    public Vector2 SampleCenter;

    public GameObject ChunkObject;
    public GameObject StorageParents;
    public GameObject[] PropParents;

    public MeshRenderer MeshR;
    public MeshFilter MeshF;
    public MeshCollider MeshC;

    public bool Visible;
    public bool PropsGenerated;

    public event System.Action<TerrainChunk, bool> VisibilityChanged;
    public ChunkClass ChunkData;

    public Bounds Bounds;

    public TerrainChunk(Vector2 coord, ChunkClass chunkData, Transform parent, Material material)
    {
        Coord = coord;
        SampleCenter = Coord * 64;

        ChunkObject = new GameObject("Terrain Chunk " + coord.x + "-" + coord.y);
        MeshR = ChunkObject.AddComponent<MeshRenderer>();
        MeshF = ChunkObject.AddComponent<MeshFilter>();
        MeshC = ChunkObject.AddComponent<MeshCollider>();

        MeshR.material = material;
        Bounds = new Bounds(SampleCenter, Vector3.one * 64);

        ChunkObject.transform.position = new Vector3(coord.x, 0, coord.y);
        ChunkObject.transform.parent = parent;

        ChunkObject.layer = 8;
        ChunkObject.tag = "TerrainChunk";

        //BuildingManager.instance.LoadData();
        SetVisible(false);

        MeshF.mesh = Generation.GenerateTerrainMesh(ChunkData.HeightMap);
    }

    public void UpdateTerrainChunk()
    {
        float viewerDst = Mathf.Sqrt(Bounds.SqrDistance(viewerPosition()));

        bool wasVisible = IsVisible();
        bool visible = viewerDst <= 30;

        if (wasVisible != visible)
        {
            SetVisible(visible);
            VisibilityChanged?.Invoke(this, visible);
            if (!visible)
            {
                //Generation.SaveChunkData(this);
                //PropsGeneration.Clear(this);
            }
            else
            {
                //PropsGeneration.Generate(this);
            }
        }
    }

    Vector2 viewerPosition()
    {
        if (GameManager.ActivePlayer == null) { return Vector2.zero; }
        return new Vector2(GameManager.ActivePlayer.transform.position.x, GameManager.ActivePlayer.transform.position.z);
    }

    //public void RemoveChunk() { PropsGeneration.instance.RemoveFromChunk(this); Object.Destroy(ChunkObject); }
    public void SetVisible(bool visible) { ChunkObject.SetActive(visible); }
    public bool IsVisible() { if (ChunkObject != null) { return ChunkObject.activeSelf; } else { return false; } }
}