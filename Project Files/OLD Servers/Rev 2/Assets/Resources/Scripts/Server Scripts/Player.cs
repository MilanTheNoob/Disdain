using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public CharacterController controller;
    public float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float jumpSpeed = 10f;
    public Vector3 oldPos;

    float horizontal;
    float vertical;
    bool jump;
    float yVelocity = 0;

    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }

    public void FixedUpdate()
    {
        Vector3 _moveDirection = transform.right * horizontal + transform.forward * vertical;
        _moveDirection *= moveSpeed;

        if (controller.isGrounded)
        {
            yVelocity = 0f;
            if (jump)
                yVelocity = jumpSpeed;
        }
        yVelocity += gravity;

        _moveDirection.y = yVelocity;
        controller.Move(_moveDirection);

        //ServerSend.PlayerPosition(this);
    }

    public void SetInput(float _horizontal, float _vertical, Vector3 _rotation, bool _jump)
    {
        horizontal = _horizontal;
        vertical = _vertical;
        jump = _jump;

        transform.eulerAngles = _rotation;
    }
}
