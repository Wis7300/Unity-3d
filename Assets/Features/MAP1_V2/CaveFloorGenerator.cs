using System.Collections.Generic;
using UnityEngine;

public class ImageToVoxelMap : MonoBehaviour
{
    [Header("Images de Référence")]
    public Texture2D mapMask;       // Pixil-frame-0.png
    public Texture2D heightGradient; // Gradient_100_100.png

    [Header("Paramètres de Hauteur")]
    public float maxGradientHeight = 4f;
    public float stepY = 0.25f;          // Pas du chemin
    public float wallHeight = 3f;        // Hauteur de base de la montagne (+3)
    public float mountainSlope = 0.5f;   // Élévation par bloc de distance

    [Header("Prefab Unique (Cube 1x1x1)")]
    public GameObject cubePrefab;
    public Material mapMaterial; // Matériau à appliquer au mesh final

    [ContextMenu("Générer la Carte depuis l'Image (Optimisé)")]
    public void GenerateMap()
    {
        // 1. Nettoyage rapide
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        if (mapMask == null || heightGradient == null || cubePrefab == null)
        {
            Debug.LogError("Vérifie que les images et le cubePrefab sont bien assignés !");
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

                float rawH = gradColor.grayscale * maxGradientHeight;
                rawPathHeight[x, z] = Mathf.Round(rawH / stepY) * stepY;

                if (maskColor.r > 0.5f)
                {
                    isPath[x, z] = true;
                    distanceToPath[x, z] = 0;
                    nearestPathHeight[x, z] = rawPathHeight[x, z];
                    queue.Enqueue(new Vector2Int(x, z));
                }
            }
        }

        // 3. Propagation BFS (Distances)
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

        // 4. Calcul des hauteurs finales
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                if (isPath[x, z])
                {
                    finalHeightGrid[x, z] = rawPathHeight[x, z];
                }
                else
                {
                    float baseMountainHeight = nearestPathHeight[x, z] + wallHeight;
                    float addedHeight = Mathf.Round(distanceToPath[x, z] * mountainSlope);
                    finalHeightGrid[x, z] = baseMountainHeight + addedHeight;
                }
            }
        }

        // 5. Génération directe en mémoire (SANS Instantiate ni Destroy)
        Mesh cubeMesh = cubePrefab.GetComponent<MeshFilter>().sharedMesh;
        List<CombineInstance> combineList = new List<CombineInstance>();

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                float targetY = finalHeightGrid[x, z];

                // A. Bloc du dessus (Surface)
                CombineInstance topBlock = new CombineInstance();
                topBlock.mesh = cubeMesh;
                topBlock.transform = Matrix4x4.TRS(new Vector3(x, targetY, z), Quaternion.identity, Vector3.one);
                combineList.Add(topBlock);

                // B. Recherche du voisin le plus bas
                float minNeighborY = targetY;
                foreach (Vector2Int dir in directions)
                {
                    int nx = x + dir.x;
                    int nz = z + dir.y;
                    if (nx >= 0 && nx < width && nz >= 0 && nz < length)
                    {
                        if (finalHeightGrid[nx, nz] < minNeighborY) minNeighborY = finalHeightGrid[nx, nz];
                    }
                }

                // C. Génération des blocs verticaux UNIQUEMENT si un trou est visible
                for (float fillY = targetY - stepY; fillY >= minNeighborY; fillY -= stepY)
                {
                    CombineInstance fillBlock = new CombineInstance();
                    fillBlock.mesh = cubeMesh;
                    fillBlock.transform = Matrix4x4.TRS(new Vector3(x, fillY, z), Quaternion.identity, Vector3.one);
                    combineList.Add(fillBlock);
                }
            }
        }

        // 6. Application directe du Mesh final
        GameObject combinedMap = new GameObject("GeneratedVoxelMap");
        combinedMap.transform.parent = transform;

        MeshFilter finalMeshFilter = combinedMap.AddComponent<MeshFilter>();
        MeshRenderer finalMeshRenderer = combinedMap.AddComponent<MeshRenderer>();
        MeshCollider finalCollider = combinedMap.AddComponent<MeshCollider>();

        finalMeshFilter.sharedMesh = new Mesh();
        finalMeshFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        finalMeshFilter.sharedMesh.CombineMeshes(combineList.ToArray(), true, true);

        finalMeshRenderer.sharedMaterial = mapMaterial != null ? mapMaterial : cubePrefab.GetComponent<MeshRenderer>().sharedMaterial;
        finalCollider.sharedMesh = finalMeshFilter.sharedMesh;
    }
}