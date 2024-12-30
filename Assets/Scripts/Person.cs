using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class Person : MonoBehaviour
{
    [SerializeField] float runnerMinSpeed = 2;
    [SerializeField] float runnerMaxSpeed = 3.5f;
    [SerializeField] float chaserSpeed = 4;
    [SerializeField] float aggroDistance = 15;

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

    private bool pushing = false;
    public bool stunned = false;
    private float timer = 0;
    private float speed = 3;

    private float detectionRange = 15;
    private bool activeMode = true;

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
        audioSources = GetComponents<AudioSource>();

        player = FindObjectOfType<PlayerMove>().gameObject;
        escapee = FindObjectOfType<Escapee>().gameObject;
        rb = GetComponent<Rigidbody2D>();

        if (type == PersonType.Runner)
        {
            GetComponentInChildren<SpriteRenderer>().color = Color.yellow;
            gameObject.tag = "Runner";
            speed = Random.Range(runnerMinSpeed, runnerMaxSpeed);
            chaseTarget = escapee;
            audioSource = audioSources[2];
        }
        else if (type == PersonType.Chaser)
        {
            GetComponentInChildren<SpriteRenderer>().color = Color.black;
            gameObject.tag = "Chaser";
            speed = chaserSpeed;
            StartCoroutine(CopSounds());
        }
    }

    void FixedUpdate()
    {
        if (!stunned)
        {
            // Only allow movement if not pushing
            if (!pushing)
            {
                if (chaseTarget != null && Vector3.Distance(chaseTarget.transform.position, transform.position) < aggroDistance && activeMode)
                {
                    // Calculate direction towards the player
                    Vector2 direction = (chaseTarget.transform.position - transform.position).normalized;
                    // Set velocity of the Rigidbody2D
                    rb.velocity = direction * speed;
                    timer = 0;
                }
                else if (!activeMode)
                {
                    Vector2 direction = (transform.position - escapee.transform.position).normalized;
                    rb.velocity = direction * 3;
                    timer = 0;
                }
                else
                {
                    rb.velocity = Vector2.zero; // Stop moving if no target or outside aggro distance
                    timer += Time.fixedDeltaTime;
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

    private void Update()
    {
        if (type == PersonType.Chaser)
        {
            FindClosestRunner();
        }

        if (type == PersonType.Runner)
        {
            Collider2D[] playersInRange = Physics2D.OverlapCircleAll(transform.position, 2);

            foreach (Collider2D player in playersInRange)
            {
                if (player.gameObject.tag == "Player")
                {
                    chaseTarget = this.player;
                }
            }
        }

        if (timer > 3 || Vector2.Distance(player.transform.position, transform.position) > 25)
        {
            Destroy(gameObject);
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
        pushing = false;
        // You can also re-enable movement for the runner here
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (gameObject.tag == "Chaser")
        {
            if (collision.gameObject.tag == "Runner" && activeMode)
            {
                activeMode = false;
                GetComponent<Collider2D>().enabled = false;
                collision.collider.enabled = false;
                collision.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
                collision.gameObject.transform.SetParent(this.transform, true);
            }

            if (collision.gameObject.tag == "Player" && !collision.gameObject.GetComponent<PlayerMove>().GetDetainedStatus())
            {
                audioSources[0].Play();
                if (audioClips.Length > 0)
                {
                    audioSource = audioSources[1];
                    audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
                    audioSource.Play();
                }
                activeMode = false;
                GetComponent<Collider2D>().enabled = false;
                collision.collider.enabled = false;
                collision.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
                collision.gameObject.GetComponent<PlayerMove>().SetDetained(true);
                collision.gameObject.transform.SetParent(this.transform, true);
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
}
