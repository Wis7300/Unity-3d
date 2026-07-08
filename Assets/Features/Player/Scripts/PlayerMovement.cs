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

        // Lancer la détection de marche automatique
        if (inputDirection.magnitude > 0.05f)
        {
            StepClimb();
        }
    }

    // Système optimisé pour enjamber automatiquement les blocs de 1m
    // Système de Step Climb instantané et ultra réactif
    void StepClimb()
    {
        // Raycast bas : Détecte l'obstacle devant les pieds
        RaycastHit hitLower;
        Vector3 rayLowerPos = transform.position + new Vector3(0, 0.05f, 0); // Légèrement abaissé pour une détection maximale

        // On projette le rayon un poil plus loin (0.75f) pour anticiper la collision
        if (Physics.Raycast(rayLowerPos, inputDirection, out hitLower, 0.75f))
        {
            // Raycast haut : Vérifie s'il y a de la place au-dessus pour monter
            RaycastHit hitUpper;
            Vector3 rayUpperPos = transform.position + new Vector3(0, stepHeight, 0);

            if (!Physics.Raycast(rayUpperPos, inputDirection, out hitUpper, 0.85f))
            {
                // FORCE LA MONTÉE IMMÉDIATE : On téléporte le Rigidbody juste au-dessus de la marche
                // stepSmooth est ici utilisé comme un multiplicateur d'impulsion verticale
                rb.position += new Vector3(0, 0.25f, 0);

                // On conserve une légère impulsion vers l'avant pour ne pas tuer l'élan du joueur
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 2f, rb.linearVelocity.z);
            }
        }
    }
}