using System;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Steamworks.Data;

public class Client : MonoBehaviour
{
    [SerializeField]
    GameObject playerPrefab;

    [SerializeField]
    GameManager gameManager;

    // to run at fixed tick rate
    private float clientTimer;
    public uint clientTick;
    private float minTimeBetweenTicks;
    private const float CLIENT_TICK_RATE = 120f;

    private Queue<P2Packet?> receivedPackets;

    private Lobby currentLobby;
    private Friend owner;
    private Player localPlayer;

    void Start()
    {
        clientTimer = 0.0f;
        clientTick = 0;
        minTimeBetweenTicks = 1 / CLIENT_TICK_RATE;

        receivedPackets = new Queue<P2Packet?>();

        if (SteamLobbyManager.Instance)
        {
            currentLobby = SteamLobbyManager.Instance.CurrentLobby;
            owner = currentLobby.Owner;
            // get server tick
            clientTick = Convert.ToUInt32(currentLobby.GetData("ServerTick"));
            // Instantiate player on server
            var packet = new Packet(Packet.PacketType.InstantiatePlayer);
            SendToServer(packet.buffer.ToArray());
        }
    }


    void Update()
    {
        if (SteamLobbyManager.Instance && currentLobby.Owner.Id != owner.Id)
        {
            // Means owner changed, server changed
            owner = currentLobby.Owner;
            clientTick = Convert.ToUInt32(currentLobby.GetData("ServerTick"));
        }

        // Receive packets ASAP, client receives packets only if he is not a server
        if (SteamLobbyManager.Instance && !owner.IsMe)
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

        if (Input.GetKey(KeyCode.Escape) && SteamLobbyManager.Instance)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SendToServer(new Packet(Packet.PacketType.PlayerLeaving).buffer.ToArray());
            SteamLobbyManager.Instance.LeaveLobby();
        }

        // Get input
        Structs.Inputs inputs;
        inputs.up = Input.GetKey(KeyCode.W);
        inputs.down = Input.GetKey(KeyCode.S);
        inputs.left = Input.GetKey(KeyCode.A);
        inputs.right = Input.GetKey(KeyCode.D);
        inputs.jump = Input.GetKey(KeyCode.Space);
        inputs.axisX = Input.GetAxisRaw("Mouse X");

        if (localPlayer)
        {
            // if i Am a server let server process
            // to avoid multiple mouvement process
            // this probably should be reworked
            if (owner.Id != SteamManager.Instance.PlayerSteamId)
            {
                localPlayer.ProcessMouvement(inputs, minTimeBetweenTicks);
                localPlayer.ProcessJump(inputs);
            }
            localPlayer.UpdateCamera(inputs.axisX, minTimeBetweenTicks);

            Structs.InputMessage inputMsg;
            inputMsg.tick_number = clientTick;
            inputMsg.inputs = inputs;

            var inputPacket = new Packet(Packet.PacketType.InputMessage);
            inputPacket.InsertInputMessage(inputMsg);
            SendToServer(inputPacket.buffer.ToArray());
        }

        // Handle received packets
        while (receivedPackets.Count > 0)
        {
            var recPacket = receivedPackets.Dequeue();

            var packet = new Packet(recPacket.Value.Data);

            if (packet.GetPacketType() == Packet.PacketType.InstantiatePlayer)
            {
                Debug.Log("Will instantiate a player ...");
                GameObject playerObj = Instantiate(playerPrefab, GameObject.Find("SpawnPoint").transform.position, Quaternion.identity);
                SteamId playerId = packet.PopUInt64();
                Debug.Log("Received ID : " + playerId);

                // if me set local player to instantiated player
                if (playerId == SteamManager.Instance.PlayerSteamId)
                {
                    localPlayer = playerObj.GetComponent<Player>();
                    gameManager.AddPlayerToList(playerId, localPlayer.gameObject);
                } else
                {
                    gameManager.AddPlayerToList(playerId, playerObj.gameObject);
                }
            } else if (packet.GetPacketType() == Packet.PacketType.StateMessage)
            {
                SteamId playerId = packet.PopUInt64();
                Structs.StateMessage stateMsg = packet.PopStateMessage();
                if (gameManager.playerList.ContainsKey(playerId))
                {
                    gameManager.playerList[playerId].transform.position = new Vector3(stateMsg.position.x, stateMsg.position.y, stateMsg.position.z);
                    gameManager.playerList[playerId].transform.rotation = new Quaternion(stateMsg.rotation.x, stateMsg.rotation.y, stateMsg.rotation.z, stateMsg.rotation.w);
                }
            } else if (packet.GetPacketType() == Packet.PacketType.PlayerLeaving)
            {
                SteamId playerId = packet.PopUInt64();
                if (gameManager.playerList.ContainsKey(playerId))
                {
                    gameManager.RemovePlayerFromList(playerId);
                }
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


    public void PacketManualEnqeue(P2Packet packet)
    {
        receivedPackets.Enqueue(packet);
    }


    private void SendToServer(byte[] data)
    {
        SteamNetworking.SendP2PPacket(currentLobby.Owner.Id, data);
    }

}
