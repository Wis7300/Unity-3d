using System.Collections.Generic;
using UnityEngine;

public class ImageToVoxelMap : MonoBehaviour
{
    [Header("Images de Référence")]
    public Texture2D mapMask;       // Pixil-frame-0.png
    public Texture2D heightGradient; // Gradient_100_100.png

    [Header("Paramètres de Hauteur")]
    public float maxGradientHeight = 4f; // Élévation maximale du dégradé
    public float stepY = 0.25f;          // Grille de hauteur (0.25)
    public float wallHeight = 3f;        // Hauteur de base de la montagne (+3)
    public float mountainSlope = 0.5f;   // Pente de la montagne par bloc de distance

    [Header("Prefab Unique (Cube 1x1x1)")]
    public GameObject cubePrefab;

    [ContextMenu("Générer la Carte depuis l'Image")]
    public void GenerateMap()
    {
        // 1. Nettoyage de la scène
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        if (mapMask == null || heightGradient == null)
        {
            Debug.LogError("Il manque l'image de masque ou de dégradé !");
            return;
        }

        int width = mapMask.width;
        int length = mapMask.height;

        bool[,] isPath = new bool[width, length];
        float[,] rawPathHeight = new float[width, length];
        int[,] distanceToPath = new int[width, length];
        float[,] nearestPathHeight = new float[width, length];
        float[,] finalHeightGrid = new float[width, length];

        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        // 2. Lecture des textures
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                distanceToPath[x, z] = 999999;

                Color maskColor = mapMask.GetPixel(x, z);
                Color gradColor = heightGradient.GetPixel(x, z);

                // Hauteur du dégradé lissée par pas de 0.25
                float rawH = gradColor.grayscale * maxGradientHeight;
                rawPathHeight[x, z] = Mathf.Round(rawH / stepY) * stepY;

                // Si le pixel est blanc (chemin)
                if (maskColor.r > 0.5f)
                {
                    isPath[x, z] = true;
                    distanceToPath[x, z] = 0;
                    nearestPathHeight[x, z] = rawPathHeight[x, z];
                    queue.Enqueue(new Vector2Int(x, z));
                }
            }
        }

        // 3. Propagation BFS pour calculer les distances au chemin
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighbor = current + dir;

                if (neighbor.x >= 0 && neighbor.x < width && neighbor.y >= 0 && neighbor.y < length)
                {
                    if (distanceToPath[current.x, current.y] + 1 < distanceToPath[neighbor.x, neighbor.y])
                    {
                        distanceToPath[neighbor.x, neighbor.y] = distanceToPath[current.x, current.y] + 1;
                        nearestPathHeight[neighbor.x, neighbor.y] = nearestPathHeight[current.x, current.y];
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        // 4. Calcul de la grille des hauteurs finales
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                if (isPath[x, z])
                {
                    // Le chemin garde sa variation douce tous les 0.25
                    finalHeightGrid[x, z] = rawPathHeight[x, z];
                }
                else
                {
                    // La base de la montagne se cale sur le chemin (+ 3 blocs)
                    float baseMountainHeight = nearestPathHeight[x, z] + wallHeight;

                    // L'élévation due à l'éloignement se fait par blocs entiers (sauts de 1)
                    float addedHeight = Mathf.Round(distanceToPath[x, z] * mountainSlope);

                    finalHeightGrid[x, z] = baseMountainHeight + addedHeight;
                }
            }
        }

        // 5. Instanciation des cubes 1x1x1
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                float targetY = finalHeightGrid[x, z];

                // Pose du cube de surface
                Instantiate(cubePrefab, new Vector3(x, targetY, z), Quaternion.identity, transform);

                // Comblement des vides verticaux sous le bloc (si la marche avec le voisin est haute)
                float minNeighborY = targetY;
                foreach (Vector2Int dir in directions)
                {
                    int nx = x + dir.x;
                    int nz = z + dir.y;
                    if (nx >= 0 && nx < width && nz >= 0 && nz < length)
                    {
                        if (finalHeightGrid[nx, nz] < minNeighborY)
                        {
                            minNeighborY = finalHeightGrid[nx, nz];
                        }
                    }
                }

                // Si un voisin est plus bas, on remplit verticalement avec le cube 1x1 pour boucher le trou
                for (float fillY = targetY - 1f; fillY >= minNeighborY; fillY -= 1f)
                {
                    Instantiate(cubePrefab, new Vector3(x, fillY, z), Quaternion.identity, transform);
                }
            }
        }
    }
}