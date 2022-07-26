using UnityEngine;

public class PowerUpSpawnController : MonoBehaviour
{
    public static PowerUpSpawnController instance { get; private set; }

    public GameObject[] listOfPowerUps;

    [Tooltip("The probability (in %) that a power up gets spawned")]
    [Range(0, 100)]
    public int probabilityOfPowerUpSpawn;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void OnPowerUpSpawn(Vector3 positionToSpawn)
    {
        if (listOfPowerUps == null || listOfPowerUps.Length == 0)
            return;

        int randomPick = Random.Range(1, 100);
        if (randomPick <= probabilityOfPowerUpSpawn)
        {
            var nextPowerUpToSpawn = GetRandomPowerUp();

            var newSpawnedPowerup = NetworkObjectSpawner.SpawnNewNetworkObject(nextPowerUpToSpawn);

            newSpawnedPowerup.transform.position = positionToSpawn;
        }
    }

    private GameObject GetRandomPowerUp()
    {
        int randomPick = Random.Range(0, listOfPowerUps.Length - 1);

        return listOfPowerUps[randomPick];
    }

    private void OnDestroy()
    {
        instance = null;
    }
}