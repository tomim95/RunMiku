using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] float spawnRate = 1.0f;
    [SerializeField] float spawnRange = 20f;
    [SerializeField] int spawnBulkCount = 50;
    [SerializeField] GameObject person;
    [SerializeField] GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnAudience());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator SpawnAudience()
    {
        while (true)
        {
            List<Vector3> spawnPositions = new List<Vector3>();

            while (spawnPositions.Count < spawnBulkCount)
            {
                Vector3 newSpawnPosition;

                do
                {
                    float spawnX = Random.Range(-spawnRange * 2, spawnRange * 2);
                    float spawnY = Mathf.Abs(spawnX) > spawnRange ?
                        // If distance over 20 on X axis, Y can be anything between 0 and 40
                        Random.Range(0, spawnRange * 2) :
                        // If distance less than 20 on X axis, Y can only be between 20 and 40
                        Random.Range(spawnRange, spawnRange * 2);

                    newSpawnPosition = new Vector3(spawnX, spawnY, 0);
                }
                while (newSpawnPosition.y < 0); // Repeat until Y is 0 or greater

                spawnPositions.Add(newSpawnPosition);
            }

            yield return new WaitForSeconds(spawnRate);

            foreach (var spawnPosition in spawnPositions)
            {
                GameObject personObj = Instantiate(
                    person,
                    new Vector3(player.transform.position.x + spawnPosition.x, player.transform.position.y + spawnPosition.y, 0),
                    Quaternion.identity);
                personObj.transform.parent = transform;

                bool isChaser = Random.Range(0, 2) == 1;

                // Set the person type based on the random result
                if (isChaser)
                {
                    personObj.GetComponent<Person>().SetPersonType(Person.PersonType.Chaser);
                }
                else
                {
                    personObj.GetComponent<Person>().SetPersonType(Person.PersonType.Runner);
                }
            }
        }
    }

}
