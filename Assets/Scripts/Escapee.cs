using System.Collections;
using UnityEngine;

public class Escapee : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] float speed = 5f;                  // Movement speed
    [SerializeField] float smoothTurnSpeed = 2f;        // Smooth turning speed
    [SerializeField] float safeDistance = 20f;          // Distance threshold for random movement
    [SerializeField] float randomDirectionChangeInterval = 3f; // Interval for random direction change

    private Rigidbody2D rb;
    private Vector2 currentDirection;     // The direction the object is currently moving towards
    private Vector2 targetDirection;      // The direction to move towards while wandering
    private bool isWandering = false;     // Flag to indicate if it's wandering

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentDirection = (transform.position - player.transform.position).normalized; // Initially away from the player
        targetDirection = currentDirection; // Start with moving away from the player
        StartCoroutine(ChangeDirectionRoutine()); // Start random wandering direction changes
    }

    void FixedUpdate()
    {
        // Get the distance from the player
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        // If within safe distance, run away from the player
        if (distanceToPlayer < safeDistance)
        {
            MoveAwayFromPlayer();
        }
        else
        {
            // If far from the player, wander around randomly
            if (!isWandering)
            {
                isWandering = true;
                StartCoroutine(ChangeDirectionRoutine()); // Start random wandering when far from player
            }

            MoveRandomly();
        }
    }

    // Run away from the player with smooth turns
    void MoveAwayFromPlayer()
    {
        // Calculate direction away from the player
        Vector2 awayFromPlayerDirection = (transform.position - player.transform.position).normalized;

        // Smoothly interpolate towards the direction away from the player
        currentDirection = Vector2.Lerp(currentDirection, awayFromPlayerDirection, Time.deltaTime * smoothTurnSpeed);

        // Apply movement in the calculated direction
        rb.velocity = currentDirection * speed;
    }

    // Move randomly when far from the player with smooth turns
    void MoveRandomly()
    {
        // Smoothly interpolate towards the random target direction
        currentDirection = Vector2.Lerp(currentDirection, targetDirection, Time.deltaTime * smoothTurnSpeed);

        // Apply movement in the calculated direction
        rb.velocity = currentDirection * speed;
    }

    // Coroutine to periodically change direction when wandering
    IEnumerator ChangeDirectionRoutine()
    {
        while (true)
        {
            // Change direction every randomDirectionChangeInterval when wandering
            yield return new WaitForSeconds(randomDirectionChangeInterval);

            // Pick a random direction for wandering
            targetDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        }
    }
}
