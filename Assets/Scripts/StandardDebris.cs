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
        
        // Update rigidbody properties
        rb.mass = mass;
        
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
}