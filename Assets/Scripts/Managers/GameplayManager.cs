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
        m_numberOfPlayerConnected--;

        if (m_numberOfPlayerConnected <= 0)
        {
            LoadClientRpc();
            LoadingSceneManager.Instance.LoadScene(SceneName.Defeat);
        }
        else
        {
            // Send a client rpc to check which client was defeated, and activate the death UI
            ActivateDeathUIClientRpc(clientId);
        }
    }

    // Event to check when a player disconnect
    void OnClientDisconnect(ulong clientId)
    {
        foreach (var player in m_playerShips)
        {
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

        LoadingFadeEffect.Instance.FadeAll();
    }

    [ClientRpc]
    void SetPlayerUIClientRpc(int charIndex, string playerShipName)
    {
        // Not optimal, but this is only called one time per ship
        // We do this because we can not pass a GameObject in an RPC
        GameObject go = GameObject.Find(playerShipName);
        PlayerShipController playerShipController = go.GetComponent<PlayerShipController>();

        m_playersUI[m_charactersData[charIndex].playerId].SetUI(
            m_charactersData[charIndex].playerId,
            m_charactersData[charIndex].iconSprite,
            m_charactersData[charIndex].iconDeathSprite,
            playerShipController.health.Value,
            m_charactersData[charIndex].darkColor
        );

        // Pass the UI to the player
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
        if (IsServer)
            return;

        Shutdown();
    }

    public void BossDefeat()
    {
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
                    GameObject spaceShip = NetworkObjectSpawner.SpawnNewNetworkObject(
                        data.spaceshipPrefab,
                        m_shipStartingPositions[m_numberOfPlayerConnected].position,
                        null,
                        true,
                        true,
                        data.clientId);

                    PlayerShipController playerShipController = spaceShip.GetComponent<PlayerShipController>();
                    playerShipController.characterData = data;
                    playerShipController.gameplayManager = this;

                    m_playerShips.Add(playerShipController);
                    SetPlayerUIClientRpc(index, spaceShip.name);

                    m_numberOfPlayerConnected++;
                }

                index++;
            }
        }
    }
}
