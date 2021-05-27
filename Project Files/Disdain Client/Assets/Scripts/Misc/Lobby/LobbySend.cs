using UnityEngine;

public class LobbySend : MonoBehaviour
{
    public static void WelcomeReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            print(LobbyClient.instance.myId);
            _packet.Write(LobbyClient.instance.myId);
            _packet.Write(SavingManager.username);
            _packet.Write(SavingManager.skin);

            SendTCPData(_packet);
        }
    }

    public static void SendPlayerMovement(float horizontal, float vertical, bool jump)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
        {
            _packet.Write(horizontal);
            _packet.Write(vertical);
            _packet.Write(jump);

            _packet.Write(GameManager.ActivePlayerManager.transform.rotation);
            _packet.Write(GameManager.ActivePlayer.transform.rotation);

            _packet.Write(GameManager.moving);

            SendTCPData(_packet);
        }
    }

    #region Send Data Funcs

    public static void SendTCPData(Packet _packet) { _packet.WriteLength(); LobbyClient.instance.tcp.SendData(_packet); }

    #endregion
}
