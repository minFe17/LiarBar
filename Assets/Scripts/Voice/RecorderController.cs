using Photon.Voice.Unity;
using UnityEngine;
using Utils;

public class RecorderController : MonoBehaviour
{
    void Start()
    {
        SimpleSingleton<VoiceManager>.Instance.Recorder = GetComponent<Recorder>();
    }
}
