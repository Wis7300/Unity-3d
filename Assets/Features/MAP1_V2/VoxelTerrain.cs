using System.Collections.Generic;
using UnityEngine;

public class VoxelTerrain : MonoBehaviour
{
    [Header("Configurations")]
    public Texture2D heightMap;
    public GameObject cubePrefab;
    public Transform player;

    [Header("Paramètres de Taille")]
    public int maxHeightInBlocks = 64;
    public int scaleMultiplier = 4;

    [Header("Optimisation & Distances")]
    public bool renderEntireMap = false;     // Si coché, affiche TOUTE la map d'un coup
    public int viewRadiusInBlocks = 40;

    [Header("Débogage Visuel")]
    public bool showColliderWireframe = true;

    private Dictionary<Vector3Int, GameObject> activeBlocks = new Dictionary<Vector3Int, GameObject>();
    private Vector3Int lastPlayerBlockPos;

    void Start()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null) player = foundPlayer.transform;
        }

        if (heightMap == null || cubePrefab == null || player == null) return;

        UpdateTerrain();
        lastPlayerBlockPos = GetPlayerBlockPos();
    }

    void Update()
    {
        // Si on affiche toute la carte, pas besoin de recalculer les distances à chaque frame !
        if (renderEntireMap || player == null) return;

        Vector3Int currentPlayerBlockPos = GetPlayerBlockPos();
        if (Mathf.Abs(currentPlayerBlockPos.x - lastPlayerBlockPos.x) >= 1 || Mathf.Abs(currentPlayerBlockPos.z - lastPlayerBlockPos.z) >= 1)
        {
            lastPlayerBlockPos = currentPlayerBlockPos;
            UpdateTerrain();
        }
    }

    Vector3Int GetPlayerBlockPos()
    {
        return new Vector3Int(Mathf.FloorToInt(player.position.x), 0, Mathf.FloorToInt(player.position.z));
    }

    void UpdateTerrain()
    {
        Vector3Int playerPos = GetPlayerBlockPos();
        HashSet<Vector3Int> blocksShouldBeActive = new HashSet<Vector3Int>();

        int maxWorldX = (heightMap.width - 1) * scaleMultiplier;
        int maxWorldZ = (heightMap.height - 1) * scaleMultiplier;

        bool terrainChanged = false;

        // Définition des bornes de boucle selon le mode choisi
        int startX = renderEntireMap ? 0 : playerPos.x - viewRadiusInBlocks;
        int endX = renderEntireMap ? maxWorldX : playerPos.x + viewRadiusInBlocks;
        int startZ = renderEntireMap ? 0 : playerPos.z - viewRadiusInBlocks;
        int endZ = renderEntireMap ? maxWorldZ : playerPos.z + viewRadiusInBlocks;

        for (int x = startX; x <= endX; x++)
        {
            for (int z = startZ; z <= endZ; z++)
            {
                if (x >= 0 && x <= maxWorldX && z >= 0 && z <= maxWorldZ)
                {
                    if (renderEntireMap || Vector2.Distance(new Vector2(playerPos.x, playerPos.z), new Vector2(x, z)) <= viewRadiusInBlocks)
                    {
                        float imageX = (float)x / scaleMultiplier;
                        float imageZ = (float)z / scaleMultiplier;

                        int x0 = Mathf.FloorToInt(imageX);
                        int x1 = Mathf.Clamp(x0 + 1, 0, heightMap.width - 1);
                        int z0 = Mathf.FloorToInt(imageZ);
                        int z1 = Mathf.Clamp(z0 + 1, 0, heightMap.height - 1);

                        float h0 = Mathf.Lerp(heightMap.GetPixel(x0, z0).grayscale, heightMap.GetPixel(x1, z0).grayscale, imageX - x0);
                        float h1 = Mathf.Lerp(heightMap.GetPixel(x0, z1).grayscale, heightMap.GetPixel(x1, z1).grayscale, imageX - x0);
                        float finalHeightValue = Mathf.Lerp(h0, h1, imageZ - z0);

                        int blockY = Mathf.RoundToInt(finalHeightValue * maxHeightInBlocks);

                        Vector3Int blockCoords = new Vector3Int(x, blockY, z);
                        blocksShouldBeActive.Add(blockCoords);

                        if (!activeBlocks.ContainsKey(blockCoords))
                        {
                            Vector3 worldPos = new Vector3(x, blockY, z);
                            GameObject newBlock = Instantiate(cubePrefab, worldPos, Quaternion.identity, transform);

                            // OPTIMISATION : On désactive le collider lourd individuel du cube
                            Collider blockCollider = newBlock.GetComponent<Collider>();
                            if (blockCollider != null) blockCollider.enabled = false;

                            activeBlocks.Add(blockCoords, newBlock);
                            terrainChanged = true;
                        }
                    }
                }
            }
        }

        List<Vector3Int> blocksToRemove = new List<Vector3Int>();
        foreach (var block in activeBlocks)
        {
            if (!blocksShouldBeActive.Contains(block.Key))
            {
                Destroy(block.Value);
                blocksToRemove.Add(block.Key);
                terrainChanged = true;
            }
        }
        foreach (var key in blocksToRemove) activeBlocks.Remove(key);

        if (terrainChanged)
        {
            BakeGlobalCollider();
        }
    }

    // Algorithme Greedy Meshing appliqué à des Box Colliders 2D horizontaux par couche de hauteur
    void BakeGlobalCollider()
    {
        // Nettoyage de l'ancien conteneur de colliders optimisés
        Transform oldContainer = transform.Find("Optimized_Colliders");
        if (oldContainer != null) Destroy(oldContainer.gameObject);

        if (activeBlocks.Count == 0) return;

        GameObject container = new GameObject("Optimized_Colliders");
        container.transform.SetParent(transform);
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;

        // Étape 1 : Regrouper les coordonnées X,Z par niveau de hauteur Y
        Dictionary<int, HashSet<Vector2Int>> layers = new Dictionary<int, HashSet<Vector2Int>>();
        foreach (var coord in activeBlocks.Keys)
        {
            if (!layers.ContainsKey(coord.y)) layers[coord.y] = new HashSet<Vector2Int>();
            layers[coord.y].Add(new Vector2Int(coord.x, coord.z));
        }

        // Étape 2 : Pour chaque hauteur, fusionner les blocs en grands rectangles
        foreach (var layer in layers)
        {
            int y = layer.Key;
            HashSet<Vector2Int> points = layer.Value;
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            foreach (var p in points)
            {
                if (visited.Contains(p)) continue;

                // Calculer la largeur maximale sur l'axe X
                int width = 1;
                while (points.Contains(new Vector2Int(p.x + width, p.y)) && !visited.Contains(new Vector2Int(p.x + width, p.y)))
                {
                    width++;
                }

                // Calculer la longueur maximale sur l'axe Z (p.y représente Z ici)
                int length = 1;
                bool canExpandZ = true;
                while (canExpandZ)
                {
                    for (int w = 0; w < width; w++)
                    {
                        Vector2Int checkPoint = new Vector2Int(p.x + w, p.y + length);
                        if (!points.Contains(checkPoint) || visited.Contains(checkPoint))
                        {
                            canExpandZ = false;
                            break;
                        }
                    }
                    if (canExpandZ) length++;
                }

                // Marquer les coordonnées de ce grand rectangle comme traitées
                for (int w = 0; w < width; w++)
                {
                    for (int l = 0; l < length; l++)
                    {
                        visited.Add(new Vector2Int(p.x + w, p.y + l));
                    }
                }

                // Étape 3 : Instancier le Box Collider unique et géant pour ce rectangle
                GameObject boxObj = new GameObject($"Collider_Layer_{y}_Rect");
                boxObj.transform.SetParent(container.transform);

                float centerX = p.x + (width - 1) * 0.5f;
                float centerZ = p.y + (length - 1) * 0.5f;
                boxObj.transform.localPosition = new Vector3(centerX, y, centerZ);

                BoxCollider bc = boxObj.AddComponent<BoxCollider>();
                bc.size = new Vector3(width, 1f, length);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showColliderWireframe) return;

        Transform container = transform.Find("Optimized_Colliders");
        if (container != null)
        {
            // Vert fluo très propre pour les contours extérieurs uniquement
            Gizmos.color = new Color(0f, 1f, 0f, 0.8f);

            foreach (Transform child in container)
            {
                BoxCollider bc = child.GetComponent<BoxCollider>();
                if (bc != null)
                {
                    // DrawWireCube dessine uniquement les arrêtes extérieures sans aucune diagonale !
                    Gizmos.DrawWireCube(child.position, bc.size);
                }
            }
        }
    }
}