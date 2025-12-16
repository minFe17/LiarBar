using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameResultUI : MonoBehaviour
{
    [SerializeField] List<Text> _playerNickNames;
    [SerializeField] Text _timerText;

    float _timertime = 5f;

    void Start()
    {

    }
}