using BossRoom;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossRoomPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("Combat")]
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public LayerMask enemyLayers;
    public float attackCooldown = 0.5f;

    [Header("Health")]
    public int maxHealth = 3;

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isGrounded;
    private bool facingRight = true;
    private float lastAttackTime;
    private int currentHealth;

    // Triple-tap input tracking
    private float lastLeftTap = -1f;
    private float lastRightTap = -1f;
    private int leftTapCount = 0;
    private int rightTapCount = 0;
    private readonly float tapWindow = 0.3f;

    // Force facing direction set by triple-tap
    private bool forceFacingActive = false;
    private bool forcedFacingRight = true;

    // Systems
    private InputDesyncSystem inputDesync;
    private GoalGaslightingSystem gaslighting;

    public bool beatBoss = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;

        inputDesync = FindObjectOfType<InputDesyncSystem>();
        gaslighting = FindObjectOfType<GoalGaslightingSystem>();

        if (attackPoint == null)
        {
            GameObject attackObj = new GameObject("AttackPoint");
            attackObj.transform.SetParent(transform);
            attackObj.transform.localPosition = new Vector3(1.5f, 0, 0);
            attackPoint = attackObj.transform;
        }

        if (groundCheckPoint == null)
        {
            GameObject gc = new GameObject("GroundCheckPoint");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0f, -0.5f, 0);
            groundCheckPoint = gc.transform;
        }
    }

    void Update()
    {
        UpdateGrounded();
        HandleTripleTap();
        HandleMovement();
        HandleAttack();
        UpdateAnimations();
    }

    void UpdateGrounded()
    {
        // More forgiving ground check
        isGrounded = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckDistance, groundLayer)
            || Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckDistance * 0.75f, groundLayer);
    }

    void HandleTripleTap()
    {
        if (inputDesync != null && inputDesync.IsSwapped)
            return;

        float currentTime = Time.time;

        // Left tap
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentTime - lastLeftTap > tapWindow)
                leftTapCount = 0;

            leftTapCount++;
            lastLeftTap = currentTime;

            if (leftTapCount >= 3)
            {
                SetForcedFacing(false);
                leftTapCount = 0;
            }
        }

        // Right tap
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentTime - lastRightTap > tapWindow)
                rightTapCount = 0;

            rightTapCount++;
            lastRightTap = currentTime;

            if (rightTapCount >= 3)
            {
                SetForcedFacing(true);
                rightTapCount = 0;
            }
        }
    }

    public void SetForcedFacing(bool right)
    {
        if (forceFacingActive && forcedFacingRight == right)
            return;

        forcedFacingRight = right;
        forceFacingActive = true;

        if (facingRight != right)
            Flip();

        Debug.Log($"Force facing {(right ? "right" : "left")} scheduled due to triple tap");
    }

    void HandleMovement()
    {
        float moveInput = 0f;
        bool jumpInput = false;

        if (inputDesync == null || inputDesync.ShouldProcessInputImmediately(KeyCode.A))
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                moveInput -= 1f;
        }

        if (inputDesync == null || inputDesync.ShouldProcessInputImmediately(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                moveInput += 1f;
        }

        if (inputDesync == null || inputDesync.ShouldProcessInputImmediately(KeyCode.Space))
        {
            jumpInput = Input.GetKeyDown(KeyCode.Space);
        }

        // Horizontal movement
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Jump
        if (jumpInput && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // Reset vertical speed for consistent jumps
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            anim?.SetTrigger("Jump");
        }

        // Facing
        if (!forceFacingActive)
        {
            if (moveInput > 0 && !facingRight) Flip();
            else if (moveInput < 0 && facingRight) Flip();
        }
    }

    void HandleAttack()
    {
        bool attackInput = false;

        if (inputDesync == null || inputDesync.ShouldProcessInputImmediately(KeyCode.Mouse0))
            attackInput = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.X);

        if (attackInput && Time.time - lastAttackTime >= attackCooldown)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }

    void PerformAttack()
    {
        anim?.SetTrigger("Attack");

        var hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (var enemy in hitEnemies)
        {
            BossController boss = enemy.GetComponent<BossController>();
            if (boss != null)
            {
                if (gaslighting != null && gaslighting.ShouldPreventDamage())
                {
                    gaslighting.ShowFakeDamage();
                    return;
                }
                boss.TakeDamage(1f);
                Debug.Log("Hit boss!");
            }
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0f, 180f, 0f);

        var pos = attackPoint.localPosition;
        pos.x = -pos.x;
        attackPoint.localPosition = pos;
    }

    void UpdateAnimations()
    {
        if (anim == null) return;

        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
        anim.SetBool("isMoving", isMoving);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        gaslighting?.UpdatePlayerHearts(currentHealth, maxHealth);

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        Debug.Log("Player died!");
        var setupManager = FindObjectOfType<BossRoom.BossRoomSetupManager>();
        setupManager?.OnPlayerDeath();
    }

    public bool IsFacingRight() => facingRight;
    public int GetCurrentHealth() => currentHealth;

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        gaslighting?.UpdatePlayerHearts(currentHealth, maxHealth);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(groundCheckPoint.position, groundCheckPoint.position + Vector3.down * groundCheckDistance);
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckDistance * 0.75f);
        }
    }
}
