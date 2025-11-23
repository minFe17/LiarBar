using Photon.Pun;
using Photon.Voice.Unity;
using UnityEngine;
using Utils;

public class RecorderController : MonoBehaviour
{
    void Start()
    {
        GameObject temp = PhotonNetwork.Instantiate("PlayerVoice", Vector3.zero, Quaternion.identity);
        SimpleSingleton<VoiceManager>.Instance.Recorder = temp.GetComponent<Recorder>();
    }
}
