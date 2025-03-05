// StandardDebris.cs

using UnityEngine;

/// Standard debris type implementation - medium mass and size with normal physics interactions.

public class StandardDebris : DebrisBase
{
    [Header("Standard Debris Properties")]
    [SerializeField] private float standardMass = 1.0f;
    [SerializeField] private float standardSize = 0.5f;
    
    /// Initialize standard debris specific properties
    protected override void Awake()
    {
        // Set standard debris specific properties
        mass = standardMass;
        size = standardSize;
        
        // Call base implementation for common setup
        base.Awake();
    }
    
    /// Initialize the standard debris with a position and direction
    public override void Initialize(Vector2 position, Vector2 direction)
    {
        // Apply a small random variation to the standard properties (±10%)
        mass = standardMass * Random.Range(0.9f, 1.1f);
        size = standardSize * Random.Range(0.9f, 1.1f);
        bounciness = 0.5f;  // Standard bounciness
        friction = 0.2f;    // Standard friction
        
        // Update rigidbody properties
        rb.mass = mass;
        
        // Update collider properties
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            PhysicsMaterial2D physicsMaterial = new PhysicsMaterial2D();
            physicsMaterial.bounciness = bounciness;
            physicsMaterial.friction = friction;
            collider.sharedMaterial = physicsMaterial;
        }
        
        // Update transform scale
        transform.localScale = new Vector3(size, size, 1.0f);
        
        // Call base implementation for common initialization
        base.Initialize(position, direction);
    }
    
    // In StandardDebris.cs
    public override float GetGravityResponseFactor()
    {
        // Standard debris has normal gravity response
        return 1.0f / mass;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if colliding with another debris
        if (collision.gameObject.CompareTag("Debris"))
        {
            // Check for standard-to-standard collision
            StandardDebris otherStandardDebris = collision.gameObject.GetComponent<StandardDebris>();
            if (otherStandardDebris != null)
            {
                // Normal physics handles this
                // Debug.Log("Standard-to-standard collision detected");
                return;
            }
            
            // Check for standard-to-heavy collision
            HeavyDebris heavyDebris = collision.gameObject.GetComponent<HeavyDebris>();
            if (heavyDebris != null)
            {
                // Standard debris is significantly affected by collision with heavy debris
                // Calculate an additional impulse force
                float heavyImpactFactor = 1.5f; // Heavy debris has more significant impact
                Vector2 collisionNormal = collision.contacts[0].normal;
                float heavyMass = heavyDebris.GetComponent<Rigidbody2D>().mass;
                Vector2 additionalForce = -collisionNormal * heavyImpactFactor * heavyMass;
                
                // Apply additional force to represent the heavier impact
                rb.AddForce(additionalForce, ForceMode2D.Impulse);
                
                // Debug.Log("Standard-to-heavy collision detected");
            }
        }
    }
}