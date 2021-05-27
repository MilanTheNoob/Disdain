using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public class MultiplayerGameManager : MonoBehaviour
{
    #region Singleton

    public static MultiplayerGameManager instance;

    private void Awake()
    {
        instance = this;
    }

    #endregion

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject playerPrefab;

    void Start()
    {
        players.Clear();
    }

    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
    {
        GameObject _player;

        /*if (_id == LobbyClient.instance.myId)
        {
            _player = Instantiate(playerPrefab, _position, _rotation);
            _player.transform.name = "Player";

            //GameManager.player = _player;
            //MultiplayerTerrainGenerator.instance.viewer = _player.transform;

            Destroy(_player.GetComponentInChildren<Camera>().gameObject);
        }
        else 
        { */
            _player = Instantiate(playerPrefab, _position, _rotation); 
        //}

        PlayerManager _pm = _player.GetComponent<PlayerManager>();

        _pm.id = _id;
        _pm.username = _username;
        players.Add(_id, _pm);
    }
}
