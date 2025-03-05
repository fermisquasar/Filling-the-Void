// DebrisBase.cs

using UnityEngine;

/// Base class for all debris types in the game.
public class DebrisBase : MonoBehaviour
{
    [Header("Physical Properties")]
    [SerializeField] public float mass = 1.0f;
    [SerializeField] protected float size = 1.0f;
    [SerializeField] protected float drag = 0.05f;
    [SerializeField] protected float angularDrag = 0.05f;
    
    [Header("Movement")]
    [SerializeField] protected float initialVelocityMin = 1.0f;
    [SerializeField] protected float initialVelocityMax = 3.0f;
    
    [Header("Visual")]
    [SerializeField] protected Color debrisColor = Color.white;
    [SerializeField] protected bool showDebrisType = true;

    [Header("Collision Properties")]
    [SerializeField] protected float bounciness = 0.5f;  // How bouncy the debris is
    [SerializeField] protected float friction = 0.2f;    // Friction coefficient for collisions

    // Reference to the rigidbody component
    protected Rigidbody2D rb;
    
    // Reference to the debris manager
    protected DebrisManager debrisManager;
    
    // Reference to the sprite renderer
    protected SpriteRenderer spriteRenderer;

    protected virtual void Awake()
    {
        // Get or add a Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // Get sprite renderer
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            // If no sprite renderer, try to find it in children
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        // Apply physical properties to the rigidbody
        rb.mass = mass;
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
        
        // Apply size to transform
        transform.localScale = new Vector3(size, size, 1.0f);
        
        // Apply color to sprite renderer if available
        if (spriteRenderer != null)
        {
            spriteRenderer.color = debrisColor;
        }

        // Apply physical properties to the rigidbody
        rb.mass = mass;
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
        
        // Configure physics material for collision properties
        PhysicsMaterial2D physicsMaterial = new PhysicsMaterial2D();
        physicsMaterial.bounciness = bounciness;
        physicsMaterial.friction = friction;
        
        // Apply physics material to collider
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.sharedMaterial = physicsMaterial;
        }
        
        // Set the appropriate tag for all debris objects
        gameObject.tag = "Debris";
    }

    protected virtual void Start()
    {
        // Find the debris manager in the scene
        debrisManager = FindFirstObjectByType<DebrisManager>();
        if (debrisManager == null)
        {
            Debug.LogWarning("DebrisManager not found in the scene!");
        }
    }

    protected virtual void Update()
    {
        // Check if the debris has left the field boundaries
        if (debrisManager != null && !debrisManager.IsWithinFieldBoundaries(transform.position))
        {
            DespawnDebris();
        }
    }

    /// Initialize the debris with a position and direction
    public virtual void Initialize(Vector2 position, Vector2 direction)
    {
        // Set position
        transform.position = position;
        
        // Apply initial velocity
        float initialVelocity = Random.Range(initialVelocityMin, initialVelocityMax);
        rb.linearVelocity = direction.normalized * initialVelocity;
        
        // Add some random rotation
        rb.angularVelocity = Random.Range(-90f, 90f);
    }
    
    /// Handles despawning of debris when it leaves the field or is otherwise removed
    protected virtual void DespawnDebris()
    {
        // In a more advanced implementation, this would return the object to a pool
        Destroy(gameObject);
    }
    
    /// Gets the user-friendly name of the debris type
    protected virtual string GetDebrisTypeName()
    {
        // Base implementation - child classes should override
        if (this is StandardDebris)
            return "Standard";
        else if (this is HeavyDebris)
            return "Heavy";
        else if (this is StickyDebris)
            return "Sticky";
        else if (this is FragileDebris)
            return "Fragile";
        else
            return "Unknown";
    }

    /// Gets the gravity response factor based on debris mass
    public virtual float GetGravityResponseFactor()
    {
        // Base implementation - more mass means less gravitational influence
        // Return an inverse relationship with mass (higher mass = lower response)
        return 1.0f / mass;
    }
}