using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Person : MonoBehaviour
{
    [SerializeField] float minSpeed = 1;
    [SerializeField] float maxSpeed = 3;
    [SerializeField] float aggroDistance = 15;

    private GameObject player;
    private Rigidbody2D rb;

    private bool stunned = false;

    void Start()
    {
        player = FindObjectOfType<PlayerMove>().gameObject;
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (player != null && !stunned && Vector3.Distance(player.transform.position, transform.position) < aggroDistance)
        {
            // Calculate direction towards the player
            Vector2 direction = (player.transform.position - transform.position).normalized;
            // Set velocity of the Rigidbody2D
            rb.velocity = direction * Random.Range(minSpeed, maxSpeed);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Vector3.Distance(player.transform.position, transform.position) < aggroDistance)
        {
            stunned = true;
            rb.AddForce((transform.position - player.transform.position).normalized * (aggroDistance - Vector3.Distance(transform.position, player.transform.position)) * 30);
            Invoke("Recover", 2);
        }
    }

    private void Recover()
    {
        stunned = false;
        rb.velocity = Vector3.zero;
    }
}
