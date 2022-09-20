using System;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;

public class Server : MonoBehaviour
{
    // to run at fixed tick rate
    private float serverTimer;
    public uint serverTick;
    private float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 120f;

    private Queue<P2Packet?> receivedPackets;

    private Steamworks.Friend owner;

    void Start()
    {
        serverTimer = 0.0f;
        serverTick = 0;
        minTimeBetweenTicks = 1 / SERVER_TICK_RATE;
        
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
            serverTick = Convert.ToUInt32(SteamLobbyManager.Instance.CurrentLobby.GetData("ServerTick"));
        }
        
        if (SteamLobbyManager.Instance && !owner.IsMe)
        {
            return;
        }

        // Receive packets ASAP
        ReceivePackets();

        serverTimer += Time.deltaTime;

        while (serverTimer >= minTimeBetweenTicks)
        {
            serverTimer -= minTimeBetweenTicks;

            // Handle tick here
            if (SteamManager.Instance)
                SteamLobbyManager.Instance.CurrentLobby.SetData("ServerTick", serverTick.ToString());

            HandleTick();

            serverTick++;
        }
    }


    private void HandleTick()
    {


        // handle received packets
        while(receivedPackets.Count > 0)
        {
            var recPacket = receivedPackets.Dequeue();
            var packet = new Packet(recPacket.Value.Data);

            if (packet.GetPacketType() == Packet.PacketType.InstantiatePlayer)
            {
                Debug.Log("Instantiating player ...");
            }
            
        }


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
