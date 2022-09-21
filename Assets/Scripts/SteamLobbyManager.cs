using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SteamLobbyManager : MonoBehaviour
{
    public static SteamLobbyManager Instance;

    // Lobby currently joined
    public Lobby CurrentLobby { get; set; }

    // Here resides the lobby search result
    public List<Lobby> LobbiesResult;

    private void Awake()
    {

        if (Instance == null)
        {
            DontDestroyOnLoad(this);
            Instance = this;
            LobbiesResult = new List<Lobby>();
        } else if (Instance != this)
        {
            Destroy(gameObject);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        SteamMatchmaking.OnLobbyCreated += OnLobbyCreatedCallback;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberDisconnectedCallback;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeaveCallback;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoinedCallback;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEnteredCallback;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInviteCallback;
        SteamMatchmaking.OnLobbyGameCreated += OnLobbyGameCreatedCallback;
        SteamMatchmaking.OnChatMessage += OnChatMessageCallback;
        SteamNetworking.OnP2PSessionRequest += OnP2PSessionRequestCallback;
        SceneManager.sceneLoaded += OnSceneLoadedCallback;
    }

    private void OnApplicationQuit()
    {
        CleanUp();
    }

    #region Help functions

    private void CleanUp()
    {
        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreatedCallback;
        SteamMatchmaking.OnLobbyMemberDisconnected -= OnLobbyMemberDisconnectedCallback;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeaveCallback;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoinedCallback;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEnteredCallback;
        SteamMatchmaking.OnLobbyInvite -= OnLobbyInviteCallback;
        SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreatedCallback;
        SteamMatchmaking.OnChatMessage -= OnChatMessageCallback;
        SteamNetworking.OnP2PSessionRequest -= OnP2PSessionRequestCallback;
        SceneManager.sceneLoaded -= OnSceneLoadedCallback;
    }

    #endregion

    #region Core functions

    public async Task<bool> CreateLobby(int maxPlayers, string gameName)
    {

        try
        {
            var lobbyOut = await SteamMatchmaking.CreateLobbyAsync(maxPlayers);
            
            if(!lobbyOut.HasValue)
            {
                throw new System.Exception("Lobby created but instantiated correctly");
            }

            lobbyOut.Value.SetPublic();
            lobbyOut.Value.SetJoinable(true);
            lobbyOut.Value.SetData("GameName", gameName);

            return true;

        } catch (System.Exception e)
        {
            Debug.Log(e);
        }

        return false;

    }

    public async Task<bool> JoinLobby(Lobby lobby)
    {
        RoomEnter joinedLobbySuccess = await lobby.Join();
        if (joinedLobbySuccess != RoomEnter.Success)
        {
            return false;
        }
        return true;
    }

    public void LeaveLobby()
    {
        try
        {
            CurrentLobby.Leave();
        }
        catch
        {
            return;
        }

        // Left Successfully
        // Load the desired scene
        SceneManager.LoadScene(0);
    }

    public async Task<bool> SearchLobbies(string gameName)
    {
        try
        {
            LobbiesResult.Clear();
            Lobby[] lobbies = await SteamMatchmaking.LobbyList.WithMaxResults(20)
                                                              .WithKeyValue("GameName", gameName)
                                                              .RequestAsync();
            if (lobbies != null)
            {
                foreach (Lobby lobby in lobbies.ToList())
                {
                    LobbiesResult.Add(lobby);
                }
            }
            return true;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
        
        return false;
    }

    #endregion

    #region Callbacks

    // Executed when a lobby is created
    private void OnLobbyCreatedCallback(Result result, Lobby lobby)
    {
        if (result == Result.OK)
        {
            CurrentLobby = lobby;
            CurrentLobby.SetData("GameState", "Waiting");

            // TEMPORARY: START GAME IMMEDIATLY
            CurrentLobby.SetGameServer(SteamManager.Instance.PlayerSteamId);
        }
    }

    // Executed only when a member Disconnected
    // Which means when i get Disconnected it will not get executed 
    private void OnLobbyMemberDisconnectedCallback(Lobby lobby, Friend member)
    {
        SteamManager.Instance.CloseP2P(member.Id);
    }

    // Executed only when a member leaves
    // Which means when i leave it will not get executed 
    private void OnLobbyMemberLeaveCallback(Lobby lobby, Friend member)
    {
        // The idea is when a member leaves the lobby
        // All users in lobby will execute this
        SteamManager.Instance.CloseP2P(member.Id);
        Debug.Log("The owner is : " + CurrentLobby.Owner.Id);
    }

    // Executed when a member joins the lobby
    private void OnLobbyMemberJoinedCallback(Lobby lobby, Friend member)
    {
        // The idea is when a member joins a lobby
        // All users in lobby will execute this
        SteamManager.Instance.AcceptP2P(member.Id);

        if (CurrentLobby.GetData("GameState").Equals("Started"))
        {
            // Means game started
        }
    }

    // Executed when we enter the lobby
    private void OnLobbyEnteredCallback(Lobby lobby)
    {
        CurrentLobby = lobby;

        if(CurrentLobby.GetData("GameState").Equals("Started"))
        {
            // Means game started
            SceneManager.LoadScene(1);
        }

        // TODO: load game scene
    }

    private void OnLobbyInviteCallback(Friend member, Lobby lobby)
    {

    }

    // When Game is started
    private void OnLobbyGameCreatedCallback(Lobby lobby, uint ip, ushort port, SteamId gameServerId)
    {
        Debug.Log("Game is Started by: " + gameServerId.ToString());

        CurrentLobby.SetData("GameState", "Started");
        CurrentLobby.SetData("ServerTick", "0");

        // Load game scene
        SceneManager.LoadScene(1);
    }

    // Executed when we receive a chat message
    private void OnChatMessageCallback(Lobby lobby, Friend friend, string message)
    {
        // Received chat message
        Debug.Log($"{friend.Name}: {message}");
    }

    private void OnP2PSessionRequestCallback(SteamId user)
    {
        Debug.Log("P2P Request from " + user.ToString());
        SteamManager.Instance.AcceptP2P(user);
    }

    // Executed when a scene is loaded
    private void OnSceneLoadedCallback(Scene scene, LoadSceneMode loadSceneMode)
    {
        // Lobby Scene Loaded
    }

    #endregion

}
