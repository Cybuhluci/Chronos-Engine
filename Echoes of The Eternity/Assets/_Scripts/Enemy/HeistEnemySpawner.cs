using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public class HeistEnemySpawner : MonoBehaviour
{
    public GameObject[] _CommonEnemies;
    public GameObject[] _SpecialEnemies;
    [SerializeField] private EnemyButtonSpawner[] spawners;
    [SerializeField] private float spawnInterval = 1f; // Time between spawns in seconds
    private float spawnTimer;

    int commonEnemiesSpawned = 0;
    int specialEnemiesSpawned = 0;
    int maxEnemiesToSpawn;

    private void Update()
    {
        if (MissionManager.Instance.GetHeistStage() == MissionManager.HeistStage.Stealth)
        {
            return; // Don't spawn enemies while the player is in stealth phase
        }

        // during the stealth phase = no enemy spawn
        // during the control phase = spawn enemies at a slow rate
        // during the anticipation phase = enemy spawn begins to ramp up
        // during the assault phase = enemy spawn is at its peak
        // during the fade phase = enemy spawn begins to slow down

        // this script does NOT change the heist phase, it only manages the spawning for the phases.
        // every phase will have a timer attached to it - control is 1 minute, anticipation  15 seconds, assault 3 minutes, fade 30 seconds.
        // and then after fade it repeats.


        // difficulty scaling based on the heist stage
        // Easy should have small amounts, only about 10 enemies in total, with a long spawn interval - no special enemies

        //after here, special enemies spawn - the amount of special enemies that can exist at a time is 10% of the total enemy spawn of that difficulty
        // normal should have a moderate amount, about 20 enemies in total
        // Hard should have a large amount, about 30 enemies in total
        // VeryHard should have a very large amount, about 40 enemies in total
        // Overkill should have an overwhelming amount, about 50 enemies in total
        // Mayhem should have an insane amount, about 60 enemies in total


        // about wave spawns:
        // during control phase, waves spawn once 90% of the spawned enemies die.
        // during anticipation phase, two waves spawn at once.
        // during assault phase, waves spawn once 30% of the spawned enemies die.
        // during fade phase, waves stop spawning until the control phase goes back on.

        switch (MissionManager.Instance.currentHeistStage)
        {
            case MissionManager.HeistStage.Stealth:
                spawnInterval = 69f; // effectively no spawns during stealth
                break;
            case MissionManager.HeistStage.Control:
                spawnInterval = 5f; // spawn every 5 seconds during control phase
                break;
            case MissionManager.HeistStage.Anticipation:
                spawnInterval = 5f; // spawn every 5 seconds during anticipation phase
                break;
            case MissionManager.HeistStage.Assault:
                spawnInterval = 5f; // spawn every 5 seconds during assault phase
                break;
            case MissionManager.HeistStage.Fade:
                spawnInterval = 10f; // spawn every 10 seconds during fade phase
                break;
            default:
                break;
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            int funScore = Random.Range(0, 1); // Random value to determine whether to spawn a common or special enemy
            if (funScore == 0)
            {
                SpawnEnemyCommon();
            }
            else
            {
                SpawnEnemySpecial();
            }
            spawnTimer = 0f; // Reset the timer after spawning an enemy
        }
    }

    private void SpawnEnemyCommon()
    {
        if (spawners.Length == 0 || _CommonEnemies == null)
        {
            Debug.LogWarning("No spawn points or common enemy prefabs assigned.");
            return;
        }
        // Choose a random spawn point from the array
        EnemyButtonSpawner spawner = spawners[Random.Range(0, spawners.Length)];
        //Instantiate(enemyPrefab, spawner.transform.position, spawner.transform.rotation);
        GameObject enemyPrefab = _CommonEnemies[Random.Range(0, _CommonEnemies.Length)];
        spawner.SpawnEnemyExternal(enemyPrefab);
    }

    private void SpawnEnemySpecial()
    {
        if (spawners.Length == 0 || _SpecialEnemies == null)
        {
            Debug.LogWarning("No spawn points or special enemy prefabs assigned.");
            return;
        }
        // Choose a random spawn point from the array
        EnemyButtonSpawner spawner = spawners[Random.Range(0, spawners.Length)];
        //Instantiate(enemyPrefab, spawner.transform.position, spawner.transform.rotation);
        GameObject enemyPrefab = _SpecialEnemies[Random.Range(0, _SpecialEnemies.Length)];
        spawner.SpawnEnemyExternal(enemyPrefab);
    }

    private void SpawnCaptainFinalWave()
    {
        // do not worry about this, since this will NOT be in my demo - forget this captain part.
        // it is just a special enemy that spawns on the 5th assault phase that doesnt make the assault end until he dies.
    }
}
