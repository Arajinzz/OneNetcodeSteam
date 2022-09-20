using System;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;

public class Client : MonoBehaviour
{
    // to run at fixed tick rate
    private float clientTimer;
    public uint clientTick;
    private float minTimeBetweenTicks;
    private const float CLIENT_TICK_RATE = 60f;

    private Queue<P2Packet?> receivedPackets;

    private Steamworks.Friend owner;

    void Start()
    {
        clientTimer = 0.0f;
        clientTick = 0;
        minTimeBetweenTicks = 1 / CLIENT_TICK_RATE;

        receivedPackets = new Queue<P2Packet?>();

        if (SteamLobbyManager.Instance)
        {
            owner = SteamLobbyManager.Instance.CurrentLobby.Owner;
        }
    }


    void Update()
    {
        if (SteamLobbyManager.Instance && SteamLobbyManager.Instance.CurrentLobby.Owner.Id != owner.Id)
        {
            // Means owner changed, server changed
            owner = SteamLobbyManager.Instance.CurrentLobby.Owner;
            clientTick = Convert.ToUInt32(SteamLobbyManager.Instance.CurrentLobby.GetData("ServerTick"));
        }

        // Receive packets ASAP
        ReceivePackets();

        clientTimer += Time.deltaTime;

        while (clientTimer >= minTimeBetweenTicks)
        {
            clientTimer -= minTimeBetweenTicks;

            // Handle tick here
            HandleTick();

            clientTick++;
        }
    }


    private void HandleTick()
    {



    }


    private void ReceivePackets()
    {
        if (!SteamManager.Instance)
            return;

        while (SteamNetworking.IsP2PPacketAvailable())
        {
            var packet = SteamNetworking.ReadP2PPacket();
            if (packet.HasValue)
            {
                receivedPackets.Enqueue(packet);
            }
        }
    }


}
