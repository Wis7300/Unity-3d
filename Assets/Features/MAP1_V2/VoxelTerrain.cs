using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class VoxelTerrain : MonoBehaviour
{
    [Header("Configurations")]
    public Texture2D heightMap;
    public GameObject cubePrefab;
    public Transform player;

    [Header("Paramètres de Taille")]
    public int maxHeightInBlocks = 64;
    public int scaleMultiplier = 4;

    [Header("Optimisation")]
    public int viewRadiusInBlocks = 40;

    private Dictionary<Vector3Int, GameObject> activeBlocks = new Dictionary<Vector3Int, GameObject>();
    private Vector3Int lastPlayerBlockPos;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

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
        if (player == null) return;

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

        for (int x = playerPos.x - viewRadiusInBlocks; x <= playerPos.x + viewRadiusInBlocks; x++)
        {
            for (int z = playerPos.z - viewRadiusInBlocks; z <= playerPos.z + viewRadiusInBlocks; z++)
            {
                if (x >= 0 && x <= maxWorldX && z >= 0 && z <= maxWorldZ)
                {
                    if (Vector2.Distance(new Vector2(playerPos.x, playerPos.z), new Vector2(x, z)) <= viewRadiusInBlocks)
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

        // Si des blocs sont apparus ou ont disparu, on recalcule la hitbox globale fusionnée
        if (terrainChanged)
        {
            BakeGlobalCollider();
        }
    }
    [Header("Débogage Visuel")]
    public bool showColliderWireframe = true; // Case à cocher dans l'Inspecteur

    // Fusionne tous les rendus des cubes en un seul maillage physique lisse et unique
    void BakeGlobalCollider()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        if (meshFilters.Length <= 1) return;

        List<CombineInstance> combineList = new List<CombineInstance>();

        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].gameObject == gameObject) continue;
            if (meshFilters[i].sharedMesh == null) continue;

            CombineInstance c = new CombineInstance();
            c.mesh = meshFilters[i].sharedMesh;
            c.transform = transform.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;

            combineList.Add(c);
        }

        if (combineList.Count == 0) return;

        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        // TRUC PHYSIQUE : fusionner les sous-maillages en une seule topologie (true, true)
        combinedMesh.CombineMeshes(combineList.ToArray(), true, true);

        // Nettoie et soude virtuellement les normales pour supprimer les collisions fantômes sur les arêtes internes
        combinedMesh.RecalculateBounds();
        combinedMesh.RecalculateNormals();
        combinedMesh.Optimize();

        meshFilter.sharedMesh = combinedMesh;
        meshCollider.sharedMesh = combinedMesh;
    }

    // Dessine uniquement les contours du Mesh Collider en vert fluo si activé
    private void OnDrawGizmos()
    {
        if (!showColliderWireframe) return;

        MeshCollider collider = GetComponent<MeshCollider>();

        if (collider != null && collider.sharedMesh != null)
        {
            // Vert fluo bien visible pour les lignes
            Gizmos.color = new Color(0f, 1f, 0f, 0.8f);

            Gizmos.DrawWireMesh(
                collider.sharedMesh,
                transform.position,
                transform.rotation,
                transform.localScale
            );
        }
    }
}