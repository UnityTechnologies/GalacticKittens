using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameplayManager : SingletonNetwork<GameplayManager>
{
    public static Action<ulong> playerDead;

    [SerializeField]
    CharacterDataSO[] m_charactersData;
    [SerializeField]
    PlayerUI[] m_playersUI;
    [SerializeField]
    GameObject m_deathUI;
    [SerializeField]
    Transform[] m_shipStartingPositions;

    int m_numberOfPlayerConnected;
    List<ulong> m_connectedClients = new List<ulong>();
    List<PlayerShipController> m_playerShips = new List<PlayerShipController>();

    void OnEnable()
    {
        if (IsServer)
        {            
            playerDead += PlayerDeath;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        }
    }

    void OnDisable()
    {
        if (IsServer)
        {
            playerDead -= PlayerDeath;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
        }
    }

    public void PlayerDeath(ulong clientId)
    {
        // print("PlayerDeath " + m_numberOfPlayerConnected);
        m_numberOfPlayerConnected--;

        if (m_numberOfPlayerConnected <= 0)
        {
            // print("GameLose " + m_numberOfPlayerConnected);
            // isGameOver = true;
            LoadClientRpc();
            LoadingSceneManager.Instance.LoadScene(SceneName.Defeat);
        }
        else
        {
            // Send a client rpc to check witch clients is the one that dies and activate the death UI
            ActivateDeathUIClientRpc(clientId);
        }
    }

    // Event to check when a player disconnect
    void OnClientDisconnect(ulong clientId)
    {
        // print($"OnClientDisconnect -> {clientId}");
        foreach (var player in m_playerShips)
        {
            // print($"OnClientDisconnect ->{player.name} {player.characterData.clientId} {clientId}");
            if (player != null)
            {
                if (player.characterData.clientId == clientId)
                {
                    player.Hit(999); // Do critical damage
                }
            }
        }
    }

    [ClientRpc]
    void ActivateDeathUIClientRpc(ulong clientId)
    {
        // print($"{clientId} == {NetworkManager.Singleton.LocalClientId}");
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            m_deathUI.SetActive(true);
        }
    }

    [ClientRpc]
    void LoadClientRpc()
    {
        if (IsServer)
            return;

        // LoadingFadeEffect.fadedAll?.Invoke();
        LoadingFadeEffect.Instance.FadeAll();
    }

    // void SetPlayerUIOnClients(int charIndex, string playerShipName)
    // {
    //     SetPlayerUIClientRpc(charIndex, playerShipName);
    // }

    [ClientRpc]
    void SetPlayerUIClientRpc(int charIndex, string playerShipName)
    {
        // Not optimal but this is only call one time per ship
        // We do this because we can not pass a GameObject on a RPC
        GameObject go = GameObject.Find(playerShipName);
        PlayerShipController playerShipController = go.GetComponent<PlayerShipController>();

        // print($"playerUI set -> {OwnerClientId}");
        m_playersUI[m_charactersData[charIndex].playerId].SetUI(
            m_charactersData[charIndex].playerId,
            m_charactersData[charIndex].iconSprite,
            m_charactersData[charIndex].iconDeathSprite,
            playerShipController.health.Value,
            m_charactersData[charIndex].darkColor
        );

        // Pass the UI to the player
        // print($"setplayuerUi ->  player id {_charactersData[charIndex].playerId} ");
        playerShipController.playerUI = m_playersUI[m_charactersData[charIndex].playerId];
    }

    IEnumerator HostShutdown()
    {
        // Tell the clients to shutdown
        ShutdownClientRpc();

        // Wait some time for the message to get to clients
        yield return new WaitForSeconds(0.5f);

        // Shutdown server/host
        Shutdown();
    }

    void Shutdown()
    {
        NetworkManager.Singleton.Shutdown();
        LoadingSceneManager.Instance.LoadScene(SceneName.Menu, false);
    }

    [ClientRpc]
    void ShutdownClientRpc()
    {
        if (IsServer) return;

        Shutdown();
    }

    public void BossDefeat()
    {
        // print("Boss defeated");
        LoadClientRpc();
        LoadingSceneManager.Instance.LoadScene(SceneName.Victory);
    }

    public void ExitToMenu()
    {
        if (IsServer)
        {
            StartCoroutine(HostShutdown());
        }
        else
        {
            NetworkManager.Singleton.Shutdown();
            LoadingSceneManager.Instance.LoadScene(SceneName.Menu, false);
        }

    }

    // So this method is call on the server each time a player enter the scene,
    // because of that if we create the ship when a player connects we could have a sync error
    // with the other clients because maybe the scene on the client is no yet load.
    // To fix this problem we wait until all clients call this method then we create the ships
    // for every client connected 
    public void ServerSceneInit(ulong clientId)
    {
        // Save the clients 
        m_connectedClients.Add(clientId);

        // Check if is the last client
        if (m_connectedClients.Count < NetworkManager.Singleton.ConnectedClients.Count)
            return;

        // For each client spawn and set UI
        foreach (var client in m_connectedClients)
        {
            int index = 0;
            foreach (CharacterDataSO data in m_charactersData)
            {
                if (data.isSelected && data.clientId == client)
                {
                    GameObject spaceShip = NetworkSpawnController.SpawnHelper(
                        data.spaceshipPrefab,
                        m_shipStartingPositions[m_numberOfPlayerConnected].position,
                        null,
                        true,
                        true,
                        data.clientId
                        );

                    PlayerShipController playerShipController = spaceShip.GetComponent<PlayerShipController>();
                    playerShipController.characterData = data;
                    playerShipController.gameplayManager = this;

                    m_playerShips.Add(playerShipController);
                    SetPlayerUIClientRpc(index, spaceShip.name);

                    m_numberOfPlayerConnected++;
                }
                index++;
            }
            // print($"PlayerConnected {m_numberOfPlayerConnected}");
        }
    }
}

// Pass to only clients all the players connected on a list

// int index = 0;
// foreach (CharacterDataSO data in m_charactersData)
// {
//     if (data.isSelected && data.clientId == clientId)
//     {
//         GameObject spaceShip = NetworkSpawnController.SpawnHelper(data.spaceshipPrefab, m_shipStartingPositions[m_numberOfPlayerConnected].position, null, true, true, data.clientId);
//         spaceShip.GetComponent<PlayerShipController>().characterData = data;
//         // SetPlayerUIOnClients(index, spaceShip.name);
//         SetPlayerUI(index, spaceShip.name);

//         m_numberOfPlayerConnected++;
//     }
//     index++;
// }
//     }
// }
