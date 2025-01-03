using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using UnityEditor;

#if UNITY_EDITOR
using UnityEditor.Animations;
using UnityEditor.Experimental.GraphView;
#endif

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField] GameObject guide;     // The guide object that will stay between the player and runner
    [SerializeField] GameObject runner;    // The runner object (target)
    [SerializeField] float speed = 5f;               // Movement speed
    [SerializeField] float skillCooldown = 10;
    [SerializeField] AudioClip[] audioClips;
    [SerializeField] GameObject skillIcon;

    private AudioSource audioSource;
    private Animator animator;
    private GameObject escapee;
    private GameManager gameManager;

    private Rigidbody2D rb;
    private Vector2 velocity;
    private float timer = 0;
    private bool runTimer = true;
    private bool detained = false;
    private bool skillOnCooldown = true;
    private bool wasGameOn = false;
    private AudioSource coinAudioSource;

    //TODO remove?
    private string currentAnim = "";

    public bool flinched = false;

    private float baseSpeed;
    private float startSpeed;

    public void SetDetained(bool detained) 
    { 
        if (detained)
        {
            audioSource.clip = audioClips[1];
            audioSource.Play();
            this.detained = detained;

            if(currentAnim != "Run")
            {
                animator.CrossFade("Run", 0.2f);
            }
        }
        
    }
    public bool GetDetainedStatus() { return detained; }

    void Awake()
    {
        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        escapee = FindObjectOfType<Escapee>().gameObject;
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        gameManager = FindObjectOfType<GameManager>();
        startSpeed = speed;
        baseSpeed = speed;
        ChangeAnimation("Idle", 0.2f);

        var audiosources = GetComponents<AudioSource>();
        coinAudioSource = audiosources[1];

        if (skillIcon == null)
        {
            GameObject HUDCanvas = GameObject.FindWithTag("HUDCanvas");

            skillIcon = HUDCanvas.transform.Find("ScreamIcon").gameObject;
        }
    }

    void Update()
    {
        print(rb.velocity.magnitude);

        // Check if gameIsOn has just transitioned from false to true
        if (gameManager.gameIsOn && !wasGameOn)
        {
            // Trigger the skill icon animation once when gameIsOn turns true
            skillIcon.GetComponent<Animator>().Play("ScreamIconRun");

            // Update the flag to indicate that the game is now active
            wasGameOn = true;
        }

        if (gameManager.gameIsOn)
        {
            if (!guide.activeInHierarchy)
            {
                guide.SetActive(true);
            }

            if (!detained && !flinched)
            {
                //Animations
                if (rb.velocity.magnitude > 0.1f)
                {
                    ChangeAnimation("Run", 0.2f);
                }

                else
                {
                    ChangeAnimation("Idle", 0.2f);
                }

                if (rb.velocity.x > 0)
                {
                    transform.localScale = new Vector3(-1, 1, 1);
                }

                else if (rb.velocity.x < 0)
                {
                    transform.localScale = new Vector3(1, 1, 1);
                }


                // Reset velocity each frame
                velocity = Vector2.zero;

                // Check input
                if (Input.GetKey(KeyCode.W)) // Move up
                    velocity.y += 1;
                if (Input.GetKey(KeyCode.S)) // Move down
                    velocity.y -= 1;
                if (Input.GetKey(KeyCode.D)) // Move right
                    velocity.x += 1;
                if (Input.GetKey(KeyCode.A)) // Move left
                    velocity.x -= 1;

                // Normalize the velocity if there's input to prevent diagonal speed boost
                if (velocity != Vector2.zero)
                {
                    velocity = velocity.normalized * speed;
                }

                // Update guide position (between player and runner, clamped within the camera view)
                UpdateGuidePosition();

                // Calculate direction vector from guide to runner for rotation
                Vector2 direction = runner.transform.position - guide.transform.position;

                // Get the angle in radians between the guide and the runner
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                // Apply the angle to the guide object (in 2D, we only rotate around the Z axis)
                guide.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

                // Check if the runner is in the camera view and hide/show the guide accordingly
                CheckIfRunnerIsVisible();

                if (runTimer)
                {
                    timer += Time.deltaTime;
                }

                if (timer > 9.5f)
                {
                    skillIcon.GetComponent<AudioSource>().Play();
                }

                if (timer >= skillCooldown)
                {
                    runTimer = false;
                    skillOnCooldown = false;
                    timer = 0;
                }

                if (Input.GetKeyDown(KeyCode.Space) && !skillOnCooldown)
                {
                    SpeedBoost();
                    SetSkillOnCooldown();


                    skillIcon.GetComponent<Animator>().Play("ScreamIconRun");

                    Collider2D[] runnersInRange = Physics2D.OverlapCircleAll(transform.position, 7);

                    audioSource.clip = audioClips[0];
                    audioSource.Play();

                    if (runnersInRange.Length <= 0)
                    {
                        print("No runners in range");
                        return;
                    }

                    foreach (Collider2D r in runnersInRange)
                    {
                        if (r.gameObject.GetComponent<Rigidbody2D>() != null && r.gameObject.GetComponent<Person>() != null)
                        {
                            // Vector from player to the escapee
                            Vector2 vectorToEscapee = escapee.transform.position - transform.position;

                            // Vector from player to the runner
                            Vector2 vectorToRunner = r.transform.position - transform.position;

                            // Cross product to determine side
                            float cross = vectorToEscapee.x * vectorToRunner.y - vectorToEscapee.y * vectorToRunner.x;

                            // Determine the perpendicular vector to push the runner
                            Vector2 perpendicularVectorToEscapee = (cross > 0)
                                ? new Vector2(-vectorToEscapee.y, vectorToEscapee.x) // Clockwise direction
                                : new Vector2(vectorToEscapee.y, -vectorToEscapee.x); // Counterclockwise direction

                            // Normalize and apply force
                            r.gameObject.GetComponent<Person>().SetStunnedStatus(true);
                            r.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                            r.gameObject.GetComponent<Rigidbody2D>().AddForce(perpendicularVectorToEscapee.normalized *
                                (5 - Vector2.Distance(r.transform.position, transform.position)) * 50);
                        }
                    }
                }

            }

            else if (flinched)
            {
                transform.GetChild(1).gameObject.SetActive(true);
                ChangeAnimation("Flinch", 0);
                Invoke("RecoverFromFlinch", .5f);
            }
        }

        else
        {
            //guide.SetActive(false);
            rb.velocity = Vector2.zero;
        }
    }

    void SpeedBoost()
    {
        speed = startSpeed * 1.4f;

        StartCoroutine(SlowDownBackToNormalSpeed());
    }

    IEnumerator SlowDownBackToNormalSpeed()
    {
        while (speed > startSpeed)
        {
            speed -= Time.deltaTime;
            yield return null;
        }
    }

    private void ChangeAnimation(string animation, float crossfade)
    {
        if (currentAnim != animation)
        {
            currentAnim = animation;
            animator.CrossFade(animation, crossfade);
        }
    }

    private void RecoverFromFlinch()
    {
        transform.GetChild(1).gameObject.SetActive(false);
        flinched = false;
    }

    void FixedUpdate()
    {

        if (!detained && !flinched && gameManager.gameIsOn)
        {
            // Apply the velocity to the Rigidbody2D
            rb.velocity = velocity;
        }

        else if (!gameManager.gameIsOn) 
        {
            rb.velocity = Vector2.zero;
        }
    }

    // Method to update the guide position between player and runner, clamped inside the camera view
    void UpdateGuidePosition()
    {
        // Calculate the direction from the player to the runner
        Vector2 direction = runner.transform.position - transform.position;
        direction.Normalize(); // Normalize the direction for consistent movement

        // Calculate the guide's position along the line between player and runner (relative position)
        // Increase the multiplier to place the guide farther from the player
        Vector2 guidePosition = (Vector2)transform.position + direction * 3f; // Guide will be 3 units between player and runner

        // Clamp the guide position to stay within the camera's viewport
        guidePosition = ClampPositionToCameraView(guidePosition);

        // Apply the clamped position to the guide
        guide.transform.position = guidePosition;
    }

    // Method to clamp the position inside the camera's viewport bounds
    Vector2 ClampPositionToCameraView(Vector2 position)
    {
        // Convert the world position to viewport space (0 to 1 range for x and y)
        Vector3 viewportPosition = Camera.main.WorldToViewportPoint(position);

        // Clamp the viewport position to stay within the 0 to 1 range for both X and Y axes
        viewportPosition.x = Mathf.Clamp(viewportPosition.x, 0f, 1f);
        viewportPosition.y = Mathf.Clamp(viewportPosition.y, 0f, 1f);

        // Convert the clamped viewport position back to world space
        return Camera.main.ViewportToWorldPoint(viewportPosition);
    }

    // Method to check if the runner is visible in the camera's view
    void CheckIfRunnerIsVisible()
    {
        // Get the runner's position in the viewport space (0 to 1 range for x and y)
        Vector3 runnerViewportPosition = Camera.main.WorldToViewportPoint(runner.transform.position);

        // If the runner is within the viewport (0 <= x <= 1, 0 <= y <= 1), the guide should be hidden
        if (runnerViewportPosition.x >= 0f && runnerViewportPosition.x <= 1.15f &&
            runnerViewportPosition.y >= 0f && runnerViewportPosition.y <= 1.15f)
        {
            // Runner is visible, so hide the guide
            guide.SetActive(false);
        }
        else
        {
            // Runner is not visible, so show the guide
            guide.SetActive(true);
        }
    }

    public void SetSkillOnCooldown()
    {
        skillOnCooldown = true;
        runTimer = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Escapee")
        {
            gameManager.gameIsOn = false;
            gameManager.SetCrowdNoises();

            // Determine the side of the collision
            float offsetX = (transform.position.x < collision.transform.position.x) ? -0.5f : 0.5f;

            // Set the new position
            transform.position = new Vector2(collision.transform.position.x + offsetX, collision.transform.position.y);

            // Flip this object to face the collided object
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * Mathf.Sign(offsetX), transform.localScale.y, transform.localScale.z);

            // Flip the collided object to face this object
            collision.transform.localScale = new Vector3(Mathf.Abs(collision.transform.localScale.x) * -Mathf.Sign(offsetX), collision.transform.localScale.y, collision.transform.localScale.z);

            animator.Play("Idle");
            collision.gameObject.GetComponent<Animator>().Play("EscIdle");
            collision.gameObject.GetComponent<Escapee>().EnableHeart();
            gameManager.HandleGameWin();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Note")
        {
            coinAudioSource.Play();
            gameManager.AddNote();
            Destroy(collision.gameObject);
            speed += .05f;
            startSpeed += .05f;
        }
    }

    public void ResetMoveSpeed()
    {
        speed = baseSpeed;
        startSpeed = baseSpeed;
    }
}
