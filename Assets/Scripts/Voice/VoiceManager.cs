using Photon.Voice.Unity;
using Photon.Voice.PUN;
using Photon.Pun;


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
        if (PunVoiceClient.Instance != null && !PunVoiceClient.Instance.Client.InRoom)
        {
            PunVoiceClient.Instance.ConnectAndJoinRoom();
        }
    }
}