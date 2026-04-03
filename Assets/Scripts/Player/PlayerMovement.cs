using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody rb;

    [Header("Dash Settings")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private float dashTimer;
    private float cooldownTimer;
    private Vector3 dashDirection;
    private bool isDashing;

    private Vector3 currentVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        // INPUT
        float moveZ = Input.GetAxis("Vertical");
        float moveX = Input.GetAxis("Horizontal");

        Vector3 inputDirection = new Vector3(moveX, 0, moveZ).normalized;

        // DASH
        if (Input.GetKeyDown(KeyCode.Space) && cooldownTimer <= 0)
        {
            isDashing = true;
            dashTimer = dashDuration;
            cooldownTimer = dashCooldown;
            dashDirection = inputDirection.magnitude > 0 ? inputDirection : transform.forward;
        }

        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        Vector3 velocity;

        if (isDashing && dashTimer > 0)
        {
            velocity = dashDirection * dashForce;
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0)
                isDashing = false;
        }
        else
        {
            velocity = inputDirection * speed;
        }

        currentVelocity = velocity;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = currentVelocity;
    }
}