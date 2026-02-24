using UnityEngine;

public class EnemyButtonSpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnLocation;

    public void SpawnEnemy()
    {
        if (enemyPrefab != null && spawnLocation != null)
        {
            Instantiate(enemyPrefab, spawnLocation.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Enemy prefab or spawn location is not assigned in the inspector.");
        }
    }
}
