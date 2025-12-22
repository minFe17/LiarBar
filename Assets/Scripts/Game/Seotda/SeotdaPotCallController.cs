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
        _text = GetComponent<TextMeshPro>();
        _text.gameObject.SetActive(false);
    }

    void Update()
    {
        if (_isOn)
        {
            _timer += Time.deltaTime;
            if (_timer > 3)
            {
                _isOn = false;
                _timer = 0;
            }
        }
    }


    [PunRPC]
    private void RPC_OnCallText(string text, Color32 color)
    {
        _isOn = true;
        _text.gameObject.SetActive(true);
        _text.text = text;
        _text.color = color;
    }

}
