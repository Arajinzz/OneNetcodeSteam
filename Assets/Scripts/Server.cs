using System;
using UnityEngine;

public class Server : MonoBehaviour
{
    // to run at fixed tick rate
    private float serverTimer;
    public uint serverTick;
    private float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 60f;

    private Steamworks.Friend owner;

    void Start()
    {
        serverTimer = 0.0f;
        serverTick = 0;
        minTimeBetweenTicks = 1 / SERVER_TICK_RATE;

        if (SteamLobbyManager.Instance)
        {
            owner = SteamLobbyManager.Instance.CurrentLobby.Owner;
        }
    }


    void Update()
    {   
        if (!SteamLobbyManager.Instance && SteamLobbyManager.Instance.CurrentLobby.Owner.Id != owner.Id)
        {
            // Means owner changed, server changed
            owner = SteamLobbyManager.Instance.CurrentLobby.Owner;
            serverTick = Convert.ToUInt32(SteamLobbyManager.Instance.CurrentLobby.GetData("ServerTick"));
        }
        
        if (!SteamLobbyManager.Instance && !owner.IsMe)
        {
            return;
        }

        serverTimer += Time.deltaTime;

        while (serverTimer >= minTimeBetweenTicks)
        {
            serverTimer -= minTimeBetweenTicks;

            // Handle tick here
            SteamLobbyManager.Instance.CurrentLobby.SetData("ServerTick", serverTick.ToString());

            serverTick++;
        }
    }
}
