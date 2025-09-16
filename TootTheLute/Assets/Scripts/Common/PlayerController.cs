using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f; // Configurable movement speed

    private Vector2 moveInput; // Stores movement input
    private Rigidbody2D rb; // Reference to the Rigidbody2D component

    public event Action OnAttack; // Event triggered on left mouse button press
    public Health health;
    public float PDamageTaken;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed) // Fires event when left mouse button is pressed
        {
            OnAttack?.Invoke();
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {

        if(collision.gameObject.layer == LayerMask.NameToLayer("Enemy")
            || collision.gameObject.layer == LayerMask.NameToLayer("EnemyAttack")
            )
        {
            //health.DirectlyIncrement(PDamageTaken);

        }




    }

    private void FixedUpdate()
    {
        Vector2 movement = moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);
    }
}
