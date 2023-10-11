using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraBlender : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private CinemachineVirtualCamera movementCamera;
    [SerializeField] private CinemachineVirtualCamera inventoryCamera;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void BlendToMovementCam()
    {
        movementCamera.Priority = 1;
        inventoryCamera.Priority = 0;

        animator.Play("Movement");
    }

    public void BlendToInventoryCam()
    {
        movementCamera.Priority = 0;
        inventoryCamera.Priority = 1;

        animator.Play("Inventory");
    }
}
