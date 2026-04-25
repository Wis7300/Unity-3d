using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 7f;
    private Rigidbody rb;
    private Camera cam;
    private Animator anim; // Référence pour l'animation

    [Header("Dash Settings")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Physique Custom")]
    public float gravityScale = 5f;

    [Header("Smooth Settings")]
    public float rotationSpeed = 10f; // Vitesse de rotation (plus c'est haut, plus c'est réactif)

    private float dashTimer;
    private float cooldownTimer;
    private Vector3 dashDirection;
    private bool isDashing;
    private Vector3 currentVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
        anim = GetComponent<Animator>(); // On récupère l'Animator au lancement

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.useGravity = true;
    }

    void Update()
    {
        float moveZ = Input.GetAxisRaw("Vertical");
        float moveX = Input.GetAxisRaw("Horizontal");

        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

        forward.y = 0;
        right.y = 0;
        forward = forward.normalized;
        right = right.normalized;

        Vector3 inputDirection = (forward * moveZ + right * moveX).normalized;

        // --- GESTION DE L'ANIMATION ---
        if (anim != null)
        {
            // On utilise une valeur très basse pour être sûr de capter le moindre mouvement
            bool isMoving = inputDirection.magnitude > 0.05f;
            anim.SetBool("isWalking", isMoving);
        }

        // --- LOGIQUE DU DASH ---
        if (Input.GetKeyDown(KeyCode.Space) && cooldownTimer <= 0)
        {
            isDashing = true;
            dashTimer = dashDuration;
            cooldownTimer = dashCooldown;
            dashDirection = inputDirection.magnitude > 0 ? inputDirection : -transform.forward;
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

        // --- ROTATION SMOOTH ---
        if (inputDirection.magnitude > 0 && !isDashing)
        {
            // On calcule la direction vers laquelle on veut aller
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);

            // On ajoute un pivot de 180 degrés pour compenser le modèle inversé
            Quaternion correctedRotation = targetRotation * Quaternion.Euler(0, 180, 0);

            // On tourne progressivement vers cette rotation corrigée
            transform.rotation = Quaternion.Slerp(transform.rotation, correctedRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        rb.AddForce(Vector3.down * gravityScale, ForceMode.Acceleration);
        rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);
    }
}
