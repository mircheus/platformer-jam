using UnityEngine;

public class PlayerMovementModel
{
    private Vector2 moveInput;

    private float moveSpeed;
    private float jumpForce;
    private bool isGrounded;

    public PlayerMovementModel(float moveSpeed, float jumpForce)
    {
        this.moveSpeed = moveSpeed;
        this.jumpForce = jumpForce;
    }

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }
    public float GetJumpForce()
    {
        return jumpForce;
    }
}