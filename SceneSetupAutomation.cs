using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Automated scene setup script for The Echoing Asylum
/// This script will create all necessary GameObjects and components
/// Run from Unity Editor: Tools → Setup Echoing Asylum Scene
/// </summary>
public class SceneSetupAutomation : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Setup Echoing Asylum Scene")]
    static void SetupScene()
    {
        Debug.Log("Starting Echoing Asylum Scene Setup...");
        
        // Create Player
        CreatePlayer();
        
        // Create Enemy
        CreateEnemy();
        
        // Create Environment
        CreateEnvironment();
        
        // Create Game Manager
        CreateGameManager();
        
        // Create UI
        CreateUI();
        
        // Create Sample Level
        CreateSampleLevel();
        
        Debug.Log("Scene Setup Complete! Press Play to test.");
        EditorUtility.DisplayDialog("Setup Complete", 
            "The Echoing Asylum scene has been set up!\n\n" +
            "Next steps:\n" +
            "1. Bake NavMesh (Window → AI → Navigation → Bake)\n" +
            "2. Configure audio clips in components\n" +
            "3. Press Play to test!", 
            "OK");
    }
    
    static void CreatePlayer()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        
        // Add CharacterController
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.5f;
        cc.center = new Vector3(0, 1, 0);
        
        // Add PlayerController
        PlayerController pc = player.AddComponent<PlayerController>();
        
        // Add SanitySystem
        player.AddComponent<SanitySystem>();
        
        // Add AudioSource
        AudioSource playerAudio = player.AddComponent<AudioSource>();
        playerAudio.spatialBlend = 1f;
        
        // Create Camera
        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.tag = "MainCamera";
        cameraObj.transform.parent = player.transform;
        cameraObj.transform.localPosition = new Vector3(0, 1.6f, 0);
        
        Camera cam = cameraObj.AddComponent<Camera>();
        cam.fieldOfView = 75f;
        cam.nearClipPlane = 0.1f;
        
        cameraObj.AddComponent<AudioListener>();
        
        // Create Flashlight
        GameObject flashlight = new GameObject("Flashlight");
        flashlight.transform.parent = cameraObj.transform;
        flashlight.transform.localPosition = new Vector3(0.3f, -0.2f, 0.5f);
        
        Light flashlightLight = flashlight.AddComponent<Light>();
        flashlightLight.type = LightType.Spot;
        flashlightLight.intensity = 1f;
        flashlightLight.range = 15f;
        flashlightLight.spotAngle = 45f;
        flashlightLight.enabled = false;
        
        player.transform.position = new Vector3(0, 1, 0);
        
        Debug.Log("Player created successfully");
    }
    
    static void CreateEnemy()
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = "Enemy";
        enemy.tag = "Enemy";
        enemy.transform.position = new Vector3(10, 1, 0);
        
        // Change color to dark
        Renderer rend = enemy.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.2f, 0.2f, 0.25f);
        rend.material = mat;
        
        // Add NavMeshAgent
        UnityEngine.AI.NavMeshAgent agent = enemy.AddComponent<UnityEngine.AI.NavMeshAgent>();
        agent.speed = 3.5f;
        agent.acceleration = 8f;
        agent.height = 2f;
        agent.radius = 0.5f;
        
        // Add EnemyAI
        EnemyAI ai = enemy.AddComponent<EnemyAI>();
        
        // Add AudioSource
        AudioSource enemyAudio = enemy.AddComponent<AudioSource>();
        enemyAudio.spatialBlend = 1f;
        
        // Add light
        GameObject enemyLight = new GameObject("EnemyLight");
        enemyLight.transform.parent = enemy.transform;
        enemyLight.transform.localPosition = new Vector3(0, 1, 0);
        
        Light light = enemyLight.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = Color.yellow;
        light.intensity = 1f;
        light.range = 8f;
        
        Debug.Log("Enemy created successfully");
    }
    
    static void CreateEnvironment()
    {
        GameObject environment = new GameObject("Environment");
        environment.AddComponent<EnvironmentController>();
        
        // Create ground plane
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(10, 1, 10);
        ground.transform.position = Vector3.zero;
        
        Material groundMat = new Material(Shader.Find("Standard"));
        groundMat.color = new Color(0.2f, 0.2f, 0.25f);
        ground.GetComponent<Renderer>().material = groundMat;
        
        // Create some walls
        for (int i = 0; i < 4; i++)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = $"Wall_{i}";
            wall.transform.parent = environment.transform;
            
            Material wallMat = new Material(Shader.Find("Standard"));
            wallMat.color = new Color(0.3f, 0.3f, 0.35f);
            wall.GetComponent<Renderer>().material = wallMat;
            
            float angle = i * 90f;
            wall.transform.position = new Vector3(
                Mathf.Sin(angle * Mathf.Deg2Rad) * 25f,
                2f,
                Mathf.Cos(angle * Mathf.Deg2Rad) * 25f
            );
            wall.transform.localScale = new Vector3(50f, 4f, 1f);
            wall.transform.rotation = Quaternion.Euler(0, angle, 0);
        }
        
        // Create some lights
        for (int i = 0; i < 10; i++)
        {
            GameObject lightObj = new GameObject($"Light_{i}");
            lightObj.transform.parent = environment.transform;
            lightObj.transform.position = new Vector3(
                Random.Range(-20f, 20f),
                3.5f,
                Random.Range(-20f, 20f)
            );
            
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 10f;
            light.intensity = Random.Range(0.5f, 1.5f);
            light.color = new Color(1f, 0.9f, 0.8f);
            
            lightObj.AddComponent<FlickeringLight>();
        }
        
        // Set fog
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.1f, 0.1f, 0.15f);
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.05f;
        
        Debug.Log("Environment created successfully");
    }
    
    static void CreateGameManager()
    {
        GameObject gm = new GameObject("GameManager");
        GameManager manager = gm.AddComponent<GameManager>();
        
        AudioSource musicSource = gm.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = 0.3f;
        
        Debug.Log("GameManager created successfully");
    }
    
    static void CreateUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create Interaction Text
        GameObject interactionTextObj = new GameObject("InteractionText");
        interactionTextObj.transform.SetParent(canvasObj.transform, false);
        
        Text interactionText = interactionTextObj.AddComponent<Text>();
        interactionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        interactionText.fontSize = 24;
        interactionText.alignment = TextAnchor.MiddleCenter;
        interactionText.color = Color.white;
        
        RectTransform interactionRT = interactionTextObj.GetComponent<RectTransform>();
        interactionRT.anchorMin = new Vector2(0.5f, 0.5f);
        interactionRT.anchorMax = new Vector2(0.5f, 0.5f);
        interactionRT.sizeDelta = new Vector2(400, 50);
        interactionRT.anchoredPosition = new Vector2(0, 0);
        
        interactionTextObj.SetActive(false);
        
        // Create HUD Panel
        GameObject hudPanel = new GameObject("HUD");
        hudPanel.transform.SetParent(canvasObj.transform, false);
        
        RectTransform hudRT = hudPanel.AddComponent<RectTransform>();
        hudRT.anchorMin = new Vector2(0, 1);
        hudRT.anchorMax = new Vector2(0, 1);
        hudRT.pivot = new Vector2(0, 1);
        hudRT.sizeDelta = new Vector2(300, 200);
        hudRT.anchoredPosition = new Vector2(20, -20);
        
        // Battery bar
        CreateUIBar(hudPanel.transform, "BatteryBar", new Vector2(0, -30), Color.green);
        
        // Sanity bar
        CreateUIBar(hudPanel.transform, "SanityBar", new Vector2(0, -60), Color.cyan);
        
        // Stamina bar
        CreateUIBar(hudPanel.transform, "StaminaBar", new Vector2(0, -90), Color.yellow);
        
        Debug.Log("UI created successfully");
    }
    
    static void CreateUIBar(Transform parent, string name, Vector2 position, Color color)
    {
        GameObject barBG = new GameObject($"{name}_BG");
        barBG.transform.SetParent(parent, false);
        
        Image bgImage = barBG.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f);
        
        RectTransform bgRT = barBG.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0, 1);
        bgRT.anchorMax = new Vector2(0, 1);
        bgRT.pivot = new Vector2(0, 1);
        bgRT.sizeDelta = new Vector2(200, 20);
        bgRT.anchoredPosition = position;
        
        GameObject bar = new GameObject(name);
        bar.transform.SetParent(barBG.transform, false);
        
        Image barImage = bar.AddComponent<Image>();
        barImage.color = color;
        barImage.type = Image.Type.Filled;
        barImage.fillMethod = Image.FillMethod.Horizontal;
        
        RectTransform barRT = bar.GetComponent<RectTransform>();
        barRT.anchorMin = Vector2.zero;
        barRT.anchorMax = Vector2.one;
        barRT.sizeDelta = Vector2.zero;
    }
    
    static void CreateSampleLevel()
    {
        // Create a simple room
        GameObject room = new GameObject("SampleRoom");
        
        // Floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.parent = room.transform;
        floor.transform.position = new Vector3(0, -0.5f, 0);
        floor.transform.localScale = new Vector3(20, 1, 20);
        
        // Walls
        for (int i = 0; i < 4; i++)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = $"Wall_{i}";
            wall.transform.parent = room.transform;
            
            float angle = i * 90f;
            wall.transform.position = new Vector3(
                Mathf.Sin(angle * Mathf.Deg2Rad) * 10f,
                2f,
                Mathf.Cos(angle * Mathf.Deg2Rad) * 10f
            );
            wall.transform.localScale = new Vector3(20f, 4f, 1f);
            wall.transform.rotation = Quaternion.Euler(0, angle, 0);
        }
        
        Debug.Log("Sample level created");
    }
#endif
}
