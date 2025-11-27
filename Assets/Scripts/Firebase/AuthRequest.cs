[System.Serializable]
public class AuthRequest
{
    public string email;
    public string password;
    public bool returnSecureToken;

    public AuthRequest(string email, string password, bool returnSecureToken = true)
    {
        this.email = email;
        this.password = password;
        this.returnSecureToken = returnSecureToken;
    }
}