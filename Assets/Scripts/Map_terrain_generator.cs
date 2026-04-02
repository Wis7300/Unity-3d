using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Réglages Map")]
    public GameObject blockPrefab;
    public Transform container;
    public int size = 50;

    [Header("Paramètres Gorge")]
    public int pathWidth = 6;
    public int maxWallHeight = 10;
    public float mossThreshold = 0.6f;

    [Header("Matériaux")]
    public Material[] stoneMaterials;
    public Material[] mossMaterials;

    private int[,] heightMap;

    [ContextMenu("Generate V4.4 Absolute")]
    public void GenerateMap()
    {
        if (container == null || blockPrefab == null) return;

        // ÉTAPE CRUCIALE : On réinitialise le container pour éviter la torsion
        container.localPosition = Vector3.zero;
        container.localRotation = Quaternion.identity;
        container.localScale = Vector3.one;

        heightMap = new int[size, size];

        for (int x = 0; x < size; x++)
            for (int z = 0; z < size; z++)
                heightMap[x, z] = maxWallHeight;

        GenerateAdvancedPathSystem();
        ApplyGorgeLogic();
        RenderMapAbsolute();
    }

    void GenerateAdvancedPathSystem()
    {
        Vector2Int current = new Vector2Int(0, size / 2);
        int currentH = 0;
        while (current.x < size)
        {
            int rWidth = Random.Range(4, pathWidth + 1);
            if (Random.value > 0.9f) currentH = Mathf.Clamp(currentH + Random.Range(-1, 2), 0, 1);
            DigPath(current.x, current.y, rWidth, currentH);
            current.x++;
            if (Random.value > 0.7f) current.y += Random.Range(-1, 2);
            current.y = Mathf.Clamp(current.y, 10, size - 10);
        }
    }

    void DigPath(int cx, int cz, int w, int h)
    {
        int r = w / 2;
        for (int x = -r; x <= r; x++)
            for (int z = -r; z <= r; z++)
            {
                int nx = cx + x; int nz = cz + z;
                if (nx >= 0 && nx < size && nz >= 0 && nz < size) heightMap[nx, nz] = h;
            }
    }

    void ApplyGorgeLogic()
    {
        for (int x = 0; x < size; x++)
            for (int z = 0; z < size; z++)
                if (heightMap[x, z] > 1)
                {
                    float dist = FindNearestPathDist(x, z);
                    if (dist <= 1.5f) heightMap[x, z] = 0;
                    else if (dist <= 4f) heightMap[x, z] = 3;
                    else heightMap[x, z] = Mathf.Min(maxWallHeight, 3 + Mathf.FloorToInt(dist - 4) * 2);
                }
    }

    float FindNearestPathDist(int x, int z)
    {
        float min = 20f;
        for (int dx = -5; dx <= 5; dx++)
            for (int dz = -5; dz <= 5; dz++)
            {
                int nx = x + dx; int nz = z + dz;
                if (nx >= 0 && nx < size && nz >= 0 && nz < size && heightMap[nx, nz] <= 1)
                {
                    float d = Vector2.Distance(new Vector2(x, z), new Vector2(nx, nz));
                    if (d < min) min = d;
                }
            }
        return min;
    }

    void RenderMapAbsolute()
    {
        // Nettoyage radical
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (Transform child in container) toDestroy.Add(child.gameObject);
        toDestroy.ForEach(child => DestroyImmediate(child));

        // On utilise la taille du prefab pour forcer le placement
        Vector3 s = Vector3.one;
        MeshRenderer mr = blockPrefab.GetComponentInChildren<MeshRenderer>();
        if (mr != null) s = mr.bounds.size;

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                int h = heightMap[x, z];
                float mNoise = Mathf.PerlinNoise(x * 0.12f, z * 0.12f);
                Material[] fam = (mNoise > mossThreshold) ? mossMaterials : stoneMaterials;

                for (int y = 0; y <= h; y++)
                {
                    // Placement forcé sur grille locale
                    GameObject b = Instantiate(blockPrefab, container);
                    b.transform.localPosition = new Vector3(x * s.x, y * s.y, z * s.z);
                    b.transform.localRotation = Quaternion.identity;

                    Renderer r = b.GetComponentInChildren<Renderer>();
                    if (r != null && fam.Length >= 3)
                    {
                        if (y == 0) r.material = fam[0];
                        else if (y == h) r.material = fam[2];
                        else r.material = fam[1];
                    }
                }
            }
        }
    }
}