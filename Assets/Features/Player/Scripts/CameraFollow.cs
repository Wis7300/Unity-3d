using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public LayerMask obstacleLayer; // Assigne ici le Layer de tes blocs (ex: "Ground")

    [Header("Position & Angle")]
    public float pitch = 45f;       // L'inclinaison vers le bas (hauteur de vue)
    public float rotationSpeed = 0.5f;

    [Header("Zoom (Roulette)")]
    public float currentDistance = 10f;
    public float minDistance = 3f;
    public float maxDistance = 25f;
    public float zoomSpeed = 2f;

    [Header("Collision")]
    public float cameraRadius = 0.5f; // Épaisseur de la caméra pour éviter de voir à travers les murs

    private float currentYaw = 45f; // Rotation horizontale actuelle

    void LateUpdate()
    {
        if (target == null) return;

        // 1. Rotation autour du joueur (Maintien du Clic Droit + Mouvement Souris)
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            currentYaw += Mouse.current.delta.x.ReadValue() * rotationSpeed;
        }

        // 2. Zoom (Roulette de la souris)
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.y.ReadValue();
            if (scroll != 0)
            {
                currentDistance -= Mathf.Sign(scroll) * zoomSpeed;
                currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            }
        }

        // 3. Calcul de la rotation bloquée en inclinaison (Pitch) mais libre en rotation (Yaw)
        Quaternion rotation = Quaternion.Euler(pitch, currentYaw, 0);
        Vector3 direction = rotation * Vector3.back;

        // 4. Gestion des Collisions (Raycast depuis la tête du joueur vers la caméra)
        float finalDistance = currentDistance;
        Vector3 rayOrigin = target.position + Vector3.up * 1f; // On lance le rayon un peu au-dessus des pieds

        if (Physics.SphereCast(rayOrigin, cameraRadius, direction, out RaycastHit hit, currentDistance, obstacleLayer))
        {
            // Si on touche un bloc, on raccourcit la distance de la caméra
            finalDistance = hit.distance;
        }

        // 5. Application finale
        transform.position = rayOrigin + direction * finalDistance;
        transform.rotation = rotation;
    }
}