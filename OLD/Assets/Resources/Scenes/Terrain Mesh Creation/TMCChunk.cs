using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TMCChunk : MonoBehaviour
{
    public GameObject go;
    Material mat;
    Vector2 coord;
    GenerationSettings gs;
    LobbyChunkSettings lc;
    GameObject parent;

    public TMCChunk(Vector2 coord, GenerationSettings gs, Material mat, GameObject parent, LobbyChunkSettings lc)
    {
        this.coord = coord;
        this.mat = mat;
        this.gs = gs;
        this.parent = parent;
        this.lc = lc;
        //ThreadedDataRequester.RequestData(() => Generation.GenerateHeightMap(gs, coord * gs.MeshSetting.meshWorldSize / gs.MeshSetting.meshScale), OnHeightMapRecieved);
    }

    void OnHeightMapRecieved(object heightMapObject)
    {
        float[,] hm = (float[,])heightMapObject;
        float[] newHM = new float[gs.MeshSetting.numVertsPerLine * gs.MeshSetting.numVertsPerLine];
        int count = 0;
        for (int x = 0; x < gs.MeshSetting.numVertsPerLine; x++)
        {
            for (int y = 0; y < gs.MeshSetting.numVertsPerLine; y++)
            {
                newHM[count] = hm[x, y];
                count++;
            }
        }

        lc.Chunks.Add(new LobbyChunkSettings.LobbyChunk
        {
            Coord = coord,
            Heightmap = newHM
        });
        /*
        ThreadManager.RequestData(() => Generation.GenerateTerrainMesh((float[,])heightMapObject, gs.MeshSetting
            , 0), OnMeshDataRecieved);
        */
    }

    public void OnMeshDataRecieved(object meshDataObject)
    {
        AssetDatabase.CreateAsset(((MeshData)meshDataObject).CreateMesh(), "Assets/Prefabs/Terrain Meshes/" + coord + ".asset");
        AssetDatabase.SaveAssets();

        go = new GameObject(coord.x + ", " + coord.y);
        go.transform.position = new Vector3(coord.x * gs.MeshSetting.meshWorldSize, 0, coord.y * gs.MeshSetting.meshWorldSize);
        go.AddComponent<MeshFilter>().mesh = (Mesh)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Terrain Meshes/" + coord + ".asset", typeof(Mesh));
        go.AddComponent<MeshRenderer>().material = mat;
        go.transform.parent = parent.transform;
    }
}
