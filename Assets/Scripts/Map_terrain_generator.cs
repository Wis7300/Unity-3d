using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Réglages")]
    public GameObject blockPrefab;
    public Transform container;
    public int size = 50;
    public float scale = 10f; // Plus c'est petit, plus les murs sont gros

    [Header("Matériaux")]
    public Material[] stoneMaterials; // 0: Sol, 1: Mur, 2: Sommet

    [ContextMenu("Generate Map")]
    public void GenerateMap()
    {
        ClearMap();

        for (int x = 0; x < size; x++)
        {
            for (int z = 0; z < size; z++)
            {
                // On utilise le Perlin Noise pour décider si c'est un Mur ou un Chemin
                float noise = Mathf.PerlinNoise(x / scale, z / scale);

                // Si noise > 0.45 : Mur (hauteur 4), sinon Sol (hauteur 1)
                int h = (noise > 0.45f) ? 4 : 1;

                for (int y = 0; y < h; y++)
                {
                    // On place les blocs à des coordonnées ENTIÈRES (x, y, z)
                    GameObject b = Instantiate(blockPrefab, container);
                    b.transform.localPosition = new Vector3(x, y, z);

                    // Assignation simple des matériaux
                    Renderer r = b.GetComponent<Renderer>();
                    if (y == 0) r.material = stoneMaterials[0];
                    else if (y == h - 1) r.material = stoneMaterials[2];
                    else r.material = stoneMaterials[1];
                }
            }
        }
    }

    [ContextMenu("Clear Map")]
    public void ClearMap()
    {
        if (container == null) return;
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in container) children.Add(child.gameObject);
        children.ForEach(child => DestroyImmediate(child));
    }
}