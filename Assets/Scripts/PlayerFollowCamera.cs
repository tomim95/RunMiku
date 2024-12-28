using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollowCamera : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] float cameraSpeed = 3;

    // Start is called before the first frame update
    void Start()
    {
        if (player == null)
        {
            player = FindObjectOfType<PlayerMove>().gameObject;
        }
    }

    // FixedUpdate is called at a consistent interval
    void FixedUpdate()
    {
        if (player != null)
        {
            // Smoothly follow the player using Lerp in FixedUpdate
            transform.position = Vector3.Lerp(
                transform.position,
                new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z),
                Time.fixedDeltaTime * cameraSpeed
            );
        }
    }
}
