using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

/// <summary>
/// Procedural level generator for The Echoing Asylum
/// Creates hallways, rooms, and places objects using primitive shapes
/// </summary>
public class AsylumLevelGenerator : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int numberOfRooms = 20;
    [SerializeField] private int numberOfHallways = 15;
    [SerializeField] private Vector2 roomSizeRange = new Vector2(8f, 15f);
    [SerializeField] private float hallwayWidth = 3f;
    [SerializeField] private float wallHeight = 4f;
    
    [Header("Materials")]
    [SerializeField] private Material wallMaterial;
    [SerializeField] private Material floorMaterial;
    [SerializeField] private Material ceilingMaterial;
    [SerializeField] private Material doorMaterial;
    
    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private GameObject generatorPrefab;
    [SerializeField] private GameObject documentPrefab;
    [SerializeField] private GameObject hidingSpotPrefab;
    [SerializeField] private GameObject batteryPrefab;
    
    [Header("Spawn Settings")]
    [SerializeField] private int keysToSpawn = 3;
    [SerializeField] private int generatorsToSpawn = 3;
    [SerializeField] private int documentsToSpawn = 8;
    [SerializeField] private int hidingSpotsToSpawn = 10;
    [SerializeField] private int batteriesToSpawn = 5;
    
    private List<Room> rooms = new List<Room>();
    private System.Random random;
    
    private class Room
    {
        public Vector3 position;
        public Vector3 size;
        public List<GameObject> walls = new List<GameObject>();
        public GameObject floor;
        public GameObject ceiling;
        public List<Vector3> doorPositions = new List<Vector3>();
    }
    
    void Start()
    {
        random = new System.Random();
        GenerateLevel();
    }
    
    void GenerateLevel()
    {
        // Create materials if not assigned
        CreateDefaultMaterials();
        
        // Generate rooms
        GenerateRooms();
        
        // Generate hallways connecting rooms
        GenerateHallways();
        
        // Bake NavMesh
        StartCoroutine(BakeNavMeshDelayed());
        
        // Spawn objects
        SpawnObjects();
        
        // Spawn player and enemy
        SpawnPlayerAndEnemy();
    }
    
    void CreateDefaultMaterials()
    {
        if (wallMaterial == null)
        {
            wallMaterial = new Material(Shader.Find("Standard"));
            wallMaterial.color = new Color(0.3f, 0.3f, 0.35f);
        }
        
        if (floorMaterial == null)
        {
            floorMaterial = new Material(Shader.Find("Standard"));
            floorMaterial.color = new Color(0.2f, 0.2f, 0.25f);
        }
        
        if (ceilingMaterial == null)
        {
            ceilingMaterial = new Material(Shader.Find("Standard"));
            ceilingMaterial.color = new Color(0.25f, 0.25f, 0.3f);
        }
        
        if (doorMaterial == null)
        {
            doorMaterial = new Material(Shader.Find("Standard"));
            doorMaterial.color = new Color(0.4f, 0.3f, 0.25f);
        }
    }
    
    void GenerateRooms()
    {
        for (int i = 0; i < numberOfRooms; i++)
        {
            Room room = new Room();
            
            // Random room size
            float width = Random.Range(roomSizeRange.x, roomSizeRange.y);
            float depth = Random.Range(roomSizeRange.x, roomSizeRange.y);
            room.size = new Vector3(width, wallHeight, depth);
            
            // Random position (spread out)
            float angle = (i / (float)numberOfRooms) * 360f + Random.Range(-20f, 20f);
            float distance = Random.Range(10f, 40f);
            room.position = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * distance,
                0,
                Mathf.Sin(angle * Mathf.Deg2Rad) * distance
            );
            
            // Create room geometry
            CreateRoomGeometry(room);
            
            rooms.Add(room);
        }
    }
    
    void CreateRoomGeometry(Room room)
    {
        GameObject roomParent = new GameObject($"Room_{rooms.Count}");
        roomParent.transform.position = room.position;
        roomParent.transform.parent = transform;
        
        // Create floor
        room.floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        room.floor.name = "Floor";
        room.floor.transform.parent = roomParent.transform;
        room.floor.transform.localPosition = new Vector3(0, -0.5f, 0);
        room.floor.transform.localScale = new Vector3(room.size.x, 1f, room.size.z);
        room.floor.GetComponent<Renderer>().material = floorMaterial;
        
        // Create ceiling
        room.ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        room.ceiling.name = "Ceiling";
        room.ceiling.transform.parent = roomParent.transform;
        room.ceiling.transform.localPosition = new Vector3(0, wallHeight - 0.5f, 0);
        room.ceiling.transform.localScale = new Vector3(room.size.x, 1f, room.size.z);
        room.ceiling.GetComponent<Renderer>().material = ceilingMaterial;
        
        // Create walls
        // North wall
        GameObject northWall = CreateWall(roomParent.transform, new Vector3(0, wallHeight / 2, room.size.z / 2), new Vector3(room.size.x, wallHeight, 0.5f));
        room.walls.Add(northWall);
        
        // South wall
        GameObject southWall = CreateWall(roomParent.transform, new Vector3(0, wallHeight / 2, -room.size.z / 2), new Vector3(room.size.x, wallHeight, 0.5f));
        room.walls.Add(southWall);
        
        // East wall
        GameObject eastWall = CreateWall(roomParent.transform, new Vector3(room.size.x / 2, wallHeight / 2, 0), new Vector3(0.5f, wallHeight, room.size.z));
        room.walls.Add(eastWall);
        
        // West wall
        GameObject westWall = CreateWall(roomParent.transform, new Vector3(-room.size.x / 2, wallHeight / 2, 0), new Vector3(0.5f, wallHeight, room.size.z));
        room.walls.Add(westWall);
        
        // Add lights to room
        AddRoomLighting(roomParent.transform, room.size);
    }
    
    GameObject CreateWall(Transform parent, Vector3 localPosition, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall";
        wall.transform.parent = parent;
        wall.transform.localPosition = localPosition;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().material = wallMaterial;
        return wall;
    }
    
    void AddRoomLighting(Transform room, Vector3 roomSize)
    {
        GameObject lightObj = new GameObject("RoomLight");
        lightObj.transform.parent = room;
        lightObj.transform.localPosition = new Vector3(0, wallHeight - 1f, 0);
        
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.range = Mathf.Max(roomSize.x, roomSize.z);
        light.intensity = Random.Range(0.5f, 1.5f);
        light.color = new Color(1f, 0.9f, 0.8f);
        
        // Add flickering component
        lightObj.AddComponent<FlickeringLight>();
    }
    
    void GenerateHallways()
    {
        // Connect nearby rooms with hallways
        for (int i = 0; i < numberOfHallways && i < rooms.Count - 1; i++)
        {
            Room roomA = rooms[i];
            Room roomB = rooms[i + 1];
            
            CreateHallway(roomA.position, roomB.position);
        }
        
        // Add some random connections
        for (int i = 0; i < numberOfHallways / 2; i++)
        {
            Room roomA = rooms[Random.Range(0, rooms.Count)];
            Room roomB = rooms[Random.Range(0, rooms.Count)];
            
            if (roomA != roomB)
            {
                CreateHallway(roomA.position, roomB.position);
            }
        }
    }
    
    void CreateHallway(Vector3 start, Vector3 end)
    {
        GameObject hallway = new GameObject("Hallway");
        hallway.transform.parent = transform;
        
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        Vector3 midpoint = (start + end) / 2f;
        
        // Create floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "HallwayFloor";
        floor.transform.parent = hallway.transform;
        floor.transform.position = midpoint + Vector3.down * 0.5f;
        floor.transform.rotation = Quaternion.LookRotation(direction);
        floor.transform.localScale = new Vector3(hallwayWidth, 1f, distance);
        floor.GetComponent<Renderer>().material = floorMaterial;
        
        // Create ceiling
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "HallwayCeiling";
        ceiling.transform.parent = hallway.transform;
        ceiling.transform.position = midpoint + Vector3.up * (wallHeight - 0.5f);
        ceiling.transform.rotation = Quaternion.LookRotation(direction);
        ceiling.transform.localScale = new Vector3(hallwayWidth, 1f, distance);
        ceiling.GetComponent<Renderer>().material = ceilingMaterial;
        
        // Create walls
        Vector3 perpendicular = Vector3.Cross(direction.normalized, Vector3.up);
        
        GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.name = "HallwayWall";
        leftWall.transform.parent = hallway.transform;
        leftWall.transform.position = midpoint + perpendicular * (hallwayWidth / 2f) + Vector3.up * (wallHeight / 2f);
        leftWall.transform.rotation = Quaternion.LookRotation(direction);
        leftWall.transform.localScale = new Vector3(0.5f, wallHeight, distance);
        leftWall.GetComponent<Renderer>().material = wallMaterial;
        
        GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.name = "HallwayWall";
        rightWall.transform.parent = hallway.transform;
        rightWall.transform.position = midpoint - perpendicular * (hallwayWidth / 2f) + Vector3.up * (wallHeight / 2f);
        rightWall.transform.rotation = Quaternion.LookRotation(direction);
        rightWall.transform.localScale = new Vector3(0.5f, wallHeight, distance);
        rightWall.GetComponent<Renderer>().material = wallMaterial;
        
        // Add lighting
        for (int i = 0; i < distance / 8f; i++)
        {
            Vector3 lightPos = start + direction.normalized * (i * 8f) + Vector3.up * (wallHeight - 1f);
            GameObject lightObj = new GameObject("HallwayLight");
            lightObj.transform.parent = hallway.transform;
            lightObj.transform.position = lightPos;
            
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 8f;
            light.intensity = 1f;
            light.color = new Color(1f, 0.9f, 0.8f);
            
            lightObj.AddComponent<FlickeringLight>();
        }
    }
    
    void SpawnObjects()
    {
        // Spawn keys
        for (int i = 0; i < keysToSpawn && keyPrefab != null; i++)
        {
            SpawnObjectInRandomRoom(keyPrefab, "Key");
        }
        
        // Spawn generators
        for (int i = 0; i < generatorsToSpawn && generatorPrefab != null; i++)
        {
            SpawnObjectInRandomRoom(generatorPrefab, "Generator");
        }
        
        // Spawn documents
        for (int i = 0; i < documentsToSpawn && documentPrefab != null; i++)
        {
            SpawnObjectInRandomRoom(documentPrefab, "Document");
        }
        
        // Spawn hiding spots
        for (int i = 0; i < hidingSpotsToSpawn && hidingSpotPrefab != null; i++)
        {
            SpawnObjectInRandomRoom(hidingSpotPrefab, "HidingSpot");
        }
        
        // Spawn batteries
        for (int i = 0; i < batteriesToSpawn && batteryPrefab != null; i++)
        {
            SpawnObjectInRandomRoom(batteryPrefab, "Battery");
        }
    }
    
    void SpawnObjectInRandomRoom(GameObject prefab, string objectName)
    {
        if (rooms.Count == 0) return;
        
        Room room = rooms[Random.Range(0, rooms.Count)];
        Vector3 randomPos = room.position + new Vector3(
            Random.Range(-room.size.x / 3, room.size.x / 3),
            1f,
            Random.Range(-room.size.z / 3, room.size.z / 3)
        );
        
        GameObject obj = Instantiate(prefab, randomPos, Quaternion.identity);
        obj.name = objectName;
    }
    
    void SpawnPlayerAndEnemy()
    {
        if (rooms.Count < 2) return;
        
        // Spawn player in first room
        if (playerPrefab != null)
        {
            Vector3 playerPos = rooms[0].position + Vector3.up;
            Instantiate(playerPrefab, playerPos, Quaternion.identity);
        }
        
        // Spawn enemy in a distant room
        if (enemyPrefab != null)
        {
            Room farRoom = rooms[rooms.Count - 1];
            Vector3 enemyPos = farRoom.position + Vector3.up;
            Instantiate(enemyPrefab, enemyPos, Quaternion.identity);
        }
    }
    
    System.Collections.IEnumerator BakeNavMeshDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        
        // In a real project, you would use NavMeshBuilder or manually set up NavMesh surfaces
        // For this example, ensure you have a NavMesh baked in the scene
        Debug.Log("NavMesh should be baked. Use Unity's Navigation window to bake the NavMesh.");
    }
}
