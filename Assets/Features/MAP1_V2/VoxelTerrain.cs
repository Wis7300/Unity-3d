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
    public int viewRadiusInBlocks = 40;

    private Dictionary<Vector3Int, GameObject> activeBlocks = new Dictionary<Vector3Int, GameObject>();
    private List<BoxCollider> generatedColliders = new List<BoxCollider>();
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

        // 1. Déterminer les blocs qui doivent être présents
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
                        }
                    }
                }
            }
        }

        // 2. Nettoyage des blocs éloignés
        List<Vector3Int> blocksToRemove = new List<Vector3Int>();
        foreach (var block in activeBlocks)
        {
            if (!blocksShouldBeActive.Contains(block.Key))
            {
                Destroy(block.Value);
                blocksToRemove.Add(block.Key);
            }
        }
        foreach (var key in blocksToRemove) activeBlocks.Remove(key);

        // 3. RECONSTRUCTION DES HITBOXS FUSIONNÉES
        GenerateMergedColliders(playerPos);
    }

    // Algorithme de fusion horizontale des colliders (X Axis Grid)
    void GenerateMergedColliders(Vector3Int playerPos)
    {
        // Supprimer les anciens colliders globaux temporaires
        foreach (var col in generatedColliders) if (col != null) Destroy(col);
        generatedColliders.Clear();

        // On crée un dictionnaire temporaire pour marquer les blocs traités lors de la fusion
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        foreach (var pair in activeBlocks)
        {
            Vector3Int startPos = pair.Key;

            if (visited.Contains(startPos)) continue;

            // On cherche jusqu'où on peut étirer la hitbox sur l'axe X à la même hauteur (Y) et même ligne (Z)
            int lengthX = 1;
            while (activeBlocks.ContainsKey(new Vector3Int(startPos.x + lengthX, startPos.y, startPos.z)) &&
                   !visited.Contains(new Vector3Int(startPos.x + lengthX, startPos.y, startPos.z)))
            {
                lengthX++;
            }

            // Marquer ces blocs comme fusionnés
            for (int i = 0; i < lengthX; i++)
            {
                visited.Add(new Vector3Int(startPos.x + i, startPos.y, startPos.z));
            }

            // Créer une seule BoxCollider pour toute cette rangée
            BoxCollider newCollider = gameObject.AddComponent<BoxCollider>();

            // Calcul du centre et de la taille de la boîte fusionnée
            newCollider.center = new Vector3(startPos.x + (lengthX - 1) * 0.5f, startPos.y, startPos.z);
            newCollider.size = new Vector3(lengthX, 1.005f, 1.005f); // Un poil plus grand pour bloquer les trous entre diagonales

            generatedColliders.Add(newCollider);
        }
    }
}