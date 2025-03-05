// DebrisCollector.cs

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

/// Handles collection and expulsion of debris when the planet comes in contact with debris objects.
public class DebrisCollector : MonoBehaviour
{
    [Header("Collection Settings")]
    [SerializeField] private float gravityMultiplierOnCollection = 3f;
    [SerializeField] private bool enableCollection = true;
    
    [Header("Capacity Settings")]
    [SerializeField] private int maxDebrisCapacity = 20;
    [SerializeField] private int currentDebrisCount = 0;
    
    [Header("Expulsion Settings")]
    [SerializeField] private KeyCode inwardExpulsionKey = KeyCode.Q;
    [SerializeField] private KeyCode outwardExpulsionKey = KeyCode.E;
    [SerializeField] private float baseExpulsionForce = 5f;
    [SerializeField] private float expulsionForcePerDebris = 0.5f;
    [SerializeField] private GameObject debrisExpulsionPrefab;
    
    // Properties to expose capacity info
    public int CurrentDebrisCount => currentDebrisCount;
    public int MaxDebrisCapacity => maxDebrisCapacity;
    public bool IsAtCapacity => currentDebrisCount >= maxDebrisCapacity;
    
    // UI references - make them public but separate in the inspector
    [Header("UI References")]
    public TMP_Text capacityText;
    public Image capacityFillBar;
    public GameObject collectingIndicator;
    
    // Reference to the planet's gravity component
    private PlanetGravity planetGravity;
    
    // Reference to the black hole transform for inward expulsion
    private Transform blackHoleTransform;
    
    // Reference to the debris manager for outward expulsion
    private DebrisManager debrisManager;
    
    // Cache the original gravity strength
    private float originalGravityStrength;
    
    // Cache for debris prefabs (optional for expulsion)
    private GameObject standardDebrisPrefab;
    
    private void Start()
    {
        // Get the PlanetGravity component
        planetGravity = GetComponent<PlanetGravity>();
        
        if (planetGravity == null)
        {
            Debug.LogError("PlanetGravity component not found on the planet!");
            enableCollection = false;
            return;
        }
        
        // Find the black hole in the scene for inward expulsion
        GameObject blackHole = GameObject.FindWithTag("BlackHole");
        if (blackHole != null)
        {
            blackHoleTransform = blackHole.transform;
        }
        else
        {
            Debug.LogWarning("Black hole not found in the scene. Inward expulsion may not work correctly.");
        }
        
        // Find the debris manager for outward expulsion
        debrisManager = FindFirstObjectByType<DebrisManager>();
        if (debrisManager == null)
        {
            Debug.LogWarning("DebrisManager not found in the scene. Outward expulsion may not work correctly.");
        }
        
        // Cache the original gravity strength from the PlanetGravity component
        originalGravityStrength = planetGravity.GetGravitationalStrength();
        
        // Find standard debris prefab (for expulsion)
        DebrisSpawner spawner = FindFirstObjectByType<DebrisSpawner>();
        if (spawner != null)
        {
            // Note: In a complete implementation, we would need to have a reference to debris prefabs.
            // This would typically be set in the inspector or shared through a central manager.
            // For this phase, we'll just create a simple debris object during expulsion.
        }
    }
    
