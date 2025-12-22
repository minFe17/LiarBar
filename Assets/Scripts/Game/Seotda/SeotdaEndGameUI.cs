using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Photon.Pun;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;

public class SeotdaEndGameUI : MonoBehaviourPun
{
    [SerializeField]
    private GameObject _reGame;
    [SerializeField]
    private TextMeshProUGUI _name;
    [SerializeField]
    private GameObject _result;
    [SerializeField]
    private TextMeshProUGUI _resultText;
    [SerializeField]
    private Button _restartButton;
    [SerializeField]
    private Button _exitButton;

    private bool _isShowReGame = false;
    private float _timer = 0;
    private void OnClickRestart()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        Debug.Log("클릭 버튼");
        photonView.RPC("RPC_ReStartGame", RpcTarget.All, true);
    }
    private void Start()
    {
        OffPanel();
        _restartButton.onClick.AddListener(OnClickRestart);
    }

    private void OnEnable()
    {
        EventManager.Instance.Subscribe("OffEndGameUI", OffPanel);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("OffEndGameUI", (Action)OffPanel);
    }
    private void Update()
    {
        if(_reGame.activeSelf && _isShowReGame)
        {
            _timer += Time.deltaTime;
            if(_timer>3)
            {
                _isShowReGame = false;
                _timer = 0;
                if (!PhotonNetwork.IsMasterClient) return;
                photonView.RPC("RPC_ReStartGame", RpcTarget.All, false);
            }
        }
    }
    private void OffPanel()
    {
        _reGame.SetActive(false);
        _result.SetActive(false);
    }
    private int FindPlayer(int index)
    {
        for(int i = 0; i < PhotonNetwork.PlayerList.Length;i++)
        {
            if (index == (int)PhotonNetwork.PlayerList[i].CustomProperties["PositionIndex"])
                return i;
        }
        return -1;
    }
    #region RPC
    [PunRPC]
    private void RPC_OnResultPanel(int[] types, int[] indexes)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        _result.SetActive(true);
        string str="";
        for(int i=0;i< types.Length;i++)
        {
            string name = PhotonNetwork.PlayerList[FindPlayer(indexes[i])].NickName;
            str += i + 1 + "등 "  + name + " ";
            if ((ESeotdaRuleType)types[i] == ESeotdaRuleType.Keut || (ESeotdaRuleType)types[i] == ESeotdaRuleType.Ddang)
                str += GetComponent<SeotdaCardManager>().FindMonth(indexes[i]);
            str += DataManager.Instance.FindDataToType((ESeotdaRuleType)types[i]).Value.name + "\n";
        }

        _resultText.text = str;
        _restartButton.gameObject.SetActive(true);
        _exitButton.gameObject.SetActive(true);

        GetComponent<SeotdaTurnManager>().EndGame();
        photonView.RPC("RPC_SetEndPanel", RpcTarget.Others, str);
    }
    [PunRPC]
    private void RPC_OnResultPanelToSpecial(int[] types, int[] indexes, int winner)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        _result.SetActive(true);
        string str = "";
        int num = 1;
        string name = PhotonNetwork.PlayerList[FindPlayer(indexes[winner])].NickName;
        str +=num + "등 "+  name + " "+ 
            DataManager.Instance.FindDataToType((ESeotdaRuleType)types[winner]).Value.name + "\n";
        num++;
        for (int i = 0; i < types.Length; i++)
        {
            if (i == winner) continue;
            name = PhotonNetwork.PlayerList[FindPlayer(indexes[i])].NickName;
            str += num++ + "등 "  + name + " ";
            if ((ESeotdaRuleType)types[i] == ESeotdaRuleType.Keut || (ESeotdaRuleType)types[i] == ESeotdaRuleType.Ddang)
                str += GetComponent<SeotdaCardManager>().FindMonth(indexes[i]);
            str += DataManager.Instance.FindDataToType((ESeotdaRuleType)types[i]).Value.name + "\n";
        }

        _resultText.text = str;
        _restartButton.gameObject.SetActive(true);
        _exitButton.gameObject.SetActive(true);

        GetComponent<SeotdaTurnManager>().EndGame();
        photonView.RPC("RPC_SetEndPanel", RpcTarget.Others, str);
    }
    [PunRPC]
    private void RPC_SetEndPanel(string str)
    {
        _result.SetActive(true);
        _resultText.text = str;
        _restartButton.gameObject.SetActive(false);
        _exitButton.gameObject.SetActive(false);
    }    
    [PunRPC]
    private void RPC_OffResultPanel()
    {
        _result.SetActive(false);
    }
    [PunRPC]
    private void RPC_ReStartGame(bool isRestart)
    {
        _result.SetActive(false);
        _reGame.SetActive(false);
        EventManager.Instance.Invoke<bool>("ResetSeotdaTable", isRestart);
    }
    [PunRPC]
    private void RPC_OnRestartPanel(int type)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        _reGame.SetActive(true);
        string str = DataManager.Instance.FindDataToType((ESeotdaRuleType)type).Value.name;
        _name.text = str;
        _isShowReGame = true;
        photonView.RPC("RPC_OnRestartPanels", RpcTarget.Others, str);
    }
    [PunRPC]
    private void RPC_OnRestartPanels(string str)
    {
        _name.text = str;
        _reGame.SetActive(true);
        _isShowReGame = true;
    }
    #endregion
}