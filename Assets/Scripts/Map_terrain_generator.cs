using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Réglages Map")]
    public GameObject blockPrefab;
    public Transform container;
    public int size = 50;

    [Header("Matériaux Pierres (1,2,3)")]
    public Material[] stoneMaterials;
    [Header("Matériaux Mousses (1,2,3)")]
    public Material[] mossMaterials;

    [Header("Paramètres Chemin")]
    [Range(3, 6)] public int mainPathWidth = 5;
    [Range(2, 4)] public int branchWidth = 3;

    private bool[,] isPath; // Grille invisible du chemin
    private float[,] heightMap;

    [ContextMenu("Generate V5 - Carver")]
    public void GenerateMap()
    {
        if (container == null || blockPrefab == null) return;

        // Reset container
        container.localPosition = Vector3.zero;
        container.localScale = Vector3.one;
        foreach (Transform child in container) DestroyImmediate(child.gameObject);

        isPath = new bool[size, size];
        heightMap = new float[size, size];

        // 1. Tracer le chemin invisible
        CreateInvisiblePath();

        // 2. Calculer le terrain (vagues)
        CalculateTerrain();

        // 3. Rendu
        RenderFinalMap();
    }

    [ContextMenu("Clear Map")] // Cette ligne crée l'option dans le clic droit
    public void ClearMap()
    {
        if (container == null)
        {
            Debug.LogWarning("Le container est vide, rien à supprimer.");
            return;
        }

        // On crée une liste temporaire pour éviter les erreurs de modification de collection pendant la boucle
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in container)
        {
            children.Add(child.gameObject);
        }

        // On supprime chaque objet
        foreach (GameObject child in children)
        {
            DestroyImmediate(child);
        }

        Debug.Log("Map nettoyée avec succès !");
    }

    void CreateInvisiblePath()
    {
        // Chemin Principal (D'un bord à l'autre)
        int curZ = size / 2;
        for (int x = 0; x < size; x++)
        {
            MarkPathCircle(x, curZ, mainPathWidth);
            if (Random.value > 0.7f) curZ += Random.Range(-1, 2);
            curZ = Mathf.Clamp(curZ, 10, size - 10);

            // Création d'une branche qui traverse aussi
            if (x == size / 4 || x == size / 2)
            {
                CreateCrossingBranch(x, curZ);
            }
        }
    }

    void CreateCrossingBranch(int startX, int startZ)
    {
        int curX = startX;
        int curZ = startZ;
        // La branche part vers le haut ou le bas jusqu'au bord
        int direction = (Random.value > 0.5f) ? 1 : -1;

        while (curZ > 0 && curZ < size - 1)
        {
            MarkPathCircle(curX, curZ, branchWidth);
            curZ += direction;
            if (Random.value > 0.8f) curX += Random.Range(-1, 2);
            curX = Mathf.Clamp(curX, 5, size - 5);
        }
    }

    void MarkPathCircle(int cx, int cz, int width)
    {
        int r = width / 2;
        for (int x = -r; x <= r; x++)
            for (int z = -r; z <= r; z++)
            {
                int nx = cx + x; int nz = cz + z;
                if (nx >= 0 && nx < size && nz >= 0 && nz < size) isPath[nx, nz] = true;
            }
    }

    void CalculateTerrain()
    {
        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                // Bruit de Perlin doux pour les "vagues" des murs
                float wave = Mathf.PerlinNoise(x * 0.1f, z * 0.1f) * 4f + 2;
                heightMap[x, z] = wave;
            }
        }
    }

    void RenderFinalMap()
    {
        // On récupère la taille pour coller les blocs
        Vector3 s = Vector3.one;
        MeshRenderer mr = blockPrefab.GetComponentInChildren<MeshRenderer>();
        if (mr != null) s = mr.bounds.size;

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                // Hauteur : Si c'est le chemin, hauteur = 0 (juste le sol). Sinon, hauteur du terrain.
                int h = isPath[x, z] ? 0 : Mathf.FloorToInt(heightMap[x, z]);

                float mNoise = Mathf.PerlinNoise(x * 0.15f, z * 0.15f);
                Material[] fam = (mNoise > 0.6f) ? mossMaterials : stoneMaterials;

                for (int y = 0; y <= h; y++)
                {
                    GameObject b = Instantiate(blockPrefab, container);
                    b.transform.localPosition = new Vector3(x * s.x, y * s.y, z * s.z);

                    Renderer r = b.GetComponentInChildren<Renderer>();
                    if (y == 0) r.material = fam[0];
                    else if (y == h) r.material = fam[2];
                    else r.material = fam[1];
                }
            }
        }
        Debug.Log("Map V5 Terminée : Chemin plat et murs en vagues !");
    }
}