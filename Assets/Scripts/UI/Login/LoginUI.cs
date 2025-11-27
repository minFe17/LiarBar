using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginUI : AuthUIBase
{
    [Header("Sing Up UI")]
    [SerializeField] GameObject _singUpUI;

    [Header("Nickname UI")]
    [SerializeField] GameObject _nicknameUI;

    void OnEndLogine()
    {
        string nickname = _firebaseRest.User.GetField<string>("nickname");

        if(string.IsNullOrEmpty(nickname))
            _nicknameUI.SetActive(true);
        else
            SceneManager.LoadScene("LobbyScene");   
    }

    #region UI Event
    public void OnClickSingUpButton()
    {
        _singUpUI.SetActive(true);
        gameObject.SetActive(false);
    }

    public override void OnClickButton()
    {
        _firebaseRest.OnLoginSuccess += () => { OnEndLogine(); };
        base.OnClickButton();
        _firebaseRest.SingIn(_email, _password);
    }
    #endregion
}