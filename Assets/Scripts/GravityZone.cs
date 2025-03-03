// GravityZone.cs

using UnityEngine;
using System.Collections.Generic;

public class GravityZone : MonoBehaviour
{
    public float radius = 1f; // Radius of gravitational influence
    public float gravityStrength = 1f; // Gravity strength
    public bool isVoid = false; // Check if void or planet

    [SerializeField] private bool showDebugRadius = true; // Toggle to show degug radius in Unity editor

    private HashSet<Debris> affectedDebris = new HashSet<Debris>(); // A set to track debris currently affected by gravity zone

    // Called when an object enters gravity's zone of influence
    private void OnTriggerEnter2D(Collider2D other)
    {  
        // Check if the object is debris
        Debris debris = other.GetComponent<Debris>();
        if (debris != null)
        {
            affectedDebris.Add(debris);  // Add debris to the set

            // If this is the void, consume the debris
            if(isVoid)
            {
                // Trigger any void consumption effects (NEEDS IMPLEMENTATION)
                debris.ResetDebris(); // Return to pool or destory
            }
        }
    }

    // Called at a fixed interval to apply gravitational forced to affected debris
    private void FixedUpdate()
    {
        foreach (Debris debris in affectedDebris)
        {
            // Check if the debris is still valid before applying force
            if (debris == null) continue;

            // Calculate the direction vector from the debris to the center of the gravity zone of influence
            Vector2 direction = (Vector2)transform.position - debris.Position;
            float distance = direction.magnitude;

            if (distance > 0)
            {
                // Calculate gravitational force (F = G * (m1 * m2 / r^2))
                float forceMagnitude = gravityStrength / (distance * distance);
                UnityEngine.Vector2 force = direction.normalized * forceMagnitude;

                debris.ApplyForce(force); // Apply force to debris
            }
        }
    }

    // Draw a debug display to show gravity's zone of influence
    private void OnDrawGizmos()
    {
        if (showDebugRadius)
        {
            // Colors the planet and Void's gizmos differently
            Gizmos.color = isVoid ? Color.red : Color.blue;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
