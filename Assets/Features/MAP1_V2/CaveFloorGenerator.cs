using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveFloorGenerator : MonoBehaviour
{
    [Header("Taille de l'Immense Carte")]
    public int mapWidth = 200;
    public int mapLength = 200;
    public float wallBaseHeight = 7f;

    [Header("Relief Smooth (Pas de 0.2)")]
    public float stepY = 0.2f;
    public float terrainHeightScale = 2f;
    public float terrainFrequency = 0.03f;
    public float wallElevationFactor = 1.2f; // Hauteur ajoutée près des murs

    [Header("2 Grandes Grottes & Chemin Sineux")]
    public int minRoomSize = 35;             // Immenses salles
    public int maxRoomSize = 60;
    public int corridorWidth = 6;            // Larges chemins
    public float corridorWaviness = 0.15f;   // Sinuosité du chemin

    [Header("Système de Seed")]
    public bool useRandomSeed = true;
    public int seed = 12345;

    [Header("Prefabs & Références")]
    public GameObject wallBlockPrefab;
    public GameObject floorBlockPrefab;
    public GameObject exitStairsPrefab;
    public Transform playerTransform;

    private int[,] mapGrid;
    private List<Vector2Int> roomCenters = new List<Vector2Int>();

    void Start()
    {
        // Ne génère au Start que si le niveau n'existe pas déjà dans la scène
        if (transform.childCount == 0)
        {
            GenerateNewFloor();
        }
    }

    // [ContextMenu] permet de lancer la génération directement dans l'éditeur sans faire PLAY !
    [ContextMenu("Générer la Carte (Editeur)")]
    public void GenerateNewFloor()
    {
        if (useRandomSeed)
        {
            seed = Random.Range(0, 999999);
        }
        Random.InitState(seed);

        ClearOldMap();

        mapGrid = new int[mapWidth, mapLength];
        for (int x = 0; x < mapWidth; x++)
            for (int z = 0; z < mapLength; z++)
                mapGrid[x, z] = 1; // 1 = Mur

        roomCenters.Clear();

        // 1. Génération de la Grotte A (Spawn)
        int wA = Random.Range(minRoomSize, maxRoomSize);
        int lA = Random.Range(minRoomSize, maxRoomSize);
        Vector2Int centerA = new Vector2Int(wA / 2 + 10, lA / 2 + 10);
        CarveRoom(centerA.x - wA / 2, centerA.y - lA / 2, wA, lA);
        roomCenters.Add(centerA);

        // 2. Génération de la Grotte B (Sortie) à l'autre bout de la carte
        int wB = Random.Range(minRoomSize, maxRoomSize);
        int lB = Random.Range(minRoomSize, maxRoomSize);
        Vector2Int centerB = new Vector2Int(mapWidth - wB / 2 - 10, mapLength - lB / 2 - 10);
        CarveRoom(centerB.x - wB / 2, centerB.y - lB / 2, wB, lB);
        roomCenters.Add(centerB);

        // 3. Connexion par un chemin SINEUX
        CarveWavyCorridor(centerA, centerB);

        // 4. Construction des Voxels avec effet de remontée le long des murs
        BuildVoxelWorld();

        // 5. Positionnement du Joueur et de la Sortie
        if (playerTransform != null)
        {
            float startY = GetSmoothHeight(centerA.x, centerA.y) + 1.5f;
            playerTransform.position = new Vector3(centerA.x, startY, centerA.y);
        }

        if (exitStairsPrefab != null)
        {
            float exitY = GetSmoothHeight(centerB.x, centerB.y) + 0.5f;
            Instantiate(exitStairsPrefab, new Vector3(centerB.x, exitY, centerB.y), Quaternion.identity, transform);
        }
    }

    private void ClearOldMap()
    {
        // Utilisation de DestroyImmediate pour pouvoir nettoyer la scène hors mode Play
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    void CarveRoom(int startX, int startZ, int width, int length)
    {
        for (int x = startX; x < startX + width; x++)
        {
            for (int z = startZ; z < startZ + length; z++)
            {
                if (x >= 0 && x < mapWidth && z >= 0 && z < mapLength)
                    mapGrid[x, z] = 0;
            }
        }
    }

    // Tunnel sinueux utilisant une trajectoire courbe
    void CarveWavyCorridor(Vector2Int start, Vector2Int end)
    {
        Vector2 current = new Vector2(start.x, start.y);
        Vector2 target = new Vector2(end.x, end.y);

        int stepCount = 0;
        while (Vector2.Distance(current, target) > 2f && stepCount < 2000)
        {
            stepCount++;
            Vector2 dir = (target - current).normalized;

            // Perturbation sinueuse avec du bruit de Perlin
            float noise = (Mathf.PerlinNoise(current.x * corridorWaviness, current.y * corridorWaviness) - 0.5f) * 2f;
            Vector2 perpendicular = new Vector2(-dir.y, dir.x);
            Vector2 moveStep = (dir + perpendicular * noise).normalized;

            current += moveStep;

            int cx = Mathf.RoundToInt(current.x);
            int cz = Mathf.RoundToInt(current.y);

            // Creuser un large cercle autour de la position actuelle
            for (int rx = -corridorWidth; rx <= corridorWidth; rx++)
            {
                for (int rz = -corridorWidth; rz <= corridorWidth; rz++)
                {
                    if (rx * rx + rz * rz <= corridorWidth * corridorWidth)
                    {
                        int tx = Mathf.Clamp(cx + rx, 0, mapWidth - 1);
                        int tz = Mathf.Clamp(cz + rz, 0, mapLength - 1);
                        mapGrid[tx, tz] = 0;
                    }
                }
            }
        }
    }

    // Calculateur de hauteur avec effet de pente sur les bords
    float GetSmoothHeight(int x, int z)
    {
        float rawNoise = Mathf.PerlinNoise((x + seed) * terrainFrequency, (z + seed) * terrainFrequency);
        float baseHeight = rawNoise * terrainHeightScale;

        // Détection de la proximité d'un mur pour faire remonter le sol
        float wallBonus = 0f;
        if (IsNearWall(x, z))
        {
            wallBonus = wallElevationFactor;
        }

        float totalY = baseHeight + wallBonus;
        return Mathf.Round(totalY / stepY) * stepY; // Arrondi à 0.2
    }

    bool IsNearWall(int x, int z)
    {
        // Regarde si un mur se trouve à moins de 2 blocs de distance
        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dz = -2; dz <= 2; dz++)
            {
                int checkX = x + dx;
                int checkZ = z + dz;
                if (checkX >= 0 && checkX < mapWidth && checkZ >= 0 && checkZ < mapLength)
                {
                    if (mapGrid[checkX, checkZ] == 1) return true;
                }
            }
        }
        return false;
    }

    void BuildVoxelWorld()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapLength; z++)
            {
                float floorY = GetSmoothHeight(x, z);

                if (mapGrid[x, z] == 0) // Sol
                {
                    Instantiate(floorBlockPrefab, new Vector3(x, floorY, z), Quaternion.identity, transform);
                }
                else // Mur
                {
                    float wallTop = floorY + wallBaseHeight;
                    for (float y = floorY; y <= wallTop; y += stepY)
                    {
                        Instantiate(wallBlockPrefab, new Vector3(x, y, z), Quaternion.identity, transform);
                    }
                }
            }
        }
    }
}