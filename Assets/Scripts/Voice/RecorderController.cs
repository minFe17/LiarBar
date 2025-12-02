using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using UnityEngine;

public class RecorderController : MonoBehaviourPunCallbacks
{
    private GameObject localVoiceObject;

    void Start()
    {
        SpawnLocalPlayerVoice();
    }

    void SpawnLocalPlayerVoice()
    {
        if (!photonView.IsMine) return;

        // 로컬 플레이어는 LocalVoicePrefab
        localVoiceObject = PhotonNetwork.Instantiate("PlayerVoiceLocal", Vector3.zero, Quaternion.identity);

        // 기존 플레이어에게는 RemoteVoicePrefab 생성 요청
        photonView.RPC("SpawnRemotePlayerVoice", RpcTarget.OthersBuffered, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    void SpawnRemotePlayerVoice(int actorNumber)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
            return;

        if (GameObject.Find("PlayerVoiceRemote_" + actorNumber) != null)
            return;

        GameObject remote = PhotonNetwork.Instantiate("PlayerVoiceRemote", Vector3.zero, Quaternion.identity);
        remote.name = "PlayerVoiceRemote_" + actorNumber;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
            photonView.RPC("SpawnRemotePlayerVoice", newPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
    }
}