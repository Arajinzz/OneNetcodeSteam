using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Steamworks;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    TMP_Text ownerText;

    [SerializeField]
    TMP_Text sTickText;

    [SerializeField]
    TMP_Text cTickText;

    [SerializeField]
    GameObject netManager;

    public Dictionary<SteamId, GameObject> playerList;

    private void Start()
    {
        playerList = new Dictionary<SteamId, GameObject>();
    }

    void Update()
    {
        if (SteamLobbyManager.Instance)
        {
            ownerText.SetText(SteamLobbyManager.Instance.CurrentLobby.Owner.Name);
        }

        if (netManager.GetComponent<Server>())
            sTickText.SetText(netManager.GetComponent<Server>().serverTick.ToString());

        if (netManager.GetComponent<Client>())
            cTickText.SetText(netManager.GetComponent<Client>().clientTick.ToString());
    }

    public void AddPlayerToList(SteamId id, GameObject player)
    {
        if (!playerList.ContainsKey(id))
        {
            playerList.Add(id, player);
            player.GetComponent<Player>().playerId = id;
        }

        Debug.Log("Adding player with id: " + id);
    }

    public void RemovePlayerFromList(SteamId id)
    {
        if (playerList.ContainsKey(id))
            playerList.Remove(id);
    }

}
