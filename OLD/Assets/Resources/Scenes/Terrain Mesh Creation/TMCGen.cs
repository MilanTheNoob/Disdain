using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TMCGen : MonoBehaviour
{
    public GenerationSettings generationSettings;
    public LobbyChunkSettings lc;
    public Material mat;

    void Start()
    {
        int ctg = Mathf.RoundToInt(1000 / generationSettings.MeshSetting.meshWorldSize);
        List<Vector2> alreadyUpdatedChunkCoords = new List<Vector2>();
        lc.Chunks.Clear();
        lc.VertsPerLine = generationSettings.MeshSetting.numVertsPerLine;

		for (int yOffset = -ctg; yOffset <= ctg; yOffset++)
		{
			for (int xOffset = -ctg; xOffset <= ctg; xOffset++)
			{
				Vector2 viewedChunkCoord = new Vector2(xOffset, yOffset);
				if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
				{
                    alreadyUpdatedChunkCoords.Add(viewedChunkCoord);
                    new TMCChunk(viewedChunkCoord, generationSettings, mat, gameObject, lc);
				}

			}
		}
        print(alreadyUpdatedChunkCoords.Count);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            EditorUtility.SetDirty(lc);
            AssetDatabase.SaveAssets();

            //PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, "Assets/Prefabs/Lobby Terrain.prefab", InteractionMode.AutomatedAction);
        }
    }
}
