using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class TerrainGenerator : MonoBehaviour
{
	public static TerrainGenerator instance;

	public static Dictionary<Vector2, TerrainChunk> ChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	public static List<TerrainChunk> VisibleChunks = new List<TerrainChunk>();

	[Space]

	public Material mapMaterial;

	[HideInInspector]
	public int chunksVisibleInViewDst = 5;

	Vector2 viewerPos;
	Vector2 viewerPosOld;

	GameObject ChunkHolder;
	GameObject PoolHolder;

	void Awake()
    {
		instance = this;

		ChunkDictionary.Clear();
		VisibleChunks.Clear();

		ChunkHolder = new GameObject("Chunks");
		ChunkHolder.transform.parent = transform;
	}

    void Start()
    {
		if (DataManager.GameState == GameStateEnum.Singleplayer ||
			DataManager.GameState == GameStateEnum.Multiplayer)
		{
			PoolHolder = new GameObject("Pools");
			PoolHolder.transform.parent = transform;

			//PropsGeneration.Init(PoolHolder, GenerationSettings.PropSettings);
		}
	}

    void FixedUpdate()
	{
		if (GameManager.ActivePlayer != null)
		{
			UpdateVisibleChunks();
			viewerPos = new Vector2(GameManager.ActivePlayer.transform.position.x, GameManager.ActivePlayer.transform.position.z);

			if (viewerPos != viewerPosOld)
			{
				viewerPosOld = viewerPos;
				UpdateVisibleChunks();
			}
		}
	}

	#region Generating

	public void UpdateVisibleChunks()
	{
		List<Vector2> updatedChunkCoords = new List<Vector2>();

		for (int i = 0; i < VisibleChunks.Count; i++)
		{
			updatedChunkCoords.Add(VisibleChunks[i].Coord);
			VisibleChunks[i].UpdateTerrainChunk();
		}

		int currentChunkCoordX = Mathf.RoundToInt(viewerPos.x / 64);
		int currentChunkCoordY = Mathf.RoundToInt(viewerPos.y / 64);

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
		{
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
			{
				Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
				if (!updatedChunkCoords.Contains(viewedChunkCoord))
				{
					if (ChunkDictionary.ContainsKey(viewedChunkCoord)) ChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
				}
			}
		}
	}

	public static void AddChunk(Packet packet, Vector2 coord)
    {
		if (ChunkDictionary.ContainsKey(coord)) { return; }

		ChunkClass chunkData = new ChunkClass();
		chunkData.Deserialize(packet, coord);

		ChunkDictionary.Add(chunkData.Coord, new TerrainChunk(chunkData.Coord, chunkData, instance.transform, instance.mapMaterial));
		ChunkDictionary[chunkData.Coord].VisibilityChanged += instance.OnTerrainChunkVisibilityChanged;
	}

    #endregion
    #region Misc Funcs

	public static Vector2 GetNearestChunk(Vector3 pos)
	{
		return new Vector2(Mathf.RoundToInt(pos.x / 64), Mathf.RoundToInt(pos.x / 64));
	}

	public static bool AddToNearestChunk(GameObject obj, ChildTypeEnum childType)
	{
		Vector2 chunkLoc = GetNearestChunk(obj.transform.position);
		if (!ChunkDictionary.ContainsKey(chunkLoc)) { return false; }

		try
		{
			obj.transform.transform.parent = ChunkDictionary[chunkLoc].PropParents[(int)childType].transform;
			return true;
		}
		catch { return false; }
	}

	public static bool CanAddToNearestChunk(Vector3 t)
	{
		Vector2 chunkLoc = new Vector2(Mathf.RoundToInt(t.x / 64), Mathf.RoundToInt(t.x / 64));
		if (ChunkDictionary.ContainsKey(chunkLoc)) { return true; } else { return false; }
	}

	public void ResetChunks()
	{
		//for (int i = 0; i < ChunkDictionary.Count; i++) { ChunkDictionary.ElementAt(i).Value.RemoveChunk(); }

		ChunkDictionary.Clear();
		VisibleChunks.Clear();

		UpdateVisibleChunks();
	}

	void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
	{
		if (isVisible)
		{
			VisibleChunks.Add(chunk);
		}
		else
		{
			VisibleChunks.Remove(chunk);
		}
	}

	#endregion
}

public enum ChildTypeEnum
{
	Foliage = 0,
	Trees,
	Rocks,
	Vehicles,
	Misc,
	Buildings,
	Structures,
	Storage
}

[Serializable]
public struct LODInfo
{
	public int LOD;
	public float ViewDst;
}