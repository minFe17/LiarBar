using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Photon.Pun;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
    public void OnClickRestart()
    {
        EventManager.Instance.Invoke<bool>("ResetGameManager", true);
    }
    private void Start()
    {
        OffPanel();
    }

    private void OnEnable()
    {
        EventManager.Instance.Subscribe("OffEndGameUI", OffPanel);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("OffEndGameUI", (Action)OffPanel);
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
            str += i + 1 + "등 " + "플레이어 아이디" + FindPlayer(indexes[i]) + " ";
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
        str+=num + "등 + 플레이어 아이디 " + FindPlayer(indexes[winner])+" "+ 
            DataManager.Instance.FindDataToType((ESeotdaRuleType)types[winner]).Value.name + "\n";
        num++;
        for (int i = 0; i < types.Length; i++)
        {
            if (i == winner) continue;
            str += num++ + "등 " + "플레이어 아이디" + FindPlayer(indexes[i]) + " ";
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
    #endregion
}