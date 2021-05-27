using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();
    public GameObject playerPrefab;
	public GameObject externalPlayerPrefab;

	[HideInInspector]
	public Dictionary<Vector2, LobbyChunk> terrainChunkDictionary = new Dictionary<Vector2, LobbyChunk>();
	[HideInInspector]
	public List<LobbyChunk> visibleTerrainChunks = new List<LobbyChunk>();

	public delegate void ChunksAdded();
	public ChunksAdded ChunksAddedCallback;

	[Header("Scriptable Objects")]
	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;

	[Header("Misc")]
	public Material mapMaterial;

	[HideInInspector]
	public float meshWorldSize;
	[HideInInspector]
	public int chunksVisibleInViewDst;
	[HideInInspector]
	public int ViewDst;

	Vector2 viewerPosition;
	Vector2 viewerPositionOld;

	bool generated;

	void Awake()
	{
		instance = this;
		heightMapSettings.noiseSettings.seed = 0;
		ViewDst = 70;
	}

	void Start()
	{
		meshWorldSize = meshSettings.meshWorldSize;
		chunksVisibleInViewDst = Mathf.RoundToInt(ViewDst / meshWorldSize);
	}

	void FixedUpdate()
	{
		if (GameManager.ActivePlayer != null)
		{
			viewerPosition = new Vector2(GameManager.ActivePlayer.transform.position.x, GameManager.ActivePlayer.transform.position.z);

			if (viewerPosition != viewerPositionOld)
			{
				viewerPositionOld = viewerPosition;
				UpdateVisibleChunks();
				generated = true;
			}
			else if (!generated)
            {
				UpdateVisibleChunks();
				generated = true;
			}
		}
	}

	public void UpdateVisibleChunks()
	{
		HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();

		for (int i = 0; i < visibleTerrainChunks.Count; i++)
		{
			alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
			visibleTerrainChunks[i].UpdateTerrainChunk();
		}

		int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
		int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
		{
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
			{
				Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
				if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
				{
					if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
					{
						terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
					}
					else
					{
						LobbyChunk newChunk = new LobbyChunk(viewedChunkCoord, heightMapSettings, meshSettings, ViewDst, transform, mapMaterial);
						terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
						newChunk.Load();
					}
				}

			}
		}
	}

	public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation, int skin)
    {
        GameObject _player;

        if (_id == LobbyClient.instance.myId)
        {
            _player = Instantiate(playerPrefab, _position, _rotation);
            _player.transform.name = "Player";

			GameManager.ActivePlayer = _player;
			GameManager.ActivePlayerManager = _player.GetComponent<PlayerManager>();
			GameManager.ActivePlayerManager.player = true;
			GameManager.ActiveCamera = _player.GetComponentInChildren<Camera>();
        }
        else 
		{ 
			_player = Instantiate(externalPlayerPrefab, _position, _rotation);
		}

        PlayerManager _pm = _player.GetComponent<PlayerManager>();
		if (_id != LobbyClient.instance.myId) { _pm.SetSkin(skin); }

        _pm.id = _id;
        _pm.username = _username;
        players.Add(_id, _pm);
    }
}
