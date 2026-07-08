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

    [Header("Optimisation")]
    public int viewRadiusInBlocks = 40; // Baissé légèrement à 40 pour plus de performances pendant les tests   

    private Dictionary<Vector3Int, GameObject> activeBlocks = new Dictionary<Vector3Int, GameObject>();
    private Vector3Int lastPlayerBlockPos;

    void Start()
    {
        // Si le joueur n'est pas assigné ou s'il a changé au respawn, on le cherche par son Tag "Player"
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null) player = foundPlayer.transform;
        }

        if (heightMap == null || cubePrefab == null || player == null)
        {
            Debug.LogError("⚠️ [VoxelTerrain] Il manque des assignations dans l'inspecteur !");
            return;
        }

        UpdateTerrain();
        lastPlayerBlockPos = GetPlayerBlockPos();
    }

    void Update()
    {
        if (player == null) return;

        Vector3Int currentPlayerBlockPos = GetPlayerBlockPos();
        // Détection de mouvement basée sur la position absolue brute
        if (Mathf.Abs(currentPlayerBlockPos.x - lastPlayerBlockPos.x) >= 1 || Mathf.Abs(currentPlayerBlockPos.z - lastPlayerBlockPos.z) >= 1)
        {
            lastPlayerBlockPos = currentPlayerBlockPos;
            UpdateTerrain();
        }
    }

    Vector3Int GetPlayerBlockPos()
    {
        return new Vector3Int(
            Mathf.FloorToInt(player.position.x),
            0,
            Mathf.FloorToInt(player.position.z)
        );
    }

    void UpdateTerrain()
    {
        Vector3Int playerPos = GetPlayerBlockPos();
        HashSet<Vector3Int> blocksShouldBeActive = new HashSet<Vector3Int>();

        int maxWorldX = (heightMap.width - 1) * scaleMultiplier;
        int maxWorldZ = (heightMap.height - 1) * scaleMultiplier;

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

                        float tx = imageX - x0;
                        float tz = imageZ - z0;

                        float h00 = heightMap.GetPixel(x0, z0).grayscale;
                        float h10 = heightMap.GetPixel(x1, z0).grayscale;
                        float h01 = heightMap.GetPixel(x0, z1).grayscale;
                        float h11 = heightMap.GetPixel(x1, z1).grayscale;

                        float h0 = Mathf.Lerp(h00, h10, tx);
                        float h1 = Mathf.Lerp(h01, h11, tx);
                        float finalHeightValue = Mathf.Lerp(h0, h1, tz);

                        int blockY = Mathf.RoundToInt(finalHeightValue * maxHeightInBlocks);

                        Vector3Int blockCoords = new Vector3Int(x, blockY, z);
                        blocksShouldBeActive.Add(blockCoords);

                        if (!activeBlocks.ContainsKey(blockCoords))
                        {
                            Vector3 worldPos = new Vector3(x, blockY, z);
                            GameObject newBlock = Instantiate(cubePrefab, worldPos, Quaternion.identity, transform);
                            activeBlocks.Add(blockCoords, newBlock);
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
            }
        }

        foreach (var key in blocksToRemove)
        {
            activeBlocks.Remove(key);
        }
    }
}