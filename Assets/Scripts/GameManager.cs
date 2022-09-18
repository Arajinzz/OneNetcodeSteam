using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{

    [SerializeField]
    TMP_Text ownerText;

    void Update()
    {
        if (SteamLobbyManager.Instance)
        {
            ownerText.SetText(SteamLobbyManager.Instance.CurrentLobby.Owner.Name);
        }
    }
}
