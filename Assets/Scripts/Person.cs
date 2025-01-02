using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class Person : MonoBehaviour
{
    [SerializeField] float runnerMinSpeed = 2;
    [SerializeField] float runnerMaxSpeed = 3.5f;
    [SerializeField] float chaserSpeed = 4;
    [SerializeField] float aggroDistance = 15;
    [SerializeField] private float patrolRadius = 3f; // Radius for random patrolling
    [SerializeField] private float patrolSpeed = 2f; // Speed while patrolling

    [SerializeField] AudioClip[] audioClips;
    [SerializeField] AudioClip[] audioClips2;
    [SerializeField] AudioClip[] audioClips3;

    private AudioSource[] audioSources;
    private AudioSource audioSource;
    private AudioClip audioClip;
    private AudioClip audioCop;
    private AudioClip audioClip3;

    private GameObject chaseTarget;
    private GameObject player;
    private GameObject escapee;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Vector2 patrolTarget; // Current target position for patrolling
    private bool hasPatrolTarget = false; // Flag to check if a patrol target exists

    private bool pushing = false;
    public bool stunned = false;
    private float speed = 3;


    private float detectionRange = 15;
    private bool activeMode = true;
    private bool detainedRunner = false;
    private GameObject detainedObject;
    private int runnerType;

    private string currentAnim = "";

    public enum PersonType { Runner, Chaser };

    private PersonType type;

    public void SetPersonType(PersonType type)
    {
        this.type = type;
        if (type == PersonType.Runner)
        {
            chaseTarget = FindObjectOfType<Escapee>()?.gameObject;
        }
    }

    public void SetStunnedStatus(bool status) { this.stunned = status; }

    void Start()
    {
        runnerType = Random.Range(0, 2);
        audioSources = GetComponents<AudioSource>();
        animator = GetComponent<Animator>();

        player = FindObjectOfType<PlayerMove>().gameObject;
        escapee = FindObjectOfType<Escapee>().gameObject;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (type == PersonType.Runner)
        {
            //GetComponentInChildren<SpriteRenderer>().color = Color.yellow;
            if (runnerType == 0)
            {
                ChangeAnimation("RunnerRun", .2f);
            }
            else if (runnerType == 1) 
            {
                ChangeAnimation("Runner2Run", .2f);
            }
            
            gameObject.tag = "Runner";
            speed = Random.Range(runnerMinSpeed, runnerMaxSpeed);
            chaseTarget = escapee;
            audioSource = audioSources[2];
        }
        else if (type == PersonType.Chaser)
        {
            //GetComponentInChildren<SpriteRenderer>().color = Color.black;
            ChangeAnimation("CopRun", .2f);
            gameObject.tag = "Chaser";
            speed = chaserSpeed;
            StartCoroutine(CopSounds());
        }
    }

    void FixedUpdate()
    {
        if (!stunned)
        {
            // Only allow movement if not pushing or if detained
            if (!pushing && !detainedRunner)
            {
                if (gameObject.tag == "Chaser" && chaseTarget && Vector3.Distance(chaseTarget.transform.position, transform.position) < aggroDistance && activeMode)
                {
                    if (chaseTarget == player && player.GetComponent<PlayerMove>().GetDetainedStatus()) { return; }

                    // Calculate direction towards the target (chaseTarget)
                    Vector2 direction = (chaseTarget.transform.position - transform.position).normalized;
                    rb.velocity = direction * speed;
                }

                else if (gameObject.tag == "Runner" && chaseTarget != player && Vector3.Distance(chaseTarget.transform.position, transform.position) < aggroDistance && activeMode)
                {
                    // Calculate direction towards the target (chaseTarget)
                    Vector2 direction = (chaseTarget.transform.position - transform.position).normalized;
                    rb.velocity = direction * speed;
                }

                // If the player is too close to the runner (within 1/5 of aggro distance)
                else if (gameObject.tag == "Runner" && chaseTarget == player && Vector3.Distance(player.transform.position, transform.position) < aggroDistance / 3 && activeMode)
                {
                    Vector2 direction = (player.transform.position - transform.position).normalized;
                    rb.velocity = direction * speed;
                    //chaseTarget = escapee; // Switch back to the escapee when too close
                }

                else if (!activeMode)
                {
                    if (detainedObject != null)
                    {
                        float detainedPosX = transform.localScale.x > 0 ? transform.position.x - .5f : transform.position.x + .5f;
                        float detainedPosY = transform.position.y;
                        detainedObject.transform.position = new Vector2(detainedPosX, detainedPosY);
                    }
                    Vector2 direction = (transform.position - escapee.transform.position).normalized;
                    rb.velocity = direction * 3;
                }
                else
                {
                    // Handle random patrolling
                    if (gameObject.tag == "Runner")
                    {
                        chaseTarget = escapee;
                    }
                    Patrol();
                }
            }
            else
            {
                // When pushing, stop all movement
                rb.velocity = Vector2.zero;
            }
        }
        else
        {
            GetComponent<Collider2D>().enabled = false;
            StartCoroutine(RecoverStunnedState());
        }
    }


    void Patrol()
    {
        if (!hasPatrolTarget || Vector2.Distance(transform.position, patrolTarget) < 2f)
        {
            // Pick a new random patrol target within the defined radius
            Vector2 randomOffset = new Vector2(Random.Range(-patrolRadius, patrolRadius), Random.Range(-patrolRadius, patrolRadius));
            patrolTarget = (Vector2)transform.position + randomOffset;
            hasPatrolTarget = true;
        }

        // Move towards the patrol target
        Vector2 direction = (patrolTarget - (Vector2)transform.position).normalized;
        rb.velocity = direction * patrolSpeed;
    }

    private void Update()
    {
        if (!detainedRunner && rb.velocity.x > 0)
        {
            //spriteRenderer.flipX = true;
            transform.localScale = new Vector3(-1, 1, 1);
        }

        else if (!detainedRunner && rb.velocity.x < 0)
        {
            //spriteRenderer.flipX = false;
            transform.localScale = new Vector3(1, 1, 1);
        }

        if (type == PersonType.Chaser)
        {
            FindClosestRunner();
        }

        if (type == PersonType.Runner && !detainedRunner && !pushing)
        {
            Collider2D[] playersInRange = Physics2D.OverlapCircleAll(transform.position, 2);

            foreach (Collider2D player in playersInRange)
            {
                if (player.gameObject.tag == "Player" && Vector2.Distance(player.transform.position, transform.position) < 4)
                {
                    chaseTarget = this.player;
                }
            }
        }

        if (Vector2.Distance(player.transform.position, transform.position) > 25)
        {
            Destroy(gameObject);
        }

        if (!activeMode && detainedObject != player)
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, .3f);
        }

        else if (!activeMode && detainedObject == player)
        {
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1);
        }
    }

    IEnumerator CopSounds()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(6, 22));

            if (activeMode)
            {
                audioSources[2].clip = audioClips2[Random.Range(0, 1)];
                audioSources[2].Play();
            }
        }
    }

    private void FindClosestRunner()
    {
        Collider2D[] runnersInRange = Physics2D.OverlapCircleAll(transform.position, detectionRange);

        float closestDistance = Mathf.Infinity;
        GameObject newChaseTarget = null;

        foreach (Collider2D runnerCollider in runnersInRange)
        {
            if ((runnerCollider.CompareTag("Runner") && runnerCollider.gameObject.GetComponent<Person>().activeMode) ||
                (runnerCollider.CompareTag("Player") && !player.GetComponent<PlayerMove>().GetDetainedStatus()))
            {
                float distanceToRunner = Vector2.Distance(transform.position, runnerCollider.transform.position);
                if (distanceToRunner < closestDistance)
                {
                    closestDistance = distanceToRunner;
                    newChaseTarget = runnerCollider.gameObject;
                }
            }
        }

        if (newChaseTarget != null)
        {
            chaseTarget = newChaseTarget;
        }
    }

    // Coroutine to recover from the stunned state
    IEnumerator RecoverStunnedState()
    {
        yield return new WaitForSeconds(2f);  // Wait for the stunned duration
        GetComponent<Collider2D>().enabled = true;
        stunned = false;
        pushing = false;
        rb.velocity = Vector3.zero;
    }

    private void Recover()
    {
        if (runnerType == 0)
        {
            ChangeAnimation("RunnerRun", 0);
        }
        else if (runnerType == 1)
        {
            ChangeAnimation("Runner2Run", 0);
        }

        pushing = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (gameObject.tag == "Chaser")
        {
            if (collision.gameObject.tag == "Runner" && activeMode)
            {
                ChangeAnimation("CopRun2", 0f);
                activeMode = false;
                detainedObject = collision.gameObject;
                collision.gameObject.transform.localScale = transform.localScale;
                collision.gameObject.GetComponent<Person>().detainedRunner = true;
                GetComponent<Collider2D>().enabled = false;
                collision.collider.enabled = false;
                collision.gameObject.GetComponent<SpriteRenderer>().color = new Color(
                    collision.gameObject.GetComponent<SpriteRenderer>().color.r,
                    collision.gameObject.GetComponent<SpriteRenderer>().color.g,
                    collision.gameObject.GetComponent<SpriteRenderer>().color.b,
                    .3f);

                collision.gameObject.GetComponentInChildren<SpriteRenderer>().color = new Color(
                    collision.gameObject.GetComponent<SpriteRenderer>().color.r,
                    collision.gameObject.GetComponent<SpriteRenderer>().color.g,
                    collision.gameObject.GetComponent<SpriteRenderer>().color.b,
                    .3f);
                collision.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;

                collision.gameObject.transform.SetParent(this.transform);
                
            }

            if (collision.gameObject.tag == "Player" && !collision.gameObject.GetComponent<PlayerMove>().GetDetainedStatus())
            {
                ChangeAnimation("CopRun2", 0f);
                audioSources[0].Play();
                if (audioClips.Length > 0)
                {
                    audioSource = audioSources[1];
                    audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
                    audioSource.Play();
                }
                detainedObject = collision.gameObject;
                activeMode = false;
                collision.gameObject.GetComponent<PlayerMove>().SetDetained(true);
                collision.gameObject.transform.localScale = transform.localScale;
                GetComponent<Collider2D>().enabled = false;
                collision.collider.enabled = false;
                collision.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
                collision.gameObject.transform.SetParent(this.transform, true);

                //if (escapee.transform.position.x > transform.position.x)
                //{
                //    collision.gameObject.GetComponent<PlayerMove>().SetDetained(true, false);
                //    collision.gameObject.transform.position = new Vector2(transform.position.x + .5f, transform.position.y);
                //}
                //else
                //{
                //    collision.gameObject.GetComponent<PlayerMove>().SetDetained(true, true);
                //    collision.gameObject.transform.position = new Vector2(transform.position.x - .5f, transform.position.y);
                //}

                StartCoroutine(ReloadScene());
            }
        }

        if (gameObject.tag == "Runner")
        {
            if (collision.gameObject.tag == "Player")
            {
                audioSource = audioSources[3];
                audioSource.clip = audioClips3[Random.Range(0, audioClips3.Length)];
                audioSource.Play();

                if (runnerType == 0)
                {
                    ChangeAnimation("RunnerPush", 0);
                }
                else if (runnerType == 1)
                {
                    ChangeAnimation("Runner2Push", 0);
                }

                chaseTarget = null;
                rb.velocity = Vector2.zero;
                pushing = true;
                Invoke("Recover", .5f);  // You can modify the recovery time here if needed
                collision.gameObject.GetComponent<PlayerMove>().flinched = true;
                collision.gameObject.GetComponent<Rigidbody2D>().AddForce((collision.gameObject.transform.position - transform.position).normalized * 200);
                chaseTarget = escapee;
            }
        }
    }

    IEnumerator ReloadScene()
    {
        yield return new WaitForSeconds(4);
        SceneManager.LoadScene(0);
    }

    void ChangeAnimation(string animation, float crossfade)
    {
        if (currentAnim != animation)
        {
            animator.CrossFade(animation, crossfade);
        }
    }
}
