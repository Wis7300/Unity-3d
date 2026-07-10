using System.Collections;
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

    [Header("Système de Chunks")]
    [Tooltip("Nombre de chunks affichés autour du joueur")]
    public int chunkViewDistance = 4;
    public const int ChunkSize = 16;

    [Header("Débogage Visuel")]
    public bool showColliderWireframe = true;

    private Dictionary<Vector2Int, ChunkData> activeChunks = new Dictionary<Vector2Int, ChunkData>();
    private Vector2Int lastPlayerChunkPos = new Vector2Int(-999, -999);
    private bool isUpdatingTerrain = false;
    private Rigidbody playerRb; // Référence pour geler le joueur au démarrage

    private class ChunkData
    {
        public GameObject chunkContainer;
        public Dictionary<Vector3Int, GameObject> blocks;
        public GameObject colliderContainer;

        public ChunkData(GameObject container)
        {
            chunkContainer = container;
            blocks = new Dictionary<Vector3Int, GameObject>();
        }
    }

    void Start()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null) player = foundPlayer.transform;
        }

        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody>();
        }

        if (heightMap == null || cubePrefab == null || player == null) return;

        // On lance la première génération forcée
        CheckPlayerChunkPosition(true);
    }

    void Update()
    {
        if (player == null || isUpdatingTerrain) return;

        CheckPlayerChunkPosition(false);
    }

    void CheckPlayerChunkPosition(bool forceUpdate)
    {
        int playerChunkX = Mathf.FloorToInt(player.position.x / ChunkSize);
        int playerChunkZ = Mathf.FloorToInt(player.position.z / ChunkSize);
        Vector2Int currentChunkPos = new Vector2Int(playerChunkX, playerChunkZ);

        if (forceUpdate || currentChunkPos != lastPlayerChunkPos)
        {
            lastPlayerChunkPos = currentChunkPos;
            StartCoroutine(UpdateChunksCoroutine(currentChunkPos, forceUpdate));
        }
    }

    IEnumerator UpdateChunksCoroutine(Vector2Int playerChunkGrid, bool isFirstLoad)
    {
        isUpdatingTerrain = true;

        // SÉCURITÉ : Si c'est le tout premier chargement au Start(), on fige le joueur
        if (isFirstLoad && playerRb != null)
        {
            playerRb.isKinematic = true;
        }

        int maxWorldX = (heightMap.width - 1) * scaleMultiplier;
        int maxWorldZ = (heightMap.height - 1) * scaleMultiplier;
        int maxChunkX = maxWorldX / ChunkSize;
        int maxChunkZ = maxWorldZ / ChunkSize;

        HashSet<Vector2Int> chunksShouldBeActive = new HashSet<Vector2Int>();

        for (int x = playerChunkGrid.x - chunkViewDistance; x <= playerChunkGrid.x + chunkViewDistance; x++)
        {
            for (int z = playerChunkGrid.y - chunkViewDistance; z <= playerChunkGrid.y + chunkViewDistance; z++)
            {
                if (x >= 0 && x <= maxChunkX && z >= 0 && z <= maxChunkZ)
                {
                    chunksShouldBeActive.Add(new Vector2Int(x, z));
                }
            }
        }

        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunk in activeChunks)
        {
            if (!chunksShouldBeActive.Contains(chunk.Key))
            {
                Destroy(chunk.Value.chunkContainer);
                chunksToRemove.Add(chunk.Key);
            }
        }
        foreach (var key in chunksToRemove) activeChunks.Remove(key);

        foreach (Vector2Int chunkPos in chunksShouldBeActive)
        {
            if (!activeChunks.ContainsKey(chunkPos))
            {
                GenerateChunk(chunkPos, maxWorldX, maxWorldZ);
                yield return null;
            }
        }

        // SÉCURITÉ : Les chunks initiaux sont faits, on libère le joueur !
        if (isFirstLoad && playerRb != null)
        {
            playerRb.isKinematic = false;
        }

        isUpdatingTerrain = false;
    }

    int GetSmoothedHeightAt(int x, int z, int maxWorldX, int maxWorldZ)
    {
        if (x < 0 || x > maxWorldX || z < 0 || z > maxWorldZ) return 0;

        float imageX = (float)x / scaleMultiplier;
        float imageZ = (float)z / scaleMultiplier;

        int x0 = Mathf.FloorToInt(imageX);
        int x1 = Mathf.Clamp(x0 + 1, 0, heightMap.width - 1);
        int z0 = Mathf.FloorToInt(imageZ);
        int z1 = Mathf.Clamp(z0 + 1, 0, heightMap.height - 1);

        float tX = imageX - x0;
        float tZ = imageZ - z0;

        tX = tX * tX * (3f - 2f * tX);
        tZ = tZ * tZ * (3f - 2f * tZ);

        float h0 = Mathf.Lerp(heightMap.GetPixel(x0, z0).grayscale, heightMap.GetPixel(x1, z0).grayscale, tX);
        float h1 = Mathf.Lerp(heightMap.GetPixel(x0, z1).grayscale, heightMap.GetPixel(x1, z1).grayscale, tX);
        float finalHeightValue = Mathf.Lerp(h0, h1, tZ);

        return Mathf.RoundToInt(finalHeightValue * maxHeightInBlocks);
    }

    void GenerateChunk(Vector2Int chunkGridPos, int maxWorldX, int maxWorldZ)
    {
        GameObject chunkObj = new GameObject($"Chunk_{chunkGridPos.x}_{chunkGridPos.y}");
        chunkObj.transform.SetParent(transform);
        chunkObj.transform.localPosition = Vector3.zero;

        ChunkData chunkData = new ChunkData(chunkObj);

        int startX = chunkGridPos.x * ChunkSize;
        int startZ = chunkGridPos.y * ChunkSize;

        for (int x = startX; x < startX + ChunkSize; x++)
        {
            for (int z = startZ; z < startZ + ChunkSize; z++)
            {
                if (x > maxWorldX || z > maxWorldZ) continue;

                int surfaceY = GetSmoothedHeightAt(x, z, maxWorldX, maxWorldZ);

                int hLeft = GetSmoothedHeightAt(x - 1, z, maxWorldX, maxWorldZ);
                int hRight = GetSmoothedHeightAt(x + 1, z, maxWorldX, maxWorldZ);
                int hDown = GetSmoothedHeightAt(x, z - 1, maxWorldX, maxWorldZ);
                int hUp = GetSmoothedHeightAt(x, z + 1, maxWorldX, maxWorldZ);

                int lowestNeighbor = surfaceY;
                if (x > 0) lowestNeighbor = Mathf.Min(lowestNeighbor, hLeft);
                if (x < maxWorldX) lowestNeighbor = Mathf.Min(lowestNeighbor, hRight);
                if (z > 0) lowestNeighbor = Mathf.Min(lowestNeighbor, hDown);
                if (z < maxWorldZ) lowestNeighbor = Mathf.Min(lowestNeighbor, hUp);

                int minY = lowestNeighbor;

                for (int currentY = surfaceY; currentY >= minY; currentY--)
                {
                    Vector3Int blockCoords = new Vector3Int(x, currentY, z);

                    if (!chunkData.blocks.ContainsKey(blockCoords))
                    {
                        Vector3 worldPos = new Vector3(x, currentY, z);
                        GameObject newBlock = Instantiate(cubePrefab, worldPos, Quaternion.identity, chunkObj.transform);

                        Collider blockCollider = newBlock.GetComponent<Collider>();
                        if (blockCollider != null) blockCollider.enabled = false;

                        chunkData.blocks.Add(blockCoords, newBlock);
                    }
                }
            }
        }

        // --- NOUVEAU : NETTOYAGE DES FACES INUTILES ---
        foreach (var kvp in chunkData.blocks)
        {
            Vector3Int localPos = kvp.Key;
            GameObject blockObj = kvp.Value;

            // Règle N°1 : La face du bas est TOUJOURS cachée (vue de dessus)
            Transform bottomFace = blockObj.transform.Find("Bottom");
            if (bottomFace != null) bottomFace.gameObject.SetActive(false);

            // Règle N°2 : Si un voisin existe dans le dictionnaire, on désactive la face mitoyenne
            if (chunkData.blocks.ContainsKey(localPos + Vector3Int.up))
                blockObj.transform.Find("Top")?.gameObject.SetActive(false);

            if (chunkData.blocks.ContainsKey(localPos + Vector3Int.right))
                blockObj.transform.Find("East")?.gameObject.SetActive(false);

            if (chunkData.blocks.ContainsKey(localPos + Vector3Int.left))
                blockObj.transform.Find("West")?.gameObject.SetActive(false);

            if (chunkData.blocks.ContainsKey(localPos + new Vector3Int(0, 0, 1)))
                blockObj.transform.Find("North")?.gameObject.SetActive(false);

            if (chunkData.blocks.ContainsKey(localPos + new Vector3Int(0, 0, -1)))
                blockObj.transform.Find("South")?.gameObject.SetActive(false);
        }

        BakeChunkCollider(chunkData, chunkGridPos);
        activeChunks.Add(chunkGridPos, chunkData);
    }

    void BakeChunkCollider(ChunkData chunk, Vector2Int chunkGridPos)
    {
        if (chunk.blocks.Count == 0) return;

        GameObject colliderContainer = new GameObject("Optimized_Colliders");
        colliderContainer.transform.SetParent(chunk.chunkContainer.transform);
        colliderContainer.transform.localPosition = Vector3.zero;
        chunk.colliderContainer = colliderContainer;

        Dictionary<int, HashSet<Vector2Int>> layers = new Dictionary<int, HashSet<Vector2Int>>();
        foreach (var coord in chunk.blocks.Keys)
        {
            if (!layers.ContainsKey(coord.y)) layers[coord.y] = new HashSet<Vector2Int>();
            layers[coord.y].Add(new Vector2Int(coord.x, coord.z));
        }

        foreach (var layer in layers)
        {
            int y = layer.Key;
            HashSet<Vector2Int> points = layer.Value;
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            foreach (var p in points)
            {
                if (visited.Contains(p)) continue;

                int width = 1;
                while (points.Contains(new Vector2Int(p.x + width, p.y)) && !visited.Contains(new Vector2Int(p.x + width, p.y)) && (p.x + width) < (chunkGridPos.x * ChunkSize + ChunkSize))
                {
                    width++;
                }

                int length = 1;
                bool canExpandZ = true;
                while (canExpandZ)
                {
                    for (int w = 0; w < width; w++)
                    {
                        Vector2Int checkPoint = new Vector2Int(p.x + w, p.y + length);
                        if (!points.Contains(checkPoint) || visited.Contains(checkPoint) || (p.y + length) >= (chunkGridPos.y * ChunkSize + ChunkSize))
                        {
                            canExpandZ = false;
                            break;
                        }
                    }
                    if (canExpandZ) length++;
                }

                for (int w = 0; w < width; w++)
                {
                    for (int l = 0; l < length; l++)
                    {
                        visited.Add(new Vector2Int(p.x + w, p.y + l));
                    }
                }

                GameObject boxObj = new GameObject($"Collider_L{y}");
                boxObj.transform.SetParent(colliderContainer.transform);

                float centerX = p.x + (width - 1) * 0.5f;
                float centerZ = p.y + (length - 1) * 0.5f;

                boxObj.transform.position = new Vector3(centerX, y, centerZ);

                BoxCollider bc = boxObj.AddComponent<BoxCollider>();
                bc.size = new Vector3(width, 1f, length);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!showColliderWireframe) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
        foreach (var chunk in activeChunks.Values)
        {
            if (chunk.colliderContainer != null)
            {
                foreach (Transform child in chunk.colliderContainer.transform)
                {
                    BoxCollider bc = child.GetComponent<BoxCollider>();
                    if (bc != null) Gizmos.DrawWireCube(child.position, bc.size);
                }
            }
        }
    }
}