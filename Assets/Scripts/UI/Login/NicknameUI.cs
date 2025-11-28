using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

public class NicknameUI : MonoBehaviour
{
    [SerializeField] GameObject _dplicateNicknameUI;
    FirebaseREST _firebaseRest;

    void Start()
    {
        _firebaseRest = MonoSingleton<FirebaseREST>.Instance;
    }

    public void OnEndNickname(string value)
    {
        StartCoroutine(_firebaseRest.CheckNicknameDuplicate(value, (isAvailable) =>
        {
            if (isAvailable)
            {
                _firebaseRest.User.SetField("nickname", value);
                _firebaseRest.SaveUserData();
                SceneManager.LoadScene("LobbyScene");
            }
            else
                _dplicateNicknameUI.SetActive(true);
        }));
    }
}