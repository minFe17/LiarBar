using Photon.Voice.Unity;
using Photon.Voice.PUN;
using Photon.Pun;
using UnityEngine;
using Utils;
using UnityEngine.InputSystem;


public class VoiceManager : MonoBehaviour
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

    private void Update()
    {
        //if(Keyboard.current.aKey.isPressed)
        //{
        //    Debug.Log("PunVoiceClient IsConnected: " + PunVoiceClient.Instance.Client.IsConnected);
        //    Debug.Log("PunVoiceClient InRoom: " + PunVoiceClient.Instance.Client.InRoom);
        //}
    }
    private void Start()
    {
       
    }
}