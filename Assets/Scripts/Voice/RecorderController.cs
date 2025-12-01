using Photon.Pun;
using Photon.Realtime;
using Photon.Voice;
using Photon.Voice.Unity;
using UnityEngine;

public class RecorderController : MonoBehaviourPunCallbacks
{
    private GameObject localVoiceObject;

    void Start()
    {
        // 로컬 플레이어 Prefab 생성
        SpawnLocalPlayerVoice();

        // Voice 서버 연결
        var voiceFollow = FindObjectOfType<VoiceFollowClient>();
        if (voiceFollow != null && !voiceFollow.Client.IsConnected && PhotonNetwork.InRoom)
        {
            voiceFollow.ConnectAndJoinRoom();
        }
    }

    /// <summary>
    /// 자기 PlayerVoice 생성
    /// </summary>
    void SpawnLocalPlayerVoice()
    {
        if (!PhotonNetwork.LocalPlayer.IsLocal) return;

        // PhotonNetwork.Instantiate: PhotonView 자동 생성
        localVoiceObject = PhotonNetwork.Instantiate("PlayerVoice", Vector3.zero, Quaternion.identity);

        var recorder = localVoiceObject.GetComponent<Recorder>();
        if (recorder != null) recorder.TransmitEnabled = true;

        // 기존 플레이어에게 RPC로 자기 Prefab 생성 요청
        photonView.RPC("SpawnRemotePlayerVoice", RpcTarget.OthersBuffered, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    /// <summary>
    /// RPC: 기존 플레이어가 새 참가자 Prefab 생성
    /// </summary>
    [PunRPC]
    void SpawnRemotePlayerVoice(int actorNumber)
    {
        // 자기 자신이면 생성 안함
        if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber) return;

        // 이미 존재하면 생성 안함
        if (GameObject.Find("PlayerVoice_" + actorNumber) != null) return;

        // PhotonNetwork.Instantiate 사용 → PhotonView 포함, Voice 연결 가능
        GameObject remotePlayer = PhotonNetwork.Instantiate("PlayerVoice", Vector3.zero, Quaternion.identity);

        remotePlayer.name = "PlayerVoice_" + actorNumber;

        // 원격 플레이어는 송신 끔
        var recorder = remotePlayer.GetComponent<Recorder>();
        if (recorder != null) recorder.TransmitEnabled = false;
    }

    /// <summary>
    /// 나중에 들어온 플레이어도 방 생성자의 Prefab을 받아야 하므로 Start 시 호출
    /// </summary>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // 방 생성자일 경우, 새 플레이어에게 자기 Prefab 생성 RPC
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
            photonView.RPC("SpawnRemotePlayerVoice", newPlayer, PhotonNetwork.LocalPlayer.ActorNumber);
    }
}