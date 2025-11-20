using Photon.Voice.Unity;

public class VoiceManager
{
    bool _isOnVoice;
    Recorder _recorder;

    public Recorder Recorder { set => _recorder = value; }

    public void ChangeVoiceChat(bool isOn)
    {
        _isOnVoice = isOn;
        _recorder.TransmitEnabled = _isOnVoice;
    }
}