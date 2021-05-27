using UnityEngine;

public class LobbyChunk
{
    public event System.Action<LobbyChunk, bool> onVisibilityChanged;
    public Vector2 coord;

    public GameObject meshObject;
    public Vector2 sampleCentre;
    public Bounds bounds;

    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public MeshCollider meshCollider;

    public bool isVisible;

    public HeightMap heightData;
    bool heightMapRecieved;
    float maxViewDst;
    public bool isGenerated;

    HeightMapSettings heightMapSettings;
    MeshSettings meshSettings;

    Transform viewer;

    LODMesh meshStruct;

    public LobbyChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, float maxViewDst, Transform parent, Material material)
    {
        this.coord = coord;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.viewer = GameManager.ActivePlayer.transform;
        this.maxViewDst = maxViewDst;

        sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
        Vector3 position = coord * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

        meshObject = new GameObject("Terrain Chunk " + position.x + "-" + position.y);
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();

        meshRenderer.material = material;

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;

        meshObject.layer = 8;
        meshObject.tag = "TerrainChunk";

        SetVisible(false);

        meshStruct = new LODMesh(0);
        meshStruct.updateCallback += SetChunkMesh;
    }

    public void Load()
    {
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, sampleCentre), OnHeightMapRecieved);
    }

    void OnHeightMapRecieved(object heightMapObject)
    {
        this.heightData = (HeightMap)heightMapObject;
        heightMapRecieved = true;

        UpdateTerrainChunk();
    }

    public void UpdateTerrainChunk()
    {
        if (heightMapRecieved)
        {
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

            bool wasVisible = IsVisible();
            bool visible = viewerDistanceFromNearestEdge <= maxViewDst;

            if (visible)
            {
                if (!meshStruct.hasRequestedMesh)
                {
                    meshStruct.RequestMesh(heightData, meshSettings);
                }
            }

            if (wasVisible != visible)
            {
                SetVisible(visible);
                onVisibilityChanged?.Invoke(this, visible);
            }
        }
    }

    public void SetChunkMesh()
    {
        meshFilter.mesh = meshStruct.mesh;
        meshCollider.sharedMesh = meshFilter.mesh;

        meshStruct.setMesh = true;
    }

    Vector2 viewerPosition { get { return new Vector2(viewer.position.x, viewer.position.z); } }
    public void UpdateViewDst(float _ViewDst) { maxViewDst = _ViewDst; }
    public void SetVisible(bool visible) { meshObject.SetActive(visible); }
    public bool IsVisible() { if (meshObject != null) { return meshObject.activeSelf; } else { return false; } }
}