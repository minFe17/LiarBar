using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class GameResultUI : MonoBehaviour
{
    [SerializeField] List<Text> _playerNickNames;
    [SerializeField] Text _timerText;

    Dictionary<int, int> _rankMoneys = new Dictionary<int, int>
    {
        { 1, 7000 },
        { 2, 5000 },
        { 3, 3500 },
        { 4, 2000 }
    };

    float _timertime = 5f;
    bool _isSceneLoading = false;

    void Start()
    {
        ShowRankings();
        GetMoney();
    }

    void Update()
    {
        if (_isSceneLoading) return;

        _timertime -= Time.deltaTime;
        _timerText.text = Mathf.CeilToInt(_timertime).ToString();

        if (_timertime <= 0f)
        {
            _isSceneLoading = true;

            SimpleSingleton<MediatorManager>.Instance.ClearAll();

            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.LoadLevel("SelectCharacterScene");
        }
    }

    void ShowRankings()
    {
        // TurnManager에서 순위 정보 가져오기
        IReadOnlyDictionary<int, int> rankings = TurnManager.Instance.PlayerRankings;

        // 순위별로 정렬 (1등, 2등, 3등, 4등 순서)
        List<KeyValuePair<int, int>> sortedRankings = rankings.OrderBy(x => x.Value).ToList();

        // UI에 표시
        for (int i = 0; i < sortedRankings.Count && i < _playerNickNames.Count; i++)
        {
            int viewID = sortedRankings[i].Key;
            int rank = sortedRankings[i].Value;

            // ViewID로 플레이어 찾기
            GamePlayer player = PhotonView.Find(viewID)?.GetComponent<GamePlayer>();

            if (player != null)
                _playerNickNames[i].text = player.photonView.Owner.NickName;
        }
    }

    void GetMoney()
    {
        IReadOnlyDictionary<int, int> rankings = TurnManager.Instance.PlayerRankings;

        // 내 ViewID 찾기
        GamePlayer myPlayer = FindObjectsOfType<GamePlayer>().FirstOrDefault(p => p.photonView.IsMine);

        if (myPlayer != null && rankings.TryGetValue(myPlayer.ViewID, out int myRank))
        {
            if (_rankMoneys.TryGetValue(myRank, out int money))
            {
                int userMoney = MonoSingleton<FirebaseREST>.Instance.User.GetField<int>("money");
                MonoSingleton<FirebaseREST>.Instance.User.SetField("money", money + userMoney);
                MonoSingleton<FirebaseREST>.Instance.SaveUserData();
            }
        }
    }
}