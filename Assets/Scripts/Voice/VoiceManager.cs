using Photon.Voice.Unity;
using Photon.Voice.PUN;
using Photon.Pun;
using UnityEngine;
using Utils;


public class VoiceManager : MonoBehaviourPunCallbacks
{
    bool _isOnVoice;
    Recorder _recorder;

    public Recorder Recorder { set => _recorder = value; }

    public void ChangeVoiceChat(bool isOn)
    {
        if(_recorder.MicrophoneDevice == null)
        {
            isOn = false;
        }
        _isOnVoice = isOn;
        _recorder.TransmitEnabled = _isOnVoice;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("VoiceManager OnJoinedRoom 호출됨!");
        if (PunVoiceClient.Instance != null && !PunVoiceClient.Instance.Client.InRoom)
        {
            PunVoiceClient.Instance.ConnectAndJoinRoom();
        }
    }
    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("Room Created: " + PhotonNetwork.CurrentRoom.Name);

        // Voice 연결
        if (SimpleSingleton<VoiceManager>.Instance != null)
        {
            SimpleSingleton<VoiceManager>.Instance.OnJoinedRoom();
        }
    }
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // PunVoiceClient가 이미 있으면 연결
        var voiceClient = PunVoiceClient.Instance;
        if (voiceClient != null)
        {
            DontDestroyOnLoad(voiceClient.gameObject);
        }
    }

}