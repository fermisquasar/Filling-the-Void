// FragileDebris.cs

using UnityEngine;

/// Fragile debris type implementation - small to medium size that will eventually break upon collision.
public class FragileDebris : DebrisBase
{
    [Header("Fragile Debris Properties")]
    [SerializeField] private float fragileMass = 0.5f;
    [SerializeField] private float fragileSize = 0.45f;
    
    /// Initialize fragile debris specific properties
    protected override void Awake()
    {
        // Set fragile debris specific properties
        mass = fragileMass;
        size = fragileSize;
        
        // Call base implementation for common setup
        base.Awake();
    }
    
    /// Initialize the fragile debris with a position and direction
    public override void Initialize(Vector2 position, Vector2 direction)
    {
        // Apply a small random variation to the fragile properties (±15%)
        mass = fragileMass * Random.Range(0.85f, 1.15f);
        size = fragileSize * Random.Range(0.85f, 1.15f);
        
        // Update rigidbody properties
        rb.mass = mass;
        
        // Update transform scale
        transform.localScale = new Vector3(size, size, 1.0f);
        
        // Call base implementation for common initialization
        base.Initialize(position, direction);
    }
    
    // In FragileDebris.cs
    public override float GetGravityResponseFactor()
    {
        // Fragile debris has slightly higher gravity response
        return 1.2f / mass;
    }
    // Note: The actual breaking behavior will be implemented in a later phase
}