using System;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;

public class Server : MonoBehaviour
{

    [SerializeField]
    GameManager gameManager;

    // to run at fixed tick rate
    private float serverTimer;
    public uint serverTick;
    private float minTimeBetweenTicks;
    private const float SERVER_TICK_RATE = 120f;

    private Queue<P2Packet?> receivedPackets;

    private Lobby currentLobby;
    private Friend owner;

    void Start()
    {
        serverTimer = 0.0f;
        serverTick = 0;
        minTimeBetweenTicks = 1 / SERVER_TICK_RATE;
        
        receivedPackets = new Queue<P2Packet?>();

        if (SteamLobbyManager.Instance)
        {
            currentLobby = SteamLobbyManager.Instance.CurrentLobby;
            owner = currentLobby.Owner;
        }
    }


    void Update()
    {   
        if (SteamLobbyManager.Instance && SteamLobbyManager.Instance.CurrentLobby.Owner.Id != owner.Id)
        {
            // Means owner changed, server changed
            owner = currentLobby.Owner;
            serverTick = Convert.ToUInt32(currentLobby.GetData("ServerTick"));
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
                currentLobby.SetData("ServerTick", serverTick.ToString());

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
                packet.InsertUInt64(recPacket.Value.SteamId);

                P2Packet packetToSend;
                packetToSend.SteamId = recPacket.Value.SteamId;
                packetToSend.Data = packet.buffer.ToArray();

                // Send to all players
                SendToAllLobby(packetToSend);

                // Send all players to target
                foreach (SteamId id in gameManager.playerList.Keys)
                {
                    if (id != recPacket.Value.SteamId)
                    {
                        Debug.Log("Sending to " + recPacket.Value.SteamId + " ID : " + id);
                        var newPack = new Packet(Packet.PacketType.InstantiatePlayer);
                        newPack.InsertUInt64(id);
                        SendToTarget(recPacket.Value.SteamId, newPack.buffer.ToArray());
                    }
                }
            }
            else if (packet.GetPacketType() == Packet.PacketType.InputMessage)
            {
                Structs.InputMessage inputMsg = packet.PopInputMessage();
                GameObject whatPlayer = gameManager.playerList[recPacket.Value.SteamId];
                whatPlayer.GetComponent<Player>().ProcessMouvement(inputMsg.inputs, minTimeBetweenTicks);
                whatPlayer.GetComponent<Player>().ProcessJump(inputMsg.inputs);

                Structs.StateMessage stateMsg;
                stateMsg.tick_number = inputMsg.tick_number + 1;
                stateMsg.position = whatPlayer.GetComponent<Player>().transform.position;
                stateMsg.rotation = whatPlayer.GetComponent<Player>().transform.rotation;

                var statePacket = new Packet(Packet.PacketType.StateMessage);
                statePacket.InsertUInt64(recPacket.Value.SteamId);
                statePacket.InsertStateMessage(stateMsg);

                P2Packet packetToSend;
                packetToSend.SteamId = recPacket.Value.SteamId;
                packetToSend.Data = statePacket.buffer.ToArray();

                SendToAllLobby(packetToSend);
            } else if (packet.GetPacketType() == Packet.PacketType.PlayerLeaving)
            {
                packet.InsertUInt64(recPacket.Value.SteamId);

                P2Packet packetToSend;
                packetToSend.SteamId = recPacket.Value.SteamId;
                packetToSend.Data = packet.buffer.ToArray();

                SendToAllLobby(packetToSend);
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


    private void SendToTarget(SteamId target, byte[] data)
    {
        SteamNetworking.SendP2PPacket(target, data);
    }

    public void SendToAllLobby(P2Packet packet)
    {
        foreach (Friend member in currentLobby.Members)
        {
            // This is me
            if (member.Id == owner.Id)
            {
                // Redirect packet to my client script
                gameObject.GetComponent<Client>().PacketManualEnqeue(packet);
                continue;
            }
            SendToTarget(member.Id, packet.Data);
        }
    }


}
