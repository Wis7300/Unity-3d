using System.Collections.Generic;
using UnityEngine;

public class VoxelTerrain : MonoBehaviour
{
    [Header("Configurations")]
    public Texture2D heightMap;
    public GameObject cubePrefab;
    public Transform player;

    [Header("Paramètres de Taille")]
    [Tooltip("Hauteur max de la montagne (Z)")]
    public int maxHeightInBlocks = 120;

    [Tooltip("Facteur d'agrandissement (ex: 4 pour ajouter 3 cubes entre chaque pixel)")]
    public int scaleMultiplier = 4;

    [Header("Optimisation")]
    public int viewRadiusInBlocks = 50;

    private Dictionary<Vector3Int, GameObject> activeBlocks = new Dictionary<Vector3Int, GameObject>();
    private Vector3Int lastPlayerBlockPos;

    void Start()
    {
        if (heightMap == null || cubePrefab == null || player == null) return;
        UpdateTerrain();
        lastPlayerBlockPos = GetPlayerBlockPos();
    }

    void Update()
    {
        Vector3Int currentPlayerBlockPos = GetPlayerBlockPos();
        if (currentPlayerBlockPos != lastPlayerBlockPos)
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

        // On calcule la nouvelle taille maximale du monde agrandi
        int maxWorldX = (heightMap.width - 1) * scaleMultiplier;
        int maxWorldZ = (heightMap.height - 1) * scaleMultiplier;

        for (int x = playerPos.x - viewRadiusInBlocks; x <= playerPos.x + viewRadiusInBlocks; x++)
        {
            for (int z = playerPos.z - viewRadiusInBlocks; z <= playerPos.z + viewRadiusInBlocks; z++)
            {
                // On vérifie qu'on reste dans les limites de la carte géante
                if (x >= 0 && x <= maxWorldX && z >= 0 && z <= maxWorldZ)
                {
                    if (Vector2.Distance(new Vector2(playerPos.x, playerPos.z), new Vector2(x, z)) <= viewRadiusInBlocks)
                    {
                        // --- INTERPOLATION BILINÉAIRE (Le lissage des 3 cubes vides) ---

                        // On trouve où l'on se situe sur l'image d'origine
                        float imageX = (float)x / scaleMultiplier;
                        float imageZ = (float)z / scaleMultiplier;

                        // On récupère les coordonnées des 4 pixels qui nous entourent
                        int x0 = Mathf.FloorToInt(imageX);
                        int x1 = Mathf.Clamp(x0 + 1, 0, heightMap.width - 1);
                        int z0 = Mathf.FloorToInt(imageZ);
                        int z1 = Mathf.Clamp(z0 + 1, 0, heightMap.height - 1);

                        // On calcule le pourcentage de distance entre les pixels (ex: 0.25, 0.50, 0.75)
                        float tx = imageX - x0;
                        float tz = imageZ - z0;

                        // On lit la hauteur de ces 4 vrais pixels
                        float h00 = heightMap.GetPixel(x0, z0).grayscale;
                        float h10 = heightMap.GetPixel(x1, z0).grayscale;
                        float h01 = heightMap.GetPixel(x0, z1).grayscale;
                        float h11 = heightMap.GetPixel(x1, z1).grayscale;

                        // On calcule la pente (vecteur) sur X, puis sur Z
                        float h0 = Mathf.Lerp(h00, h10, tx);
                        float h1 = Mathf.Lerp(h01, h11, tx);
                        float finalHeightValue = Mathf.Lerp(h0, h1, tz);

                        // On transforme cette valeur lissée en hauteur de bloc entière
                        int blockY = Mathf.RoundToInt(finalHeightValue * maxHeightInBlocks);

                        // ---------------------------------------------------------------

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

        // Nettoyage des blocs hors du rayon de vue
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