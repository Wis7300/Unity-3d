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
    public float stepHeight = 1.1f;       // Hauteur max d'une marche que le joueur peut monter (1 bloc = 1m)
    public float stepSmooth = 0.2f;       // Force de levée initiale ajustée

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
        rb.linearVelocity = new Vector3(currentVelocity.x, rb.linearVelocity.y, currentVelocity.z);

        if (inputDirection.magnitude > 0.05f)
        {
            StepClimb();
        }
    }

    void StepClimb()
    {
        Vector3 moveDir = inputDirection.normalized;

        // CORRECTION : On monte le centre du cube à 0.4m (au lieu de 0.15m) pour ne plus détecter le sol plat sous nos pieds
        Vector3 detectionCenter = transform.position + (moveDir * 0.45f) + new Vector3(0f, 0.4f, 0f);
        Vector3 boxHalfExtents = new Vector3(0.35f, 0.15f, 0.35f);

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
                // CORRECTION : On applique une poussée verticale plus douce (1.8f au lieu de 3.5f) pour glisser sur le bloc au lieu de sauter sauvagement
                rb.position += new Vector3(0f, stepSmooth, 0f);
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 1.8f, rb.linearVelocity.z);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && inputDirection.magnitude > 0.05f)
        {
            Gizmos.color = Color.cyan;
            // Ajustement visuel du Gizmo pour correspondre à la nouvelle boîte de détection rehaussée
            Vector3 detectionCenter = transform.position + (inputDirection.normalized * 0.45f) + new Vector3(0f, 0.4f, 0f);
            Gizmos.DrawWireCube(detectionCenter, new Vector3(0.7f, 0.3f, 0.7f));
        }
    }
}