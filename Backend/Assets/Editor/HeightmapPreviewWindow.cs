using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class HeightmapPreviewWindow : EditorWindow
{
    GenerationSettings gs;
    int mapSize;

    [MenuItem("Custom Windows/Lobby Generation")]
    public static void ShowWindow()
    {
        GetWindow<LobbyGenerationWindow>("Heightmap Preview");
    }

    private void OnGUI()
    {
        gs = EditorGUILayout.ObjectField(x

        mapSize = EditorGUILayout.IntField("Preview Size", mapSize);
        if (GUILayout.Button("Preview"))
        {
            mapSize = Mathf.Clamp(mapSize, 1, 100);

            if (mapSize == 1)
            {
                SaveFileClass.ChunkClass chunkData = new SaveFileClass.ChunkClass
                {
                    Coords = Vector3.zero,
                    HeightMap
                };
                Terrain chunk = new TerrainChunk(Vector3.zero, chunkData, )
            }
        }
    }
}
