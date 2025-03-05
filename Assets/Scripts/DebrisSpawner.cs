// DebrisSpawner.cs

using UnityEngine;

/// Handles spawning of debris objects in the game.
public class DebrisSpawner : MonoBehaviour
{
    [Header("Debris Prefabs")]
    [SerializeField] private GameObject standardDebrisPrefab;
    [SerializeField] private GameObject heavyDebrisPrefab;
    [SerializeField] private GameObject stickyDebrisPrefab;
    [SerializeField] private GameObject fragileDebrisPrefab;
    
    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2.0f;
    [SerializeField] private bool enableSpawning = true;
    [SerializeField] private int maxDebrisCount = 50;
    
    [Header("Debris Type Spawn Weights")]
    [Range(0f, 1f)]
    [SerializeField] private float standardDebrisWeight = 0.4f;
    [Range(0f, 1f)]
    [SerializeField] private float heavyDebrisWeight = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float stickyDebrisWeight = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float fragileDebrisWeight = 0.2f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    // Reference to the debris manager
    private DebrisManager debrisManager;
    
    // Tracking variables
    private float nextSpawnTime;
    private int activeDebrisCount = 0;
    
    private void Start()
    {
        // Find the debris manager
        debrisManager = FindFirstObjectByType<DebrisManager>();
        if (debrisManager == null)
        {
            Debug.LogError("DebrisManager not found in the scene!");
            enableSpawning = false;
        }
        
        // Validate debris prefabs
        ValidateDebrisPrefabs();
        
        // Initialize next spawn time
        nextSpawnTime = Time.time + spawnInterval;
        
        // Normalize weights if needed
        NormalizeWeights();
    }
    
    /// Validates all debris prefabs
    private void ValidateDebrisPrefabs()
    {
        bool allValid = true;
        
        // Check standard debris
        if (standardDebrisPrefab == null)
        {
            Debug.LogError("Standard debris prefab is not assigned!");
            allValid = false;
        }
        else if (standardDebrisPrefab.GetComponent<StandardDebris>() == null)
        {
            Debug.LogError("Standard debris prefab does not have a StandardDebris component!");
            allValid = false;
        }
        
        // Check heavy debris
        if (heavyDebrisPrefab == null)
        {
            Debug.LogError("Heavy debris prefab is not assigned!");
            allValid = false;
        }
        else if (heavyDebrisPrefab.GetComponent<HeavyDebris>() == null)
        {
            Debug.LogError("Heavy debris prefab does not have a HeavyDebris component!");
            allValid = false;
        }
        
        // Check sticky debris
        if (stickyDebrisPrefab == null)
        {
            Debug.LogError("Sticky debris prefab is not assigned!");
            allValid = false;
        }
        else if (stickyDebrisPrefab.GetComponent<StickyDebris>() == null)
        {
            Debug.LogError("Sticky debris prefab does not have a StickyDebris component!");
            allValid = false;
        }
        
        // Check fragile debris
        if (fragileDebrisPrefab == null)
        {
            Debug.LogError("Fragile debris prefab is not assigned!");
            allValid = false;
        }
        else if (fragileDebrisPrefab.GetComponent<FragileDebris>() == null)
        {
            Debug.LogError("Fragile debris prefab does not have a FragileDebris component!");
            allValid = false;
        }
        
        enableSpawning = allValid;
    }
    
    /// Normalizes the weights to ensure they sum to 1.0
    private void NormalizeWeights()
    {
        float totalWeight = standardDebrisWeight + heavyDebrisWeight + 
                           stickyDebrisWeight + fragileDebrisWeight;
        
        // If total is already 1.0 (or very close), no need to normalize
        if (Mathf.Approximately(totalWeight, 1.0f))
            return;
            
        // If total is zero, use equal weights
        if (Mathf.Approximately(totalWeight, 0.0f))
        {
            standardDebrisWeight = 0.25f;
            heavyDebrisWeight = 0.25f;
            stickyDebrisWeight = 0.25f;
            fragileDebrisWeight = 0.25f;
            return;
        }
        
        // Normalize weights
        standardDebrisWeight /= totalWeight;
        heavyDebrisWeight /= totalWeight;
        stickyDebrisWeight /= totalWeight;
        fragileDebrisWeight /= totalWeight;
    }
    
    private void Update()
    {
        if (!enableSpawning || debrisManager == null)
            return;
            
        // Check if it's time to spawn new debris
        if (Time.time >= nextSpawnTime && activeDebrisCount < maxDebrisCount)
        {
            SpawnRandomDebris();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }
    
    /// Spawns a random debris type based on weights
    private void SpawnRandomDebris()
    {
        // Get a random value to determine debris type
        float randomValue = Random.value;
        
        // Determine which debris type to spawn based on weights
        if (randomValue < standardDebrisWeight)
        {
            SpawnDebris(standardDebrisPrefab, "Standard");
        }
        else if (randomValue < standardDebrisWeight + heavyDebrisWeight)
        {
            SpawnDebris(heavyDebrisPrefab, "Heavy");
        }
        else if (randomValue < standardDebrisWeight + heavyDebrisWeight + stickyDebrisWeight)
        {
            SpawnDebris(stickyDebrisPrefab, "Sticky");
        }
        else
        {
            SpawnDebris(fragileDebrisPrefab, "Fragile");
        }
    }
    
    /// Spawns a specific debris type
    private void SpawnDebris(GameObject prefab, string typeName)
    {
        if (prefab == null)
            return;
            
        // Get a random spawn point
        Vector2 spawnPoint = debrisManager.GetRandomSpawnPoint();
        
        // Calculate linear path direction toward opposite edge
        Vector2 direction = debrisManager.CalculatePathDirection(spawnPoint);
        
        // Instantiate the debris
        GameObject debris = Instantiate(prefab, spawnPoint, Quaternion.identity);
        
        // Initialize the debris
        DebrisBase debrisComponent = debris.GetComponent<DebrisBase>();
        if (debrisComponent != null)
        {
            debrisComponent.Initialize(spawnPoint, direction);
            activeDebrisCount++;
            
            // Register for destruction to update count
            DestroyListener destroyListener = debris.AddComponent<DestroyListener>();
            destroyListener.OnDestroyed += OnDebrisDestroyed;
            
            if (debugMode)
            {
                //Debug.Log($"Spawned {typeName} debris at {spawnPoint}. Active count: {activeDebrisCount}");
            }
        }
    }
    
    /// Called when a debris object is destroyed
    private void OnDebrisDestroyed()
    {
        activeDebrisCount--;
        
        if (debugMode)
        {
            //Debug.Log($"Debris destroyed. Remaining count: {activeDebrisCount}");
        }
    }
    
    /// Simple component to listen for object destruction
    private class DestroyListener : MonoBehaviour
    {
        public System.Action OnDestroyed;
        
        private void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }
    }
}