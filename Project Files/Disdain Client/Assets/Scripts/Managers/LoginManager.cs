using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [Header("Platforms")]
    public GameObject Mobile;
    public GameObject PC;

    [Header("Mobile")]
    public InputField LoginUsernameM;
    public InputField LoginPasswordM;
    public Toggle LoginRememberM;

    [Space]

    public InputField SignupUsernameM;
    public InputField SignupPasswordM;

    [Space]

    public GameObject LoginSectionM;
    public GameObject SignupSectionM;

    [Space]

    public Text ErrorM;

    [Header("PC")]
    public InputField LoginUsernameP;
    public InputField LoginPasswordP;
    public Toggle LoginRememberP;

    [Space]

    public InputField SignupUsernameP;
    public InputField SignupPasswordP;

    [Space]

    public GameObject LoginSectionP;
    public GameObject SignupSectionP;

    [Space]

    public Text ErrorP;

    [Header("Skins")]
    public GameObject PlayerPreview;
    public List<GameObject> PlayerSkins;

    string ip = "127.0.0.1";
    int port = 26950;

    TcpClient socket;

    private NetworkStream stream;
    private Packet receivedData;
    private byte[] receiveBuffer;

    int x = 0;
    string username;
    string password;
    int skin;

    void Start()
    {
        PlayerPreview.SetActive(false);

        ErrorM.text = "";
        ErrorP.text = "";
    }

    void FixedUpdate()
    {
        PlayerPreview.transform.eulerAngles = new Vector3(0, PlayerPreview.transform.eulerAngles.y + 1f, 0);
        if (PlayerPreview.transform.eulerAngles.y >= 360) { PlayerPreview.transform.eulerAngles = Vector3.zero; }
    }

    public void LeftSkin()
    {
        if (skin > 0)
        {
            skin--;

            for (int i = 0; i < PlayerSkins.Count; i++)
            {
                if (i != skin)
                {
                    PlayerSkins[i].SetActive(false);
                }
                else
                {
                    PlayerSkins[i].SetActive(true);
                }
            }
        }
    }

    public void RightSkin()
    {
        if (skin < 43)
        {
            skin++;

            for (int i = 0; i < PlayerSkins.Count; i++)
            {
                if (i != skin)
                {
                    PlayerSkins[i].SetActive(false);
                }
                else
                {
                    PlayerSkins[i].SetActive(true);
                }
            }
        }
    }

    public void Login()
    {
        socket = new TcpClient
        {
            ReceiveBufferSize = 4096,
            SendBufferSize = 4096
        };

        receiveBuffer = new byte[4096];
        socket.BeginConnect(ip, port, ConnectCallback, socket);

        x = 0;

        if (GameManager.ControlState == GameManager.ControlStateEnum.Mobile)
        {
            username = LoginUsernameM.text;
            password = LoginPasswordM.text;
        }
        else
        {
            username = LoginUsernameP.text;
            password = LoginPasswordP.text;
        }
    }

    public void Signup()
    {
        socket = new TcpClient
        {
            ReceiveBufferSize = 4096,
            SendBufferSize = 4096
        };

        receiveBuffer = new byte[4096];
        socket.BeginConnect(ip, port, ConnectCallback, socket);

        x = 1;
        if (GameManager.ControlState == GameManager.ControlStateEnum.Mobile)
        {
            username = SignupUsernameM.text;
            password = SignupPasswordM.text;
        }
        else
        {
            username = SignupUsernameP.text;
            password = SignupPasswordP.text;
        }
    }

    #region Networking

    private void ConnectCallback(IAsyncResult _result)
    {
        socket.EndConnect(_result);

        if (!socket.Connected)
        {
            return;
        }

        stream = socket.GetStream();
        receivedData = new Packet();
        stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);

        using (Packet packet = new Packet(x))
        {
            packet.Write(username);
            packet.Write(password);
            if (x == 1) { print(skin); packet.Write(skin); }
            packet.WriteLength();

            SendData(packet);
        }
    }

    public void SendData(Packet _packet)
    {
        try
        {
            if (socket != null)
            {
                stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
            }
        }
        catch (Exception _ex)
        {
            Debug.Log($"Error sending data to server via TCP: {_ex}");
        }
    }

    private void ReceiveCallback(IAsyncResult _result)
    {
        try
        {
            int _byteLength = stream.EndRead(_result);
            if (_byteLength <= 0) { return; }
            byte[] _data = new byte[_byteLength];
            Array.Copy(receiveBuffer, _data, _byteLength);
            receivedData.Reset(HandleData(_data));
            stream.BeginRead(receiveBuffer, 0, 4096, ReceiveCallback, null);
        }
        catch { }
    }

    private bool HandleData(byte[] _data)
    {
        int _packetLength = 0;

        receivedData.SetBytes(_data);

        if (receivedData.UnreadLength() >= 4)
        {
            _packetLength = receivedData.ReadInt();
            if (_packetLength <= 0)
            {
                return true;
            }
        }

        while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
        {
            byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_packetBytes))
                {
                    int _packetId = _packet.ReadInt();
                    if (_packet.ReadBool())
                    {
                        SavingManager.ToLobby(_packet.ReadString(), _packet.ReadInt(), username, _packet.ReadInt());
                        if (_packet.ReadBool()) { SavingManager.SaveFile = JsonUtility.FromJson<SavingManager.SaveStruct>(_packet.ReadString()); }
                    }
                    else
                    {
                        if (_packet.ReadInt() == 0)
                        {
                            ErrorM.text = "Incorrect username/password";
                            ErrorP.text = "Incorrect username/password";
                        }
                        else
                        {
                            ErrorM.text = "Username already exists :/";
                            ErrorP.text = "Username already exists :/";
                        }
                    }
                }
            });

            _packetLength = 0;
            if (receivedData.UnreadLength() >= 4)
            {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    return true;
                }
            }
        }

        if (_packetLength <= 1)
        {
            return true;
        }

        return false;
    }

    #endregion
}
