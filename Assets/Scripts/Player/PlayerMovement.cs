using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 7f;
    private Rigidbody rb;

    [Header("Dash Settings")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Physique Custom")]
    public float gravityScale = 5f; // Augmente ça pour tomber plus vite !

    private float dashTimer;
    private float cooldownTimer;
    private Vector3 dashDirection;
    private bool isDashing;
    private Vector3 currentVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Bloque les rotations pour ne pas que le cube tombe
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.useGravity = true; // On laisse Unity gérer la base
    }

    void Update()
    {
        // GetAxisRaw = Arrêt INSTANTANÉ (pas de lissage)
        float moveZ = Input.GetAxisRaw("Vertical");
        float moveX = Input.GetAxisRaw("Horizontal");

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

        if (isDashing && dashTimer > 0)
        {
            currentVelocity = dashDirection * dashForce;
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0) isDashing = false;
        }
        else
        {
            currentVelocity = inputDirection * speed;
        }
    }

    void FixedUpdate()
    {
        // On applique une gravité bonus pour ne pas flotter
        rb.AddForce(Vector3.down * gravityScale, ForceMode.Acceleration);

        // On applique le mouvement X Z tout en gardant la vélocité Y du Rigidbody
        rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);
    }
}