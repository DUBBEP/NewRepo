using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonV1 : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;

    public float currentSpeed = 0f;
    float speedCap;
    public float walkSpeed = 15f;
    public float sprintSpeed = 30f;
    public float horizontalAcceleration = 0f;
    public float horizontalDeceleration = 0f;
    public float walkAcceleration = 60f;
    public float walkDeceleration = 80f;
    public float fallSpeed = -9.8f;
    public float fallMultiplyer = 2f;
    public float shortHopMultiplyer = 3f;
    public float gravity = -30f;
    public float jumpHeight = 4f;
    public float turnSmoothTime = 0.1f;
    public float bufferTime = 0.1f;
    public float coyoteTime = 0.1f;
    public float lastOnGroundTime;
    public float lastJumpTime;
    float turnSmoothVelocity;

    public bool shift;
    public bool noShift;

    public Transform groundCheck;
    float groundDistance = 0.15f;
    public LayerMask groundMask;

    public Transform upperCheck;
    float upperDistance = 0.3f;

    public Transform lowerCheck;
    float lowerDistance = 0.3f;
    
    bool isGrounded;
    bool onCorner;
    bool isJumping;
    Vector3 velocity;
    Vector3 lastDir;

    // Update is called once per frame
    void Update()
    {
        lastOnGroundTime -= Time.deltaTime;
        lastJumpTime -= Time.deltaTime;
        
        
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        onCorner = Physics.CheckSphere(lowerCheck.position, lowerDistance, groundMask) && !Physics.CheckSphere(upperCheck.position, upperDistance, groundMask);

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        
        //resets gravity when on the ground
        if (velocity.y < 0 && isGrounded)
        {
            velocity.y = -2f;
            fallSpeed = gravity;
        }
        //if the players is on the ground then set then set time to coyote time
        if (isGrounded)
        {
            lastOnGroundTime = coyoteTime;
            if (!Input.GetButton("Fire2"))
            {
                horizontalAcceleration = walkAcceleration;
                horizontalDeceleration = walkDeceleration;
                speedCap = walkSpeed;
            }
        } else
        {
            horizontalAcceleration = walkAcceleration / 1.5f;
            horizontalDeceleration = walkDeceleration / 3.5f;
        }

         //gravity function. subtracts y velocity every frame according to fall speed variable.
        velocity.y += fallSpeed * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        if (direction.magnitude >= 0.1f)
        {
            if (Input.GetButton("Fire2"))
            {
                speedCap = sprintSpeed;
            } else if (!Input.GetButton("Fire2") && speedCap > walkSpeed)
            {
                speedCap = walkSpeed;

            }
            if (currentSpeed < speedCap)
            {
                currentSpeed += horizontalAcceleration * Time.deltaTime;
            } else
            {
                currentSpeed -= horizontalDeceleration * Time.deltaTime;
            }

            lastDir.x = direction.x;
            lastDir.z = direction.z;
            
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f); 
            
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
        } else
        {
            if (currentSpeed > 0)
            {
                currentSpeed -= horizontalDeceleration * Time.deltaTime;

            float targetAngle = Mathf.Atan2(lastDir.x, lastDir.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f); 
            
            Vector3 stopLastDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(stopLastDir.normalized * currentSpeed * Time.deltaTime);
            } else
            {
                currentSpeed = 0;
            }
        }
        
        if (Input.GetButtonDown("Jump"))
        {
            lastJumpTime = bufferTime;
        }

        if (lastOnGroundTime > 0 && lastJumpTime > 0 && !isJumping)
        {
            jump();
        }
        void jump()
        {
            //set velocity for jump.
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * fallSpeed);
                isJumping = true;
                lastJumpTime = 0f;
                lastOnGroundTime = 0f;
        }
        //increases gravity while falling.
        if (velocity.y < 0 && !isGrounded)
        {
            fallSpeed = gravity * fallMultiplyer;
        } else if (velocity.y > 0 && !Input.GetButton("Jump")) //jump cut. if player releases jump they fall sooner.
        {
            fallSpeed = gravity * shortHopMultiplyer;
        }
        if (velocity.y < 0)
            {
                isJumping = false;
            }
        if (onCorner && isJumping)
        {
            transform.Translate(0f, 0.6f, 0f);
        }
    }
}