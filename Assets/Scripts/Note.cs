using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Note : MonoBehaviour
{
    [SerializeField] float speed = 2f;
    private Vector3 randomDirection; // Class-level variable

    // Start is called before the first frame update
    void Start()
    {
        float randomX = Random.Range(-1f, 1f);
        float randomY = Random.Range(-1f, 1f);

        // Avoid creating a zero vector
        while (randomX == 0 && randomY == 0)
        {
            randomX = Random.Range(-1f, 1f);
            randomY = Random.Range(-1f, 1f);
        }

        // Assign to the class-level randomDirection field
        randomDirection = new Vector3(randomX, randomY, 0).normalized; // Ensure it's a Vector3
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.parent == null || transform.parent.gameObject.tag != "Player")
        {
            transform.position += randomDirection * speed * Time.deltaTime;
        }

        // Get the parent's scale
        Vector3 parentScale = transform.parent.localScale;

        // Desired local scale of the child (constant scale regardless of parent)
        Vector3 desiredLocalScale = new Vector3(1, 1, 1); // Replace with your desired scale

        // Calculate the inverse of the parent's scale
        Vector3 inverseParentScale = new Vector3(1 / parentScale.x, 1 / parentScale.y, 1 / parentScale.z);

        // Apply the inverse scale to maintain the child's local scale
        transform.localScale = Vector3.Scale(desiredLocalScale, inverseParentScale);
    }
}
