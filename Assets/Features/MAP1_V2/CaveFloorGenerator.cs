using System.Collections.Generic;
using UnityEngine;

public class ImageToVoxelMap : MonoBehaviour
{
    [Header("Images de Référence")]
    public Texture2D mapMask;           // Pixil-frame-0.png (Chemin)
    public Texture2D heightModifierMap; // Taches: Noir (+0.1), Blanc (-0.1), Gris (0)

    [Header("Paramètres de Hauteur")]
    public float wallHeight = 3f;        // Hauteur de base de la montagne (+3)
    public float mountainSlope = 0.5f;   // Élévation par bloc de distance

    [Header("Rendu et Optimisation")]
    public Material mapMaterial;
    public bool showHitboxes = false;    // Coche ça pour voir les hitboxes en vert

    // Variables internes pour le mesh de collision (Hitbox)
    private Mesh collisionMesh;

    [ContextMenu("Générer la Carte depuis l'Image (Ultra-Optimisé)")]
    public void GenerateMap()
    {
        // 1. Nettoyage
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        if (mapMask == null || heightModifierMap == null)
        {
            Debug.LogError("Vérifie que les images mapMask et heightModifierMap sont assignées !");
            return;
        }

        int width = mapMask.width;
        int length = mapMask.height;

        bool[,] isPath = new bool[width, length];
        float[,] finalHeightGrid = new float[width, length];
        int[,] distanceToPath = new int[width, length];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        // 2. Lecture des textures et calcul des modificateurs
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                distanceToPath[x, z] = 999999;

                // Noir (0) = +0.1 | Blanc (1) = -0.1 | Gris (0.5) = 0
                float gray = heightModifierMap.GetPixel(x, z).grayscale;
                float modifier = (0.5f - gray) * 0.2f; // Donne exactement entre -0.1 et +0.1

                if (mapMask.GetPixel(x, z).r > 0.5f)
                {
                    isPath[x, z] = true;
                    distanceToPath[x, z] = 0;
                    finalHeightGrid[x, z] = modifier; // Hauteur du chemin = modificateur
                    queue.Enqueue(new Vector2Int(x, z));
                }
                else
                {
                    finalHeightGrid[x, z] = modifier; // On stocke temporairement le modificateur
                }
            }
        }

        // 3. Propagation BFS pour les distances
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            foreach (Vector2Int dir in dirs)
            {
                Vector2Int neighbor = current + dir;
                if (neighbor.x >= 0 && neighbor.x < width && neighbor.y >= 0 && neighbor.y < length)
                {
                    if (distanceToPath[current.x, current.y] + 1 < distanceToPath[neighbor.x, neighbor.y])
                    {
                        distanceToPath[neighbor.x, neighbor.y] = distanceToPath[current.x, current.y] + 1;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        // 4. Calcul des hauteurs finales de la montagne
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                if (!isPath[x, z])
                {
                    float addedHeight = Mathf.Round(distanceToPath[x, z] * mountainSlope);
                    finalHeightGrid[x, z] += wallHeight + addedHeight;
                }
            }
        }

        // 5. Génération procédurale du Mesh (Faces visibles uniquement)
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // Listes séparées pour la Hitbox (Faces du dessus uniquement)
        List<Vector3> colVerts = new List<Vector3>();
        List<int> colTris = new List<int>();

        float s = 0.5f; // Demi-taille du bloc

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                float h = finalHeightGrid[x, z];

                // --- FACE DU DESSUS (Visuel + Hitbox) ---
                Vector3 p0 = new Vector3(x - s, h, z - s);
                Vector3 p1 = new Vector3(x - s, h, z + s);
                Vector3 p2 = new Vector3(x + s, h, z + s);
                Vector3 p3 = new Vector3(x + s, h, z - s);

                AddQuad(p0, p1, p2, p3, verts, tris, uvs);
                AddQuad(p0, p1, p2, p3, colVerts, colTris, null); // Hitbox optimisée !

                // --- FACES LATÉRALES (Visuel uniquement, pas de hitbox) ---
                // Nord (+z)
                if (z == length - 1 || finalHeightGrid[x, z + 1] < h)
                    AddQuad(p2, p1, new Vector3(x - s, GetNeighborH(x, z + 1, width, length, finalHeightGrid), z + s), new Vector3(x + s, GetNeighborH(x, z + 1, width, length, finalHeightGrid), z + s), verts, tris, uvs);

                // Sud (-z)
                if (z == 0 || finalHeightGrid[x, z - 1] < h)
                    AddQuad(p0, p3, new Vector3(x + s, GetNeighborH(x, z - 1, width, length, finalHeightGrid), z - s), new Vector3(x - s, GetNeighborH(x, z - 1, width, length, finalHeightGrid), z - s), verts, tris, uvs);

                // Est (+x)
                if (x == width - 1 || finalHeightGrid[x + 1, z] < h)
                    AddQuad(p3, p2, new Vector3(x + s, GetNeighborH(x + 1, z, width, length, finalHeightGrid), z + s), new Vector3(x + s, GetNeighborH(x + 1, z, width, length, finalHeightGrid), z - s), verts, tris, uvs);

                // Ouest (-x)
                if (x == 0 || finalHeightGrid[x - 1, z] < h)
                    AddQuad(p1, p0, new Vector3(x - s, GetNeighborH(x - 1, z, width, length, finalHeightGrid), z - s), new Vector3(x - s, GetNeighborH(x - 1, z, width, length, finalHeightGrid), z + s), verts, tris, uvs);
            }
        }

        // 6. Création des objets et assignation des Meshes
        GameObject mapObj = new GameObject("GeneratedVoxelMap");
        mapObj.transform.parent = transform;
        mapObj.transform.localPosition = Vector3.zero;

        // Visuel
        Mesh visualMesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        visualMesh.SetVertices(verts);
        visualMesh.SetTriangles(tris, 0);
        visualMesh.SetUVs(0, uvs);
        visualMesh.RecalculateNormals();

        mapObj.AddComponent<MeshFilter>().sharedMesh = visualMesh;
        mapObj.AddComponent<MeshRenderer>().sharedMaterial = mapMaterial;

        // Collision optimisée
        collisionMesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        collisionMesh.SetVertices(colVerts);
        collisionMesh.SetTriangles(colTris, 0);

        MeshCollider collider = mapObj.AddComponent<MeshCollider>();
        collider.sharedMesh = collisionMesh;

        Debug.Log($"Carte générée ! Visuel: {verts.Count} vertices | Hitbox optimisée: {colVerts.Count} vertices.");
    }

    // Helper pour récupérer la hauteur d'un voisin (ou 0 si on est au bord de la map)
    private float GetNeighborH(int x, int z, int w, int l, float[,] grid)
    {
        if (x < 0 || x >= w || z < 0 || z >= l) return 0f; // Bords de la map tombent à Y=0
        return grid[x, z];
    }

    // Helper pour générer un carré (Quad) rapidement
    private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        int index = vertices.Count;
        vertices.Add(v1); vertices.Add(v2); vertices.Add(v3); vertices.Add(v4);

        triangles.Add(index); triangles.Add(index + 1); triangles.Add(index + 2);
        triangles.Add(index); triangles.Add(index + 2); triangles.Add(index + 3);

        if (uvs != null)
        {
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(1, 0));
        }
    }

    // 7. Affichage des hitboxes en vert dans l'éditeur
    private void OnDrawGizmos()
    {
        if (showHitboxes && collisionMesh != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.4f); // Vert transparent
            Gizmos.matrix = transform.localToWorldMatrix;

            // Dessine la hitbox (qui ne contient QUE les faces du dessus)
            Gizmos.DrawWireMesh(collisionMesh);
        }
    }
}