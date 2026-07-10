using UnityEngine;
using UnityEngine.InputSystem;

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
    public float stepHeight = 1.1f;       // Hauteur max d'une marche (1 bloc = 1m)
    public float stepSmooth = 0.2f;       // Force de levée initiale
    public float detectionDistance = 0.45f; // Distance de détection devant le joueur
    public float footSpacing = 0.3f;       // Écartement horizontal des capteurs (largeur des pieds)

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
        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;
        }

        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            moveX += stick.x;
            moveZ += stick.y;
        }

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

        bool dashPressed = false;
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) dashPressed = true;
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame) dashPressed = true;

        if (dashPressed && cooldownTimer <= 0)
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

        if (isDashing)
        {
            float secureY = rb.linearVelocity.y;
            if (secureY > 0f) secureY = 0f;
            rb.linearVelocity = new Vector3(currentVelocity.x, secureY, currentVelocity.z);
        }
        else
        {
            rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);

            if (inputDirection.magnitude > 0.05f)
            {
                StepClimb();
            }
        }
    }

    // MISE À JOUR : Gestion par double capteur (Pied Gauche / Pied Droit)
    void StepClimb()
    {
        Vector3 moveDir = inputDirection.normalized;

        // Calcul du vecteur perpendiculaire pour décaler les rayons à gauche et à droite
        Vector3 sideDir = Vector3.Cross(moveDir, Vector3.up).normalized;
        Vector3 leftOffset = sideDir * (footSpacing * 0.5f);
        Vector3 rightOffset = -sideDir * (footSpacing * 0.5f);

        // Positions des capteurs pour le côté gauche
        Vector3 leftRayLower = transform.position + leftOffset + new Vector3(0f, 0.1f, 0f);
        Vector3 leftRayUpper = transform.position + leftOffset + new Vector3(0f, stepHeight, 0f);

        // Positions des capteurs pour le côté droit
        Vector3 rightRayLower = transform.position + rightOffset + new Vector3(0f, 0.1f, 0f);
        Vector3 rightRayUpper = transform.position + rightOffset + new Vector3(0f, stepHeight, 0f);

        bool shouldClimb = false;

        // 1. Analyse du côté Gauche
        if (Physics.Raycast(leftRayLower, moveDir, out RaycastHit hitLeftLower, detectionDistance))
        {
            if (hitLeftLower.collider.gameObject != gameObject && !hitLeftLower.collider.isTrigger)
            {
                // Si le bas touche mais que le haut est libre, on peut monter
                if (!Physics.Raycast(leftRayUpper, moveDir, detectionDistance))
                {
                    shouldClimb = true;
                }
            }
        }

        // 2. Analyse du côté Droit (si le côté gauche n'a rien validé)
        if (!shouldClimb && Physics.Raycast(rightRayLower, moveDir, out RaycastHit hitRightLower, detectionDistance))
        {
            if (hitRightLower.collider.gameObject != gameObject && !hitRightLower.collider.isTrigger)
            {
                // Si le bas touche mais que le haut est libre, on peut monter
                if (!Physics.Raycast(rightRayUpper, moveDir, detectionDistance))
                {
                    shouldClimb = true;
                }
            }
        }

        // 3. Application de la montée si un des deux côtés a détecté une marche valide
        if (shouldClimb)
        {
            rb.position += new Vector3(0f, stepSmooth, 0f);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 1.8f, rb.linearVelocity.z);
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && inputDirection.magnitude > 0.05f && !isDashing)
        {
            Vector3 moveDir = inputDirection.normalized;
            Vector3 sideDir = Vector3.Cross(moveDir, Vector3.up).normalized;
            Vector3 leftOffset = sideDir * (footSpacing * 0.5f);
            Vector3 rightOffset = -sideDir * (footSpacing * 0.5f);

            // Gizmos Pied Gauche
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position + leftOffset + new Vector3(0f, 0.1f, 0f), moveDir * detectionDistance);
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position + leftOffset + new Vector3(0f, stepHeight, 0f), moveDir * detectionDistance);

            // Gizmos Pied Droit
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position + rightOffset + new Vector3(0f, 0.1f, 0f), moveDir * detectionDistance);
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(transform.position + rightOffset + new Vector3(0f, stepHeight, 0f), moveDir * detectionDistance);
        }
    }
}