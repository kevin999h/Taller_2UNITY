using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private bool isTopDown = false;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;
    private bool facingRight = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = isTopDown ? Input.GetAxisRaw("Vertical") : 0f;
        moveInput = new Vector2(horizontal, vertical).normalized;

        if (horizontal > 0 && !facingRight) Flip();
        else if (horizontal < 0 && facingRight) Flip();

        float currentSpeed = moveInput.sqrMagnitude;
        if (animator != null)
        {
            animator.SetFloat("Speed", currentSpeed);
        }
    }

    private void FixedUpdate()
    {
        if (isTopDown)
        {
            rb.velocity = moveInput * moveSpeed;
        }
        else
        {
            rb.velocity = new Vector2(moveInput.x * moveSpeed, rb.velocity.y);
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
