using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    public float speed = 5f; // Movement speed
    private Rigidbody2D rb;
    private Vector2 velocity;

    void Awake()
    {
        // Get the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
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
    }

    void FixedUpdate()
    {
        // Apply the velocity to the Rigidbody2D
        rb.velocity = velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        print("collision happened");
    }
}


