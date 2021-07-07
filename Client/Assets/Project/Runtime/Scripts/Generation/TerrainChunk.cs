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
    public MeshCollider MeshC;

    public bool Visible;
    public bool PropsGenerated;

    public int LodIndex;
    public int LodIndexOld = -1;

    public LODInfo[] LodInfo;
    public LODMesh[] LODS;

    public event System.Action<TerrainChunk, bool> VisibilityChanged;
    public SaveFileClass.ChunkClass ChunkData;

    public TerrainChunk(Vector2 coord, LODInfo[] lodInfo, GenerationSettings gs, Transform parent, Material material)
    {
        this.Coord = coord;
        this.LodInfo = lodInfo;

        SampleCenter = Coord * gs.MeshSetting.meshWorldSize / gs.MeshSetting.meshScale;
        Vector3 position = coord * gs.MeshSetting.meshWorldSize;
        Bounds = new Bounds(position, Vector2.one * gs.MeshSetting.meshWorldSize);

        ChunkObject = new GameObject("Terrain Chunk " + position.x + "-" + position.y);
        MeshR = ChunkObject.AddComponent<MeshRenderer>();
        MeshF = ChunkObject.AddComponent<MeshFilter>();
        MeshC = ChunkObject.AddComponent<MeshCollider>();

        MeshR.material = material;

        ChunkObject.transform.position = new Vector3(position.x, 0, position.y);
        ChunkObject.transform.parent = parent;

        ChunkObject.layer = 8;
        ChunkObject.tag = "TerrainChunk";

        //BuildingManager.instance.LoadData();
        SetVisible(false);

        #region Mesh Setup

        if (DataManager.GameState == GameStateEnum.Lobby)
        {
            ChunkData = new SaveFileClass.ChunkClass();
            ChunkData.HeightMap = TerrainGenerator.instance.LobbyChunks.GetChunk(coord);
        }
        else if (DataManager.GameState == GameStateEnum.Singleplayer)
        {
            ChunkData = DataManager.SaveFile.ReturnChunk(coord);
        }

        LODS = new LODMesh[lodInfo.Length];
        for (int i = 0; i < lodInfo.Length; i++) 
        { 
            LODS[i] = new LODMesh(lodInfo[i].LOD, ChunkData.HeightMap, gs.MeshSetting);
            LODS[i].updateCallback += UpdateTerrainChunk;
        }

        #endregion
        #region Init Prop Parents

        List<string> propTypes = DataManager.SaveFile.PropTypes;
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
        for (int i = 0; i < LODS.Length; i++)
        {
            if (!LODS[i].generated) { return; }
        }

        float viewerDst = Mathf.Sqrt(Bounds.SqrDistance(viewerPosition()));

        bool wasVisible = IsVisible();
        bool visible = viewerDst <= LodInfo[LodInfo.Length - 1].ViewDst;

        LodIndex = 0;
        if (MeshC.sharedMesh == null) MeshC.sharedMesh = LODS[0].mesh;

        if (visible)
        {
            for (int i = 0; i < LodInfo.Length; i++)
            {
                if (viewerDst > LodInfo[i].ViewDst)
                {
                    LodIndex = i + 1;
                }
                else
                {
                    break;
                }
            }

            if (LodIndex != LodIndexOld)
            {
                LodIndexOld = LodIndex;
                MeshF.mesh = LODS[LodIndex].mesh;
            }
        }

        if (wasVisible != visible)
        {
            SetVisible(visible);
            VisibilityChanged?.Invoke(this, visible);
            if (!visible)
            {
                Generation.SaveChunkData(this);
                PropsGeneration.Clear(this);
            }
            else
            {
                PropsGeneration.Generate(this);
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

public class LODMesh
{
    public Mesh mesh;
    public event System.Action updateCallback;

    public bool generated;

    public LODMesh(int lod, float[,] heightMap, MeshSettingsStruct meshSettings)
    {
        ThreadManager.RequestData(() => Generation.GenerateTerrainMesh(heightMap, meshSettings, lod), OnMeshDataRecieved);
    }

    public void OnMeshDataRecieved(object meshDataObject)
    {
        mesh = ((MeshData)meshDataObject).CreateMesh();
        generated = true;

        updateCallback();
    }
}