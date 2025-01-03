using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    [SerializeField] GameObject notePrefab;
    [SerializeField] float noteLifeTime = 3f;
    [SerializeField] float noteSpawnRate = 2f;

    // Minimum and maximum saturation levels
    [SerializeField] private float minSaturation = 0.5f;
    [SerializeField] private float maxSaturation = 1f;

    // Minimum and maximum brightness levels (value)
    [SerializeField] private float minBrightness = 0.3f;  // Set a higher minimum to avoid black
    [SerializeField] private float maxBrightness = 1f;

    private Escapee escapee;
    private GameManager gameManager;
    private GameObject spawner;

    // Start is called before the first frame update
    void Start()
    {
        escapee = transform.parent.GetComponent<Escapee>();
        gameManager = FindObjectOfType<GameManager>();
        spawner = FindObjectOfType<Spawner>().gameObject;
        
        if (escapee && gameManager)
        {
            StartCoroutine(SpawnNotes());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    IEnumerator SpawnNotes()
    {
        while (true)
        {
            while (gameManager.gameIsOn && !escapee.IsPatrolling)
            {
                var note = Instantiate(notePrefab, transform.position, Quaternion.identity);
                note.transform.position = new Vector3(transform.position.x, transform.position.y, 0.1f);
                float randomSize = Random.Range(0.5f, 1f);
                transform.localScale = new Vector3(randomSize, randomSize, randomSize);
                note.transform.GetChild(0).GetComponent<SpriteRenderer>().color = GetRandomColor(); 
                note.transform.SetParent(spawner.transform, false);
                Destroy(note, noteLifeTime);
                yield return new WaitForSeconds(noteSpawnRate);
            }
            yield return null;
        }
    }

    Color GetRandomColor()
    {
        // Randomize the Hue (0 to 1), Saturation within range, and Value within range
        float hue = Random.Range(0f, 1f); // Random hue (0 to 1)
        float saturation = Random.Range(minSaturation, maxSaturation); // Saturation within the specified range
        float value = Random.Range(minBrightness, maxBrightness); // Value within the specified range

        // Convert HSV to RGB and return the color
        return Color.HSVToRGB(hue, saturation, value);
    }
}
