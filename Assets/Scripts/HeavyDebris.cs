// HeavyDebris.cs

using UnityEngine;

/// Heavy debris type implementation - high mass and larger size with less gravitational influence.
public class HeavyDebris : DebrisBase
{
    [Header("Heavy Debris Properties")]
    [SerializeField] private float heavyMass = 3.0f;
    [SerializeField] private float heavySize = 0.8f;
    
    /// Initialize heavy debris specific properties
    protected override void Awake()
    {
        // Set heavy debris specific properties
        mass = heavyMass;
        size = heavySize;
        
        // Call base implementation for common setup
        base.Awake();
    }
    
    public override void Initialize(Vector2 position, Vector2 direction)
    {
        // Apply a small random variation to the heavy properties (±10%)
        mass = heavyMass * Random.Range(0.9f, 1.1f);
        size = heavySize * Random.Range(0.9f, 1.1f);
        
        // Set collision properties
        bounciness = 0.3f;  // Less bouncy due to mass
        friction = 0.4f;    // More friction
        
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

    /// Gets the gravity response factor for heavy debris
    public override float GetGravityResponseFactor()
    {
        // Heavy debris is less affected by gravity due to higher mass
        return 0.8f / mass; // 80% of the base response
    }

    // Add to HeavyDebris.cs
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if colliding with standard debris
        if (collision.gameObject.CompareTag("Debris"))
        {
            StandardDebris standardDebris = collision.gameObject.GetComponent<StandardDebris>();
            if (standardDebris != null)
            {
                // Heavy debris is less affected by collision, so we don't need to add any
                // additional force to the heavy debris itself
                
                // Debug.Log("Heavy-to-standard collision detected");
            }
        }
    }
}