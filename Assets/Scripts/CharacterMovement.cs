﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.PostProcessing;

public class CharacterMovement : MonoBehaviour
{
    public float maximumSpeed = 6f;
    public float sprintSpeedModifier = 1.5f;
    public float crouchSpeedModifier = 0.5f;
    public float acceleration = 50f;
    public float dampening = 30f;
    public float yDampening = 20f;
    public float gravity = 20f;
    public float jumpSpeed = 10f;

    public Vector3 velocity;
    private Vector3 acc;

    public bool inputDisabled = false;

    public float playerHeightStanding = 1.8f;
    public float playerHeightCrouching = 1.1f;
    public bool crouching;
    public bool sprinting;


    private CharacterController _controller;
    private Camera _camera;

    public float coyoteTime = 0.1f;
    private float _jumpCoyoteTimer;

    
    // Start is called before the first frame update
    void Start()
    {
        velocity = new Vector3();
        _controller = GetComponent<CharacterController>();
        _camera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        
        // Coyote Time
        if (_controller.isGrounded)
            _jumpCoyoteTimer = 0;
        _jumpCoyoteTimer += Time.deltaTime;

        float x=0, z=0;

        if (!_controller.isGrounded) {
            if (velocity.y > 0) {
                velocity.y -= yDampening * Time.deltaTime;
            }
        }

        var keyboard = Keyboard.current;
        if (keyboard != null) {
            if (keyboard.aKey.isPressed) { x -= 1; }
            if (keyboard.dKey.isPressed) { x += 1; }

            if (keyboard.wKey.isPressed) { z += 1; }
            if (keyboard.sKey.isPressed) { z -= 1; }

        }
        // Movement
        acc.x = 0;
        acc.y = 0;
        acc.z = 0;
        bool hasJumped = false;
        sprinting = false;
        bool sprint = false;
        if(!inputDisabled && keyboard != null) {

            sprint = keyboard.shiftKey.isPressed;
            if (_controller.isGrounded) {

                velocity.y = -gravity * 0.5f;
                DampenXZ();
                
                acc.x = x;
                acc.z = z;
                
                acc.Normalize();
                acc *= acceleration;
            } else {
                // Air control
                var localVelocity =  transform.worldToLocalMatrix.rotation * velocity;
                if((localVelocity.x * x + localVelocity.z * z)/(Mathf.Sqrt(localVelocity.x*localVelocity.x+localVelocity.z*localVelocity.z)*Mathf.Sqrt(x*x+z*z)) < -0.5f) {  // dot product
                    // Stop on backwards
                    DampenXZ();
                }
                acc.x = x;
                acc.z = z;
                
                acc.Normalize();
                acc *= acceleration * 0.05f;
            }
            
            if (keyboard.spaceKey.wasPressedThisFrame && !crouching) {
                if (_controller.isGrounded || _jumpCoyoteTimer <= coyoteTime) {
                    /*if (!_controller.isGrounded && _jumpCoyoteTimer <= coyoteTime)
                        Debug.Log("Coyote Jump!");
                    else {
                        Debug.Log("Normal Jump");
                    }*/
                    velocity.y = jumpSpeed;
                    if (z > 0.5 && sprint) {
                        Vector3 sprintJump = new Vector3(x, 0, z).normalized;
                        sprintJump *= maximumSpeed * sprintSpeedModifier * 2;
                        
                        velocity += transform.rotation * sprintJump;
                    }
                    hasJumped = true;
                } else {
                    // We are in the air
                }
            }
            
            // Crouching
            if (keyboard.leftCtrlKey.wasPressedThisFrame && !crouching) {
                crouching = true;
                _controller.height = playerHeightCrouching;
                _controller.center.Set(0, playerHeightCrouching / 2f, 0);
                //_camera.transform.localPosition = new Vector3(0,playerHeightCrouching - 0.1f,0);
            } else if ((keyboard.leftCtrlKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame || keyboard.shiftKey.wasPressedThisFrame) && crouching) {
                var couldStandUp = StandUp();
            }
        }
        if (inputDisabled && _controller.isGrounded) {
            DampenXZ();
        }
        // local acceleration to world velocity
        acc = transform.rotation * acc;
        velocity += acc * Time.deltaTime;
        
        // Handle max speed
        float speedSqrd = velocity.x * velocity.x + velocity.z * velocity.z;
        float maxSpeed;
        if (crouching)
            maxSpeed = maximumSpeed * crouchSpeedModifier;
        else if (sprint && !hasJumped)
            maxSpeed = maximumSpeed * sprintSpeedModifier;
        else if (sprint)
            maxSpeed = maximumSpeed * sprintSpeedModifier * 2;
        else
            maxSpeed = maximumSpeed;
        sprinting = sprint;
        float maxSpeedSqrd = maxSpeed * maxSpeed;
        if (speedSqrd > maxSpeedSqrd && (_controller.isGrounded || hasJumped))
        {
            velocity.x *= Mathf.Sqrt(maxSpeedSqrd / speedSqrd);
            velocity.z *= Mathf.Sqrt(maxSpeedSqrd / speedSqrd);
        }
        
        // Apply gravity
        velocity.y -= gravity * Time.deltaTime;
        
        // Move
        var flags = _controller.Move(velocity * Time.deltaTime);

        if ((flags & CollisionFlags.Above) != 0) {
            // head bump :(
            velocity.y = Mathf.Min(0, velocity.y);
        }
        
    }
    private bool StandUp() {

        // Raycast to check if we can stand up
        int terrainLayerMask = LayerMask.GetMask(new String[] {
            "Default"
        });
        bool hit = Physics.CheckCapsule(transform.position + new Vector3(0, playerHeightCrouching, 0), transform.position + new Vector3(0, playerHeightStanding, 0), _controller.radius, terrainLayerMask);
        if (!hit) {
            crouching = false;
            _controller.height = playerHeightStanding;
            _controller.center.Set(0, playerHeightStanding / 2f, 0);
            //_camera.transform.localPosition = new Vector3(0, playerHeightStanding - 0.1f, 0);
        }
        return !hit;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit) {
        if ((_controller.collisionFlags & CollisionFlags.Below) == 0) {
            var normal = hit.normal;
            velocity = Vector3.ProjectOnPlane(velocity, normal);
        }
    }

    private void DampenXZ() {
        Vector3 vXY = new Vector3(velocity.x, 0, velocity.z);
        if (vXY.magnitude == 0)
            return;
        Vector3 damp = vXY.normalized;
        damp.Normalize();
        damp *= -dampening * Time.deltaTime;
        if (damp.magnitude > vXY.magnitude) {
            vXY.x = 0;
            vXY.z = 0;
        } else {
            vXY += damp;
        }
        velocity.x = vXY.x;
        velocity.z = vXY.z;
    }
}