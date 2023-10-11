using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private AudioSource playerWalking;
    private Animator animator;
    public bool playerMoving;

    private void Awake()
    {
        playerWalking = GameObject.FindGameObjectWithTag("WalkingSFX").GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    private void LateUpdate()
    {
        if (playerMoving)
        {
            animator.SetBool("moving", true);
            if(!playerWalking.isPlaying)
            {
                playerWalking.Play();
            }
        }
        else
        {
            animator.SetBool("moving", false);
            if(playerWalking.isPlaying)
            {
                playerWalking.Stop();
            }
        }
    }

}
