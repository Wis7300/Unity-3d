using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 7f;
    private Rigidbody rb;
    private Camera cam;
    private Animator anim;

    [Header("Dash Settings")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Physique Custom")]
    public float gravityScale = 5f;

    [Header("Smooth Settings")]
    public float rotationSpeed = 10f;

    [Header("Système de Marches Voxel")]
    public float stepHeight = 1.1f;       // Hauteur max d'une marche que le joueur peut monter (1 bloc = 1m)
    public float stepSmooth = 0.2f;       // Vitesse de montée de la marche

    private float dashTimer;
    private float cooldownTimer;
    private Vector3 dashDirection;
    private bool isDashing;
    private Vector3 currentVelocity;
    private Vector3 inputDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
        anim = GetComponent<Animator>();

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

        inputDirection = (forward * moveZ + right * moveX).normalized;

        if (anim != null)
        {
            bool isMoving = inputDirection.magnitude > 0.05f;
            anim.SetBool("isWalking", isMoving);
        }

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

        if (inputDirection.magnitude > 0 && !isDashing)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            Quaternion correctedRotation = targetRotation * Quaternion.Euler(0, 180, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, correctedRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        rb.AddForce(Vector3.down * gravityScale, ForceMode.Acceleration);
        rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);

        // Lancer la détection de marche automatique basé sur la direction réelle
        if (inputDirection.magnitude > 0.05f)
        {
            StepClimb();
        }
    }

    // Version volumétrique élargie pour mieux capter les blocs en face
    void StepClimb()
    {
        Vector3 moveDir = inputDirection.normalized;

        // Nouvelle taille : 0.7m de large/profond et 0.4m de haut pour englober une plus grande zone
        // On l'avance à 0.45m pour détecter le bloc une fraction de seconde plus tôt
        Vector3 detectionCenter = transform.position + (moveDir * 0.45f) + new Vector3(0f, 0.25f, 0f);
        Vector3 boxHalfExtents = new Vector3(0.35f, 0.2f, 0.35f);

        Collider[] hitColliders = Physics.OverlapBox(detectionCenter, boxHalfExtents, Quaternion.identity);

        bool blockDetectedAtFeet = false;
        foreach (var col in hitColliders)
        {
            if (col.gameObject != gameObject && !col.isTrigger)
            {
                blockDetectedAtFeet = true;
                break;
            }
        }

        if (blockDetectedAtFeet)
        {
            Vector3 rayUpperPos = transform.position + new Vector3(0f, stepHeight, 0f);

            if (!Physics.Raycast(rayUpperPos, moveDir, out RaycastHit hitUpper, 0.9f))
            {
                rb.position += new Vector3(0f, stepSmooth, 0f);
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 2.5f, rb.linearVelocity.z);
            }
        }
    }

    // Dessine le nouveau cube bleu agrandi dans l'éditeur
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && inputDirection.magnitude > 0.05f)
        {
            Gizmos.color = Color.cyan;
            Vector3 detectionCenter = transform.position + (inputDirection.normalized * 0.45f) + new Vector3(0f, 0.25f, 0f);
            // Affichage de la boîte aux nouvelles dimensions (0.7m x 0.4m x 0.7m)
            Gizmos.DrawWireCube(detectionCenter, new Vector3(0.7f, 0.4f, 0.7f));
        }
    }
}