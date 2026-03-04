using UnityEngine;

public class EnemyButtonSpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnLocation;

    public void SpawnEnemyExternal(GameObject enemy)
    {
        if (enemy != null && spawnLocation != null)
        {
            Instantiate(enemy, spawnLocation.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Enemy prefab or spawn location is not assigned in the inspector.");
        }
    }

    public void SpawnEnemyButton()
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
