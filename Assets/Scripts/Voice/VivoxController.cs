using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;
using Utils;

public class VivoxController : MonoBehaviour
{
    public static VivoxController Instance { get; private set; }

    private bool _initialized;
    private bool _loggedIn;

    private async void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        // 1. Unity Services
        await UnityServices.InitializeAsync();

        // 2. Auth
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        // 3. Vivox Init
        await VivoxService.Instance.InitializeAsync();
        _initialized = true;

        // 4. Vivox Login
        LoginOptions options = new LoginOptions
        {
            DisplayName = Guid.NewGuid().ToString()
        };

        await VivoxService.Instance.LoginAsync(options);
        _loggedIn = true;

        // 5. Join Channel
        string roomId = MonoSingleton<PhotonManager>.Instance.RoomID;
        await JoinVoiceChannelAsync(roomId);
    }

    public async Task JoinVoiceChannelAsync(string channelName)
    {
        if (!_initialized || !_loggedIn)
            return;

        await VivoxService.Instance.JoinGroupChannelAsync(channelName, ChatCapability.AudioOnly);
    }

    public void ChangeVoiceChat(bool isOn)
    {
        if (isOn)
            VivoxService.Instance.UnmuteInputDevice();
        else
            VivoxService.Instance.MuteInputDevice();
    }
}
