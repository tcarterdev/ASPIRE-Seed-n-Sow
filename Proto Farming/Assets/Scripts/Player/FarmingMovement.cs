using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FarmingMovement  : MonoBehaviour
{
    [SerializeField] private float currentSpeed;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private bool canMove = true;
    [SerializeField] private ParticleSystem moveparticle; 


    private CharacterController characterController;
    private Transform cam;

    private float turnSmoothTime = 0.1f;
    
    private PlayerInput playerInput;
    private PlayerAnimation playerAnimation;

    

    
    void Awake()
    {
        cam = Camera.main.transform;
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        playerAnimation = GetComponent<PlayerAnimation>();
    }

    void Update()
    {
        if (canMove == false) { return; }

        Vector2 input = playerInput.actions["Move"].ReadValue<Vector2>();

        if (input.magnitude >= 0.1f)
        {
            playerAnimation.playerMoving = true;
            
            
            if (currentSpeed < maxSpeed)
            {
                currentSpeed += acceleration * Time.deltaTime;
                
            }
        
            
            float targetAngle = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothTime, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
        

            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            characterController.Move(moveDir * currentSpeed * Time.deltaTime);
        }
        else if (currentSpeed > 0)
        {
            
            currentSpeed -= deceleration * Time.deltaTime;
            playerAnimation.playerMoving = false;
            
            
        }
        else
        {
            playerAnimation.playerMoving = false;
            
        }
        
        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);
    }

    
}