using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private float spawnRate = 1.0f;       // Time interval between spawns
    [SerializeField] private float spawnRange = 20f;       // Range for spawning positions
    [SerializeField] private int spawnBulkCount = 50;      // Number of objects to spawn at once
    [SerializeField] private GameObject person;            // The prefab to spawn
    [SerializeField] private GameObject player;            // Reference to the player object

    private Coroutine spawnCoroutine;
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        // Start spawning if the game is on when the game starts
        CheckAndStartSpawning();
    }

    private void Update()
    {
        // Continuously check the GameIsOn property and start/stop spawning accordingly
        CheckAndStartSpawning();
    }

    private void CheckAndStartSpawning()
    {
        bool gameIsOn = gameManager != null && gameManager.gameIsOn == true;

        // Start spawning if GameIsOn is true and the spawning hasn't started yet
        if (gameIsOn && spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnAudience(true));
        }
        // Stop spawning if GameIsOn becomes false
        else if (!gameIsOn && spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnAudience(bool instantSpawn = false)
    {
        if (instantSpawn)
        {
            SpawnBulk(); // Spawn the first batch immediately
        }

        // Continue spawning at regular intervals
        while (true)
        {
            yield return new WaitForSeconds(spawnRate);
            SpawnBulk();
        }
    }

    private void SpawnBulk()
    {
        List<Vector3> spawnPositions = GenerateSpawnPositions();

        foreach (Vector3 spawnPosition in spawnPositions)
        {
            GameObject personObj = Instantiate(
                person,
                new Vector3(player.transform.position.x + spawnPosition.x, player.transform.position.y + spawnPosition.y, 0),
                Quaternion.identity
            );
            personObj.transform.parent = transform;
            personObj.GetComponent<Person>().Initialize(gameManager);

            // Randomly assign the person type (Chaser or Runner)
            bool isChaser = Random.Range(0, 2) == 1;
            personObj.GetComponent<Person>().SetPersonType(isChaser ? Person.PersonType.Chaser : Person.PersonType.Runner);
        }
    }

    private List<Vector3> GenerateSpawnPositions()
    {
        List<Vector3> spawnPositions = new List<Vector3>();

        while (spawnPositions.Count < spawnBulkCount)
        {
            // Generate random X within the expanded range
            float spawnX = Random.Range(-spawnRange * 2, spawnRange * 2);
            float spawnY;

            if (Mathf.Abs(spawnX) <= spawnRange) // If X is within the range
            {
                // Y must be outside the range (-spawnRange or +spawnRange)
                if (Random.value > 0.5f)
                {
                    spawnY = Random.Range(spawnRange, spawnRange * 2); // Above spawnRange
                }
                else
                {
                    spawnY = Random.Range(-spawnRange * 2, -spawnRange); // Below -spawnRange
                }
            }
            else
            {
                // If X is outside the range, Y can be anywhere within the expanded range
                spawnY = Random.Range(-spawnRange * 2, spawnRange * 2);
            }

            // Add the new spawn position to the list
            spawnPositions.Add(new Vector3(spawnX, spawnY, 0));
        }

        return spawnPositions;
    }
}
