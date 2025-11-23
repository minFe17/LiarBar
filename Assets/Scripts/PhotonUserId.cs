using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonUserId : MonoBehaviour
{
    void Awake()
    {
        if (PhotonNetwork.AuthValues == null)
            PhotonNetwork.AuthValues = new AuthenticationValues();

#if UNITY_EDITOR
        // 에디터 멀티플레이 모드에서는 ActorNumber 기반으로 UserId 생성
        string editorSuffix = "_" + UnityEngine.Random.Range(10000, 99999); // 안전하게 랜덤
        PhotonNetwork.AuthValues.UserId = System.Guid.NewGuid().ToString() + editorSuffix;
#else
        if (string.IsNullOrEmpty(PhotonNetwork.AuthValues.UserId))
            PhotonNetwork.AuthValues.UserId = System.Guid.NewGuid().ToString();
#endif

        Debug.Log("Photon UserId: " + PhotonNetwork.AuthValues.UserId);
    }
}