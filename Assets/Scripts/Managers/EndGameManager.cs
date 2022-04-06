using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EndGameManager : SingletonNetwork<EndGameManager>
{
    enum EndGameStatus
    {
        victory,
        defeat,
    };

    [SerializeField]
    EndGameStatus m_status;                     // Set the scene status to now if we are on victory o defeat scene

    [SerializeField]
    CharacterDataSO[] m_charactersData;         // The characters data use to take some data from there

    [SerializeField]
    Transform[] m_shipsPositions;               // The final positions of the ships 

    [SerializeField]
    AudioClip m_endGameClip;                    // The audio clip to reproduce when the scene start

    int m_shipPositionindex;                    // Var to move every player to diferent position

    PlayerShipScore m_bestPlayer;               // Catch who is the best player -> only on server
    List<ulong> m_connectedClients = new List<ulong>();

    void Start()
    {        
        AudioManager.Instance.PlaySound(m_endGameClip, 1);
    }

    [ClientRpc]
    void SetShipDataClientRpc(int enemiesDestroyed, int powerUpsUsed, int score, string spaceShipScoreName)
    {
        // Not optimal, but this is only called one time per ship
        // We use find because we cannot pass a object on RPC
        GameObject spaceShipScore = GameObject.Find(spaceShipScoreName);

        if (m_status == EndGameStatus.victory)
            spaceShipScore.GetComponent<PlayerShipScore>().SetShip(true, enemiesDestroyed, powerUpsUsed, score);
        else
            spaceShipScore.GetComponent<PlayerShipScore>().SetShip(false, enemiesDestroyed, powerUpsUsed, score);
    }

    [ClientRpc]
    void BestShipClientRpc(string spaceShipScoreName)
    {
        if (IsServer) return;

        // We use find because we cannot pass a object on RPC   
        GameObject spaceShipScore = GameObject.Find(spaceShipScoreName);
        spaceShipScore.GetComponent<PlayerShipScore>().BestShip();
    }

    IEnumerator HostShutdown()
    {
        // Tell all clients to shutdown
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

    public void ServerSceneInit(ulong clientId)
    {
        // Save the clients 
        m_connectedClients.Add(clientId);

        // Check if is the last client        
        if (m_connectedClients.Count < NetworkManager.Singleton.ConnectedClients.Count)
            return;

        // We do this only one time when all clients are connected so they sync correctly
        // Tell all clients instance to set the UI base on the server characters data
        int bestScore = -1;
        for (int i = 0; i < m_charactersData.Length; i++)
        {
            if (m_charactersData[i].isSelected)
            {
                // Spawn the spaceship                
                GameObject go = NetworkObjectSpawner.SpawnNewNetworkObject(
                    m_charactersData[i].spaceshipScorePrefab,
                    m_shipsPositions[m_shipPositionindex].position,
                    Quaternion.identity);

                // Check who has the best score
                // The score is calculated base on the enemies destroyed minus the power-ups the player used
                // Feel free to modify these values
                int enemyDestroyedScore = (m_charactersData[i].enemiesDestroyed * 100);
                int powerUpsUsedScore = (m_charactersData[i].powerUpsUsed * 50);
                int currentFinalScore = enemyDestroyedScore - powerUpsUsedScore;

                if (currentFinalScore > bestScore)
                {
                    m_bestPlayer = go.GetComponent<PlayerShipScore>();
                    bestScore = currentFinalScore;
                }

                // Victory or defeat so turn on the appropriate vfx
                if (m_status == EndGameStatus.victory)
                    go.GetComponent<PlayerShipScore>().SetShip(
                        true,
                        m_charactersData[i].enemiesDestroyed,
                        m_charactersData[i].powerUpsUsed,
                        currentFinalScore);
                else
                    go.GetComponent<PlayerShipScore>().SetShip(
                        false,
                        m_charactersData[i].enemiesDestroyed,
                        m_charactersData[i].powerUpsUsed,
                        currentFinalScore);

                // Set the values of the score on every instance
                SetShipDataClientRpc(
                    m_charactersData[i].enemiesDestroyed,
                    m_charactersData[i].powerUpsUsed,
                    currentFinalScore,
                    go.name);

                m_shipPositionindex++;
            }
        }

        // Set the crown to the best player
        if (m_status == EndGameStatus.victory)
        {
            m_bestPlayer.BestShip();

            BestShipClientRpc(m_bestPlayer.name);
        }
    }

    // When the button is press start the shutdown process
    public void GoToMenu()
    {
        if (IsServer)
        {
            StartCoroutine(HostShutdown());
        }
        else
        {
            Shutdown();
        }
    }
}