    private void Update()
    {
        // Check for space bar input to increase gravity
        if (enableCollection && planetGravity != null)
        {
            bool isCollecting = Input.GetKey(KeyCode.Space) && !IsAtCapacity;
            
            if (isCollecting)
            {
                // Increase gravity strength while space bar is held using the multiplier
                planetGravity.SetGravitationalStrength(originalGravityStrength * gravityMultiplierOnCollection);
            }
            else
            {
                // Return to original gravity strength when space bar is released or at capacity
                planetGravity.SetGravitationalStrength(originalGravityStrength);
            }
            
            // Update collecting indicator in UI
            if (collectingIndicator != null)
            {
                collectingIndicator.SetActive(isCollecting);
            }
        }
        
        // Check for expulsion input
        if (currentDebrisCount > 0)
        {
            // Inward expulsion (toward black hole)
            if (Input.GetKeyDown(inwardExpulsionKey))
            {
                ExpelDebris(true);
            }
            
            // Outward expulsion (toward field edge)
            if (Input.GetKeyDown(outwardExpulsionKey))
            {
                ExpelDebris(false);
            }
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if we're colliding with debris
        if (collision.gameObject.CompareTag("Debris"))
        {
            HandleDebrisCollision(collision.gameObject);
        }
    }
    
    /// Handles debris collision with the planet
    private void HandleDebrisCollision(GameObject debrisObject)
    {
        // Get the debris component
        DebrisBase debris = debrisObject.GetComponent<DebrisBase>();
        
        if (debris != null)
        {
            // Only collect debris if space bar is held and not at capacity
            if (enableCollection && Input.GetKey(KeyCode.Space) && !IsAtCapacity)
            {
                // Handle collection
                CollectDebris(debris);
            }
            else
            {
                // When not collecting, apply a bounce effect
                Rigidbody2D debrisRb = debrisObject.GetComponent<Rigidbody2D>();
                if (debrisRb != null)
                {
                    // Calculate the bounce direction (away from planet center)
                    Vector2 bounceDirection = ((Vector2)debrisObject.transform.position - 
                                            (Vector2)transform.position).normalized;
                    
                    // Calculate bounce force based on debris type
                    float bounceFactor = 1.0f;
                    
                    // Adjust bounce factor based on debris type
                    if (debris is HeavyDebris)
                    {
                        bounceFactor = 0.8f; // Heavier debris bounces less
                    }
                    else if (debris is StandardDebris)
                    {
                        bounceFactor = 1.0f; // Normal bounce
                    }
                    else if (debris is FragileDebris)
                    {
                        bounceFactor = 1.2f; // Fragile debris bounces more
                    }
                    else if (debris is StickyDebris)
                    {
                        bounceFactor = 0.6f; // Sticky debris bounces less
                    }
                    
                    // Calculate the bounce force - based on incoming velocity and bounce factor
                    float incomingSpeed = debrisRb.linearVelocity.magnitude;
                    float bounceForce = Mathf.Max(incomingSpeed * bounceFactor, 2.0f); // Ensure minimum force
                    
                    // Apply bounce force
                    debrisRb.linearVelocity = bounceDirection * bounceForce;
                    
                    // Add some random torque for more natural bouncing
                    debrisRb.AddTorque(Random.Range(-10f, 10f), ForceMode2D.Impulse);
                }
            }
        }
    }
    
    /// Collects the debris by destroying it and updating capacity>
    private void CollectDebris(DebrisBase debris)
    {
        // Increment the debris count
        currentDebrisCount++;
        
        // Update UI if references exist
        UpdateCapacityUI();
        
        // Destroy the debris object
        Destroy(debris.gameObject);
    }

    /// Updates the capacity UI elements
    private void UpdateCapacityUI()
    {
        if (capacityText != null)
        {
            capacityText.text = $"{currentDebrisCount} / {maxDebrisCapacity}";
        }
        
        if (capacityFillBar != null)
        {
            capacityFillBar.fillAmount = (float)currentDebrisCount / maxDebrisCapacity;
        }
    }
    
    /// Expels all collected debris in a specified direction
    private void ExpelDebris(bool inward)
    {
        if (currentDebrisCount <= 0)
            return;
        
        // Calculate expulsion force based on collected amount
        float totalForce = baseExpulsionForce + (expulsionForcePerDebris * currentDebrisCount);
        
        // Determine the number of debris objects to expel (all of them)
        int debrisToExpel = currentDebrisCount;
        
        // Spawn and expel the debris
        for (int i = 0; i < debrisToExpel; i++)
        {
            // Calculate random spawn position slightly offset from the planet
            Vector2 spawnOffset = Random.insideUnitCircle.normalized * 0.75f;
            Vector2 spawnPosition = (Vector2)transform.position + spawnOffset;
            
            // Determine direction based on inward/outward mode
            Vector2 expelDirection;
            
            if (inward && blackHoleTransform != null)
            {
                // Base direction toward black hole from planet center
                Vector2 baseDirection = ((Vector2)blackHoleTransform.position - (Vector2)transform.position).normalized;
                
                // Add a small random offset to the direction (within a narrow cone toward the black hole)
                float randomAngle = Random.Range(-15f, 15f) * Mathf.Deg2Rad; // 30 degree cone
                Vector2 randomOffset = new Vector2(
                    Mathf.Cos(randomAngle) * baseDirection.x - Mathf.Sin(randomAngle) * baseDirection.y,
                    Mathf.Sin(randomAngle) * baseDirection.x + Mathf.Cos(randomAngle) * baseDirection.y
                );
                
                // Final direction is still generally toward the black hole, but with slight variation
                expelDirection = (baseDirection + randomOffset * 0.2f).normalized;
                
                // Spawn position with variation, but still along the general path toward the black hole
                // Use a dispersed ring around the planet in the general direction of the black hole
                float angleVariation = Random.Range(-45f, 45f) * Mathf.Deg2Rad;
                Vector2 spawnVariation = new Vector2(
                    Mathf.Cos(angleVariation) * baseDirection.x - Mathf.Sin(angleVariation) * baseDirection.y,
                    Mathf.Sin(angleVariation) * baseDirection.x + Mathf.Cos(angleVariation) * baseDirection.y
    );
    
    spawnPosition = (Vector2)transform.position + (spawnVariation.normalized * 0.75f);
            }
            else
            {
                // For outward expulsion, simply expel away from the center of the screen
                // Center of the screen is assumed to be at (0,0) or can be obtained from the camera
                Vector2 screenCenter = Vector2.zero; // Or use Camera.main.transform.position
                
                // Calculate direction from screen center to planet
                expelDirection = ((Vector2)transform.position - screenCenter).normalized;
                
                // Add a small random variation to spread the debris
                float randomAngle = Random.Range(-30f, 30f) * Mathf.Deg2Rad;
                Vector2 randomOffset = new Vector2(
                    Mathf.Cos(randomAngle) * expelDirection.x - Mathf.Sin(randomAngle) * expelDirection.y,
                    Mathf.Sin(randomAngle) * expelDirection.x + Mathf.Cos(randomAngle) * expelDirection.y
                );
                
                // Apply random variation
                expelDirection = (expelDirection + randomOffset * 0.3f).normalized;
            }
            
            // Spawn the debris object
            SpawnExpelledDebris(spawnPosition, expelDirection, totalForce);
        }
        
        // Reset debris count
        currentDebrisCount = 0;
        
        // Update UI
        UpdateCapacityUI();
    }
    
    /// Spawns a debris object and applies an expulsion force
    private void SpawnExpelledDebris(Vector2 position, Vector2 direction, float force)
    {
        GameObject debrisObject;
        
        // Use the provided prefab if available, otherwise create a simple object
        if (debrisExpulsionPrefab != null)
        {
            debrisObject = Instantiate(debrisExpulsionPrefab, position, Quaternion.identity);
        }
        else
        {
            // Create a simple debris object with required components
            debrisObject = new GameObject("ExpelledDebris");
            debrisObject.transform.position = position;
            
            // Add required components
            SpriteRenderer renderer = debrisObject.AddComponent<SpriteRenderer>();
            renderer.sprite = Resources.Load<Sprite>("DefaultDebris");
            if (renderer.sprite == null)
            {
                // Create a circle shape if sprite is not available
                debrisObject.AddComponent<CircleCollider2D>();
            }
            
            // Add rigidbody for physics
            Rigidbody2D rb = debrisObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            
            // Set tag for identification and interaction
            debrisObject.tag = "Debris";
        }
        
        // Get the rigidbody and apply force
        Rigidbody2D debrisRb = debrisObject.GetComponent<Rigidbody2D>();
        if (debrisRb != null)
        {
            // Apply the expulsion force
            debrisRb.AddForce(direction * force, ForceMode2D.Impulse);
            
            // Add random rotation
            debrisRb.angularVelocity = Random.Range(-90f, 90f);
        }
        
        // Add a basic debris script if possible
        if (debrisObject.GetComponent<DebrisBase>() == null)
        {
            // For simple implementation, add a component to destroy when leaving the field
            DespawnOutOfBounds despawner = debrisObject.AddComponent<DespawnOutOfBounds>();
            despawner.debrisManager = debrisManager;
        }
    }
    
    /// Helper component to destroy debris when it leaves the field boundaries
    private class DespawnOutOfBounds : MonoBehaviour
    {
        [HideInInspector] public DebrisManager debrisManager;
        
        private void Update()
        {
            if (debrisManager != null && !debrisManager.IsWithinFieldBoundaries(transform.position))
            {
                Destroy(gameObject);
            }
        }
    }

/// Gets the field size from the debris manager using reflection
private Vector2 GetFieldSize()
{
    // Default size in case we can't access the actual size
    Vector2 defaultSize = new Vector2(20f, 20f);
    
    if (debrisManager == null)
        return defaultSize;
        
    // Try to get the field size through reflection (since it's private in DebrisManager)
    System.Reflection.FieldInfo fieldSizeInfo = typeof(DebrisManager).GetField("fieldSize", 
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    
    if (fieldSizeInfo != null)
    {
        Vector2 fieldSize = (Vector2)fieldSizeInfo.GetValue(debrisManager);
        return fieldSize;
    }
    
    return defaultSize;
    }
}