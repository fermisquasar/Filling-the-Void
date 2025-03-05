// StickyDebris.cs

using UnityEngine;

/// Sticky debris type implementation - small size that will eventually adhere to other debris.
public class StickyDebris : DebrisBase
{
    [Header("Sticky Debris Properties")]
    [SerializeField] private float stickyMass = 0.7f;
    [SerializeField] private float stickySize = 0.4f;
    
    /// Initialize sticky debris specific properties
    protected override void Awake()
    {
        // Set sticky debris specific properties
        mass = stickyMass;
        size = stickySize;
        
        // Call base implementation for common setup
        base.Awake();
    }
    
    /// Initialize the sticky debris with a position and direction
    public override void Initialize(Vector2 position, Vector2 direction)
    {
        // Apply a small random variation to the sticky properties (±10%)
        mass = stickyMass * Random.Range(0.9f, 1.1f);
        size = stickySize * Random.Range(0.9f, 1.1f);
        
        // Update rigidbody properties
        rb.mass = mass;
        
        // Update transform scale
        transform.localScale = new Vector3(size, size, 1.0f);
        
        // Call base implementation for common initialization
        base.Initialize(position, direction);
    }
    
    // In StickyDebris.cs
    public override float GetGravityResponseFactor()
    {
        // Sticky debris has normal gravity response
        return 1.0f / mass;
    }

    // Note: The actual sticking behavior will be implemented in a later phase
}