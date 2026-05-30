using UnityEngine;

public class PlayerMovementView
{
    private Rigidbody2D rigidBody;

    public PlayerMovementView(Rigidbody2D rigidBody)
    {
        this.rigidBody = rigidBody;

        rigidBody.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    public void Move(float moveX, float moveSpeed)
    {
        rigidBody.linearVelocity = new Vector2(moveX * moveSpeed, rigidBody.linearVelocity.y);
    }
}