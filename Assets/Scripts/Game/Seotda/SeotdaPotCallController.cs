using UnityEngine;
using TMPro;
using Photon.Pun;

public class SeotdaPotCallController : MonoBehaviourPun
{
    private TextMeshPro _text;

    private float _timer = 0;
    private bool _isOn = false;
    void Start()
    {
        Transform callPot = transform.Find("TableObject/CallPot");
        _text = callPot.GetComponent<TextMeshPro>();
        //카메라 방향 바라보게 회전해야됨
    }
    void LateUpdate()
    {
        LookAtCameraY();
    }
    void LookAtCameraY()
    {
        //위왼아래오른쪽
        Vector3[] rotation = { new Vector3(0, 180, 0), new Vector3(0, 90, 0), new Vector3(0, 0, 0), new Vector3(0, 270, 0) };
        _text.transform.rotation = Quaternion.Euler(rotation[MyPlayer.local.PositionIndex]);
    }


    [PunRPC]
    private void RPC_OnCallText(string text, int r, int g, int b)
    {
        _text.text = text;
        _text.color = new Color32((byte)r, (byte)g, (byte)b, 255);
    }

}
