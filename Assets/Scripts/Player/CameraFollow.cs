using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // Glisse ton objet PLAYER ici dans l'inspecteur
    public Vector3 offset = new Vector3(0, 10, -7); // La distance (X, Y, Z)
    public Vector3 fixedRotation = new Vector3(45, 45, 0); // L'angle de vue bloqué

    void LateUpdate()
    {
        if (target != null)
        {
            // On force la position par rapport au joueur
            transform.position = target.position + offset;

            // On FORCE la rotation pour qu'elle ne bouge JAMAIS
            transform.rotation = Quaternion.Euler(fixedRotation);
        }
    }
}