using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

public class NicknameUI : MonoBehaviour
{
    FirebaseREST _firebaseRest;

    void Start()
    {
        _firebaseRest = MonoSingleton<FirebaseREST>.Instance;
    }

    public void OnEndNickname(string value)
    {
        _firebaseRest.User.SetField("nickname", value);
        _firebaseRest.SaveUserData();
        SceneManager.LoadScene("LobbyScene");
    }
}