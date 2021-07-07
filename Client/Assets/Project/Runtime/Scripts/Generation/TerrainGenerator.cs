using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class TerrainGenerator : MonoBehaviour
{
	public static TerrainGenerator instance;

	public static Dictionary<Vector2, TerrainChunk> ChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	public static List<TerrainChunk> VisibleChunks = new List<TerrainChunk>();

	public GenerationSettings GenerationSettings;
	public LobbyChunkSettings LobbyChunks;

	[Space]

	public LODInfo[] LODDst;

	[Space]

	public Material mapMaterial;

	[HideInInspector]
	public float meshWorldSize;
	[HideInInspector]
	public int chunksVisibleInViewDst;
	[HideInInspector]
	public int ViewDst;

	Vector2 viewerPos;
	Vector2 viewerPosOld;

	bool isNull;
	List<Vector2> requestedChunks = new List<Vector2>();

	GameObject ChunkHolder;
	GameObject PoolHolder;

	void Awake()
    {
		instance = this;
		ViewDst = 500;

		meshWorldSize = GenerationSettings.MeshSetting.meshWorldSize;
		chunksVisibleInViewDst = Mathf.RoundToInt(ViewDst / meshWorldSize);

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

			PropsGeneration.Init(PoolHolder, GenerationSettings.PropSettings);
		}
	}

    void FixedUpdate()
	{
		if (GameManager.ActivePlayer != null)
		{
			if (!isNull) UpdateVisibleChunks();
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

		int currentChunkCoordX = Mathf.RoundToInt(viewerPos.x / meshWorldSize);
		int currentChunkCoordY = Mathf.RoundToInt(viewerPos.y / meshWorldSize);

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
		{
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
			{
				Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
				if (!updatedChunkCoords.Contains(viewedChunkCoord))
				{
					if (ChunkDictionary.ContainsKey(viewedChunkCoord))
					{
						ChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
					}
					else
					{
						TerrainChunk chunk = null;
						if (DataManager.GameState == GameStateEnum.Lobby)
                        {
							chunk = new TerrainChunk(viewedChunkCoord, LODDst, GenerationSettings, ChunkHolder.transform, mapMaterial);
							ChunkDictionary.Add(viewedChunkCoord, chunk);
							chunk.VisibilityChanged += OnTerrainChunkVisibilityChanged;
						}
						else if (DataManager.GameState == GameStateEnum.Singleplayer)
                        {
							if (DataManager.SaveFile.FindChunk(viewedChunkCoord) != -1)
							{
								chunk = new TerrainChunk(viewedChunkCoord, LODDst, GenerationSettings, ChunkHolder.transform, mapMaterial);
								ChunkDictionary.Add(viewedChunkCoord, chunk);
								chunk.VisibilityChanged += OnTerrainChunkVisibilityChanged;
							}
							else
                            {
								if (!requestedChunks.Contains(viewedChunkCoord))
								{
									//ClientSend.RequestChunk(viewedChunkCoord);
									requestedChunks.Add(viewedChunkCoord);
								}
                            }
                        }
						updatedChunkCoords.Add(viewedChunkCoord);
					}
				}

			}
		}
	}

	public static void AddChunk(Packet packet)
    {
		if (ChunkDictionary.ContainsKey(packet.ReadVector2(false))) { return; }
		int VertsPerLine = instance.GenerationSettings.MeshSetting.numVertsPerLine;
		SaveFileClass.ChunkClass chunk = new SaveFileClass.ChunkClass
		{
			Coords = packet.ReadVector2(),
			HeightMap = new float[VertsPerLine, VertsPerLine]
		};

		for (int x = 0; x < VertsPerLine; x++)
		{
			for (int y = 0; y < VertsPerLine; y++)
			{
				chunk.HeightMap[x, y] = packet.ReadFloat();
			}
		}

		int propTypesLength = packet.ReadInt();
		for (int i = 0; i < propTypesLength; i++)
        {
			SaveFileClass.PropTypeClass propType = new SaveFileClass.PropTypeClass();

			int propLength = packet.ReadInt();
			for (int j = 0; j < propLength; j++)
			{
				SaveFileClass.PropClass prop = new SaveFileClass.PropClass
				{
					ID = packet.ReadInt(),
					Pos = packet.ReadVector3(),
					Scale = packet.ReadVector3(),
					Euler = packet.ReadVector3()
				};
				propType.Props.Add(prop);
			}
			chunk.PropTypes.Add(propType);
		}

		for (int j = 0; j < packet.ReadInt(); j++)
		{
			SaveFileClass.StorageClass prop = new SaveFileClass.StorageClass
			{
				ID = packet.ReadString(),
				Pos = packet.ReadVector3(),
				Rot = packet.ReadVector3(),
				Items = packet.ReadStringList()
			};
			chunk.Storage.Add(prop);
		}

		DataManager.SaveFile.Chunks.Add(chunk);
		TerrainChunk chunkObject = new TerrainChunk(chunk.Coords, instance.LODDst, instance.GenerationSettings,
			instance.transform, instance.mapMaterial);

		ChunkDictionary.Add(chunk.Coords, chunkObject);
		chunkObject.VisibilityChanged += instance.OnTerrainChunkVisibilityChanged;
	}

    #endregion
    #region Misc Funcs

	public static Vector2 GetNearestChunk(Vector3 pos)
	{
		return new Vector2(Mathf.RoundToInt(pos.x / instance.meshWorldSize), Mathf.RoundToInt(pos.x / instance.meshWorldSize));
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
		Vector2 chunkLoc = new Vector2(Mathf.RoundToInt(t.x / instance.meshWorldSize), Mathf.RoundToInt(t.x / instance.meshWorldSize));
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