using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 7f;
    private Rigidbody rb;
    private Camera cam;

    [Header("Dash Settings")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Physique Custom")]
    public float gravityScale = 5f;

    private float dashTimer;
    private float cooldownTimer;
    private Vector3 dashDirection;
    private bool isDashing;
    private Vector3 currentVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main; // On stocke la caméra pour éviter de la chercher à chaque frame


        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.useGravity = true;
    }

    void Update()
    {
        // Inputs (Z, Q, S, D ou flèches)
        float moveZ = Input.GetAxisRaw("Vertical");
        float moveX = Input.GetAxisRaw("Horizontal");

        // --- CALCUL DE LA DIRECTION RELATIVE À LA CAMÉRA ---
        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

        // On ignore la hauteur de la caméra pour rester au sol
        forward.y = 0;
        right.y = 0;
        forward = forward.normalized;
        right = right.normalized;

        // Direction finale basée sur ce qu'on voit à l'écran
        Vector3 inputDirection = (forward * moveZ + right * moveX).normalized;

        // --- LOGIQUE DU DASH ---
        if (Input.GetKeyDown(KeyCode.Space) && cooldownTimer <= 0)
        {
            isDashing = true;
            dashTimer = dashDuration;
            cooldownTimer = dashCooldown;

            // Si on ne bouge pas, on dash vers l'avant du personnage, sinon vers l'input
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

        // --- ROTATION DU PERSONNAGE (Optionnel mais recommandé) ---
        // Oriente le cube vers la direction du mouvement
        if (inputDirection.magnitude > 0 && !isDashing)
        {
            transform.forward = inputDirection;
        }
    }

    void FixedUpdate()
    {
        // Gravité personnalisée
        rb.AddForce(Vector3.down * gravityScale, ForceMode.Acceleration);

        // Application du mouvement
        // On garde la vélocité Y actuelle (chute) et on applique X/Z
        rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);
    }
}