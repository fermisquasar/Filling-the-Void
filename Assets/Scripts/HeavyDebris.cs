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
    
    /// Initialize the heavy debris with a position and direction
    public override void Initialize(Vector2 position, Vector2 direction)
    {
        // Apply a small random variation to the heavy properties (±10%)
        mass = heavyMass * Random.Range(0.9f, 1.1f);
        size = heavySize * Random.Range(0.9f, 1.1f);
        
        // Update rigidbody properties
        rb.mass = mass;
        
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
}