using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapPreview : MonoBehaviour
{
    #region Singleton
    public static MapPreview instance;
    void Awake() { instance = this; }
    #endregion

    public GameObject PreviewObject;
    public Material PreviewMaterial;

    List<TerrainChunk> Chunks = new List<TerrainChunk>();
    public PropDataObject[] PropsData;

    #region Generation

    public void StopPreviewing()
    {
        for (int i = 0; i < Chunks.Count; i++) { DestroyImmediate(Chunks[i].ChunkObject); }
        Chunks.Clear();
    }

    public void PreviewChunks(SaveFile.ChunkClass[] chunkDatas)
    {
        for (int i = 0; i < Chunks.Count; i++) { DestroyImmediate(Chunks[i].ChunkObject); }
        Chunks.Clear();

        for (int i = 0; i < chunkDatas.Length; i++)
            Chunks.Add(new TerrainChunk(chunkDatas[i], PreviewObject.transform, PreviewMaterial, PropsData));
    }

    #endregion
}
