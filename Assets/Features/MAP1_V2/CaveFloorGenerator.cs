using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveFloorGenerator : MonoBehaviour
{
    [Header("Taille du Niveau")]
    public int mapWidth = 60;
    public int mapLength = 60;
    public int wallHeight = 4; // Hauteur des murs en blocs

    [Header("Configuration des Salles")]
    public int minRooms = 4;
    public int maxRooms = 7;
    public int minRoomSize = 6;
    public int maxRoomSize = 12;
    public int corridorWidth = 2; // Largeur des tunnels

    [Header("Système de Seed (Sauvegarde/Rejouabilité)")]
    public bool useRandomSeed = true;
    public int seed = 12345;

    [Header("Prefabs & Références")]
    public GameObject wallBlockPrefab;
    public GameObject floorBlockPrefab;
    public GameObject exitStairsPrefab;
    public Transform playerTransform;

    private int[,] mapGrid;
    private List<Room> rooms = new List<Room>();

    public struct Room
    {
        public int x, z, width, length;
        public Vector2Int Center => new Vector2Int(x + width / 2, z + length / 2);

        public Room(int x, int z, int width, int length)
        {
            this.x = x;
            this.z = z;
            this.width = width;
            this.length = length;
        }
    }

    void Start()
    {
        GenerateNewFloor();
    }

    public void GenerateNewFloor()
    {
        // GESTION DE LA SEED :
        if (useRandomSeed)
        {
            seed = Random.Range(0, 999999);
        }
        Random.InitState(seed);

        // 1. Vider le niveau précédent s'il existe
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        rooms.Clear();

        // 2. Initialiser la grille remplie de murs (1)
        mapGrid = new int[mapWidth, mapLength];
        for (int x = 0; x < mapWidth; x++)
            for (int z = 0; z < mapLength; z++)
                mapGrid[x, z] = 1;

        // 3. Creuser les salles
        int roomCount = Random.Range(minRooms, maxRooms + 1);
        for (int i = 0; i < roomCount; i++)
        {
            int w = Random.Range(minRoomSize, maxRoomSize);
            int l = Random.Range(minRoomSize, maxRoomSize);
            int x = Random.Range(2, mapWidth - w - 2);
            int z = Random.Range(2, mapLength - l - 2);

            Room newRoom = new Room(x, z, w, l);

            // Vérifier si la salle chevauche une autre
            bool overlaps = false;
            foreach (var r in rooms)
            {
                if (x < r.x + r.width + 2 && x + w + 2 > r.x &&
                    z < r.z + r.length + 2 && z + l + 2 > r.z)
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
            {
                CarveRoom(newRoom);
                rooms.Add(newRoom);
            }
        }

        // 4. Relier les salles par des tunnels (Tunnels en L)
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            ConnectRooms(rooms[i].Center, rooms[i + 1].Center);
        }

        // 5. Instancier le décor 3D
        BuildVoxelWorld();

        // 6. Placer le joueur dans la 1ère salle et la sortie dans la dernière
        if (rooms.Count > 0 && playerTransform != null)
        {
            Vector2Int startPos = rooms[0].Center;
            playerTransform.position = new Vector3(startPos.x, 1.5f, startPos.y);

            if (exitStairsPrefab != null)
            {
                Vector2Int exitPos = rooms[rooms.Count - 1].Center;
                Instantiate(exitStairsPrefab, new Vector3(exitPos.x, 0.5f, exitPos.y), Quaternion.identity, transform);
            }
        }
    }

    void CarveRoom(Room room)
    {
        for (int x = room.x; x < room.x + room.width; x++)
        {
            for (int z = room.z; z < room.z + room.length; z++)
            {
                mapGrid[x, z] = 0; // 0 = Vide
            }
        }
    }

    void ConnectRooms(Vector2Int start, Vector2Int end)
    {
        int currentX = start.x;
        int currentZ = start.y;

        // Déplacement Horizontal puis Vertical
        while (currentX != end.x)
        {
            CarveCorridorPoint(currentX, currentZ);
            currentX += (end.x > currentX) ? 1 : -1;
        }

        while (currentZ != end.y)
        {
            CarveCorridorPoint(currentX, currentZ);
            currentZ += (end.y > currentZ) ? 1 : -1;
        }
    }

    void CarveCorridorPoint(int x, int z)
    {
        for (int cx = -corridorWidth / 2; cx <= corridorWidth / 2; cx++)
        {
            for (int cz = -corridorWidth / 2; cz <= corridorWidth / 2; cz++)
            {
                int targetX = Mathf.Clamp(x + cx, 0, mapWidth - 1);
                int targetZ = Mathf.Clamp(z + cz, 0, mapLength - 1);
                mapGrid[targetX, targetZ] = 0;
            }
        }
    }

    void BuildVoxelWorld()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int z = 0; z < mapLength; z++)
            {
                if (mapGrid[x, z] == 0) // Sol
                {
                    Instantiate(floorBlockPrefab, new Vector3(x, 0, z), Quaternion.identity, transform);
                }
                else // Murs
                {
                    for (int y = 0; y <= wallHeight; y++)
                    {
                        Instantiate(wallBlockPrefab, new Vector3(x, y, z), Quaternion.identity, transform);
                    }
                }
            }
        }
    }
}