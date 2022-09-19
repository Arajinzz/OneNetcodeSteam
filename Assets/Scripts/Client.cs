using System;
using UnityEngine;

public class Client : MonoBehaviour
{
    // to run at fixed tick rate
    private float clientTimer;
    public uint clientTick;
    private float minTimeBetweenTicks;
    private const float CLIENT_TICK_RATE = 60f;

    private Steamworks.Friend owner;

    void Start()
    {
        clientTimer = 0.0f;
        clientTick = 0;
        minTimeBetweenTicks = 1 / CLIENT_TICK_RATE;

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
            clientTick = Convert.ToUInt32(SteamLobbyManager.Instance.CurrentLobby.GetData("ServerTick"));
        }

        clientTimer += Time.deltaTime;

        while (clientTimer >= minTimeBetweenTicks)
        {
            clientTimer -= minTimeBetweenTicks;

            // Handle tick here

            clientTick++;
        }
    }
}
