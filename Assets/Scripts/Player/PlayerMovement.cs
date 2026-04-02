using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
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
        
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        // --- DÉTECTION DES TOUCHES (AZERTY) ---
        float moveX = 0;
        float moveZ = 0;

        moveZ = Input.GetAxis("Vertical");
        moveX = Input.GetAxis("Horizontal");
        

        Vector3 inputDirection = new Vector3(moveX, 0, moveZ).normalized;

        // --- GESTION DU DASH ---
        if (Input.GetKeyDown(KeyCode.Space) && cooldownTimer <= 0)
        {
            isDashing = true;
            dashTimer = dashDuration;
            cooldownTimer = dashCooldown;
            // Si on ne bouge pas, on dashe vers l'avant de l'objet
            dashDirection = inputDirection.magnitude > 0 ? inputDirection : transform.forward;
        }

        if (cooldownTimer > 0) 
            cooldownTimer -= Time.deltaTime;

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

        // 👉 IMPORTANT : on stocke juste
        currentVelocity = velocity;



        // OPTIONNEL : Faire pivoter le personnage vers la direction de marche
        if (inputDirection != Vector3.zero && !isDashing)
        {
            transform.forward = Vector3.Slerp(transform.forward, inputDirection, Time.deltaTime * 10f);
        }
    }
    private void FixedUpdate()
    {
        rb.linearVelocity = currentVelocity;
    }
}