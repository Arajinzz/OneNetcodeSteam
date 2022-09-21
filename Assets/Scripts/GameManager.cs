using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
}
