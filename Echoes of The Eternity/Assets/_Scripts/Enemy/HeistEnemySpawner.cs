using UnityEngine;

public class HeistEnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private EnemyButtonSpawner[] spawners;
    [SerializeField] private float spawnInterval = 5f; // Time between spawns in seconds
    private float spawnTimer;

    private void Update()
    {
        if (MissionManager.Instance.currentPlayerState == MissionManager.PlayerState.Casing)
        {
            return; // Don't spawn enemies while the player is casing the joint
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0f; // Reset the timer after spawning an enemy
        }
    }

    private void SpawnEnemy()
    {
        if (spawners.Length == 0 || enemyPrefab == null)
        {
            Debug.LogWarning("No spawn points or enemy prefab assigned.");
            return;
        }
        // Choose a random spawn point from the array
        EnemyButtonSpawner spawner = spawners[Random.Range(0, spawners.Length)];
        //Instantiate(enemyPrefab, spawner.transform.position, spawner.transform.rotation);
        spawner.SpawnEnemy();
    }
}
