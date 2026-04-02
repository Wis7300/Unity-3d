using UnityEngine;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody rb;

    [Header("Dash Settings")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Camera Smooth Effect")]
    public Camera mainCamera;
    public float dashFOVMultiplier = 1.2f; // Jusqu'où le FOV augmente (ex: 1.2 = +20%)
    public float smoothSpeed = 5f;         // Vitesse du retour à la normale
    private float targetFOV;
    private float defaultFOV;

    private float dashTimer;
    private float cooldownTimer;
    private Vector3 dashDirection;
    private bool isDashing;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (mainCamera != null)
        {
            defaultFOV = mainCamera.fieldOfView;
            targetFOV = defaultFOV;
        }
    }

    void Update()
    {
        // --- DÉTECTION DES TOUCHES (Z-S-Q-D) ---
        float moveX = 0;
        float moveZ = 0;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Z)) moveZ = 1;
        if (Input.GetKey(KeyCode.S)) moveZ = -1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.Q)) moveX = -1;
        if (Input.GetKey(KeyCode.D)) moveX = 1;

        Vector3 inputDirection = new Vector3(moveX, 0, moveZ).normalized;

        // --- GESTION DU DASH ---
        if (Input.GetKeyDown(KeyCode.Space) && cooldownTimer <= 0)
        {
            isDashing = true;
            dashTimer = dashDuration;
            cooldownTimer = dashCooldown;
            dashDirection = inputDirection.magnitude > 0 ? inputDirection : transform.forward;

            // On définit la cible du FOV (effet de zoom arrière/vitesse)
            targetFOV = defaultFOV * dashFOVMultiplier;
        }

        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        // --- CALCUL DU MOUVEMENT ---
        Vector3 currentMove;
        if (isDashing && dashTimer > 0)
        {
            currentMove = dashDirection * dashForce;
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0)
            {
                isDashing = false;
                targetFOV = defaultFOV; // On revient au FOV de base
            }
        }
        else
        {
            currentMove = inputDirection * speed;
            targetFOV = defaultFOV; // Sécurité pour revenir au FOV de base
        }

        // --- APPLICATION DU SMOOTH CAMERA ---
        if (mainCamera != null)
        {
            // Lerp permet une transition fluide entre le FOV actuel et la cible
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * smoothSpeed);
        }

        rb.MovePosition(rb.position + currentMove * Time.deltaTime);
    }
}