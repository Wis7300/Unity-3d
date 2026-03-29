using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float dashForce = 5f;

    private Rigidbody rb;
    private Vector3 movement;

    public float dashCooldown = 1f;
    private float dashCooldownTimer = 0f;
    private bool isDashing = false;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        movement = new Vector3(horizontal, 0f, vertical).normalized;

        if (Input.GetKeyDown(KeyCode.Space) && dashCooldownTimer <= 0f)
        {
            if (movement != Vector3.zero)
            {
                movement.y = 0f;
                rb.AddForce(movement * dashForce, ForceMode.Impulse);
                dashCooldownTimer = dashCooldown;
                FindFirstObjectByType<CameraFollow>().TriggerDashEffect();
            }
        }

    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            rb.linearVelocity = new Vector3(
            movement.x * moveSpeed,
            rb.linearVelocity.y,
            movement.z * moveSpeed);
        }

        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

    }
}