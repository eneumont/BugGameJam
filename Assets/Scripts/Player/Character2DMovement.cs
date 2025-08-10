using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Character2DMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private float moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Ensure kinematic mode
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
    }

    private void Update()
    {
        // Get input
        moveInput = Input.GetAxisRaw("Horizontal"); // -1, 0, 1

        // Update animator
        animator.SetFloat("Speed", Mathf.Abs(moveInput)); // Use Speed in Animator
    }

    private void FixedUpdate()
    {
        // Kinematic move
        Vector2 movement = new Vector2(moveInput * moveSpeed, 0f);
        rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);

        // Flip character sprite based on movement direction
        if (moveInput != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(moveInput) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }
}
