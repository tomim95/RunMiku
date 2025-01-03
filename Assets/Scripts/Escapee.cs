using System.Collections;
using UnityEngine;

public class Escapee : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float turnSpeed = 2f;
    [SerializeField] private float safeDistance = 20f;
    [SerializeField] private float patrolRadius = 3f;
    [SerializeField] private float directionChangeInterval = 3f;
    [SerializeField] private float speedBoostMultiplier = 1.3f;
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float speedDecayRate = 0.5f; // Rate of speed decay per second

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private GameManager gameManager;

    private Vector2 currentDirection;
    private Vector2 patrolDirection;
    private float currentSpeed;
    private bool isPatrolling = true;
    public bool IsPatrolling
    {
        get { return isPatrolling; }
    }

    private bool isSpeedBoosted = false;
    private float escapeTimer = 0f;

    private string currentAnim = "";

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameManager = FindObjectOfType<GameManager>();

        currentSpeed = baseSpeed;
        currentDirection = Vector2.right; // Initial direction
        patrolDirection = GetRandomPatrolDirection();
        ChangeAnimation("EscIdle", 0f);

        StartCoroutine(PatrolDirectionChangeRoutine());
    }

    private void Update()
    {
        if (gameManager.gameIsOn)
        {
            HandleAnimation();
            HandleSpeedBoostDetection();
        }

        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        if (gameManager.gameIsOn)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

            if (distanceToPlayer < safeDistance && !player.GetComponent<PlayerMove>().GetDetainedStatus())
            {
                EscapeFromPlayer();
            }
            else
            {
                Patrol();
            }
        }

        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void EscapeFromPlayer()
    {
        isPatrolling = false;

        // Calculate direction away from the player
        Vector2 directionAwayFromPlayer = (transform.position - player.transform.position).normalized;

        // Update direction
        currentDirection = Vector2.Lerp(currentDirection, directionAwayFromPlayer, Time.deltaTime * turnSpeed);

        // Decay speed over time during escape
        escapeTimer += Time.deltaTime;
        currentSpeed = Mathf.Clamp(baseSpeed - (speedDecayRate * escapeTimer), minSpeed, maxSpeed);

        rb.velocity = currentDirection * currentSpeed;

        // Reset patrol state when speed drops to minimum
        if (currentSpeed <= minSpeed)
        {
            escapeTimer = 0f; // Reset timer for future escapes
            isPatrolling = true;
            StartCoroutine(PatrolDirectionChangeRoutine());
        }
    }

    private void Patrol()
    {
        if (!isPatrolling)
        {
            isPatrolling = true;
            StartCoroutine(PatrolDirectionChangeRoutine());
        }

        // Smoothly transition to the patrol direction
        currentDirection = Vector2.Lerp(currentDirection, patrolDirection, Time.deltaTime * turnSpeed);

        rb.velocity = currentDirection * (baseSpeed * 0.75f);
    }

    private void HandleSpeedBoostDetection()
    {
        if (isSpeedBoosted) return;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Runner"))
            {
                baseSpeed = currentSpeed;
                StartCoroutine(SpeedBoost());
                break;
            }
        }
    }

    private IEnumerator SpeedBoost()
    {
        isSpeedBoosted = true;
        float boostedSpeed = Mathf.Min(baseSpeed * speedBoostMultiplier, maxSpeed);
        currentSpeed = boostedSpeed;

        yield return new WaitForSeconds(1.5f); // Speed boost duration

        currentSpeed = baseSpeed;
        isSpeedBoosted = false;
    }

    private IEnumerator PatrolDirectionChangeRoutine()
    {
        while (isPatrolling)
        {
            patrolDirection = GetRandomPatrolDirection();
            yield return new WaitForSeconds(directionChangeInterval);
        }
    }

    private Vector2 GetRandomPatrolDirection()
    {
        // Generate a random direction within the patrol radius
        float x = Random.Range(-patrolRadius, patrolRadius);
        float y = Random.Range(-patrolRadius, patrolRadius);
        return new Vector2(x, y).normalized;
    }

    private void HandleAnimation()
    {
        if (rb.velocity.magnitude > 0.1f)
        {
            ChangeAnimation("EscRun", 0);
        }
        else
        {
            ChangeAnimation("EscIdle", 0);
        }

        // Flip sprite based on movement direction
        if (rb.velocity.x > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (rb.velocity.x < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    public void ChangeAnimation(string animation, float crossfade = 0)
    {
        if (currentAnim != animation)
        {
            currentAnim = animation;
            animator.CrossFade(animation, crossfade);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the safe distance and patrol radius in the editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, safeDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
    }

    public void EnableHeart()
    {
        GetComponentInChildren<Heart>(true).gameObject.SetActive(true);
    }
}
