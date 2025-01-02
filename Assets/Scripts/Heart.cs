using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : MonoBehaviour
{
    private Animator animator;
    private void OnEnable()
    {
        // Find the Animator component
        animator = GetComponent<Animator>();

        // Play the animation when the object is activated
        if (animator != null)
        {
            animator.Play("EscHeartRun");
        }
        else
        {
            Debug.LogWarning("Animator component not found!");
        }
    }
}
