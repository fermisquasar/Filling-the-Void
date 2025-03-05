// PlanetGravity.cs

using System.Collections.Generic;
using UnityEngine;

/// Handles the gravitational influence zone around the planet
public class PlanetGravity : MonoBehaviour
{
    [Header("Gravitational Properties")]
    [SerializeField] private float influenceRadius = 3f;
    [SerializeField] private float gravitationalStrength = 5f;
    [SerializeField] private bool showInfluenceZone = false;
    [SerializeField] private Color influenceZoneColor = new Color(0.5f, 0.5f, 1f, 0.2f);

    // Reference to the core controller
    private PlanetController planetController;
    
    // Trigger collider for the gravitational zone
    private CircleCollider2D influenceZoneCollider;
    
    // List of objects currently within the influence zone
    private List<Rigidbody2D> affectedObjects = new List<Rigidbody2D>();

    private void Awake()
    {
        // Get reference to the planet controller
        planetController = GetComponent<PlanetController>();
        
        // Ensure we have the core controller
        if (planetController == null)
        {
            Debug.LogError("PlanetController component is missing from planet GameObject!");
        }
        
        // Create or set up the influence zone collider
        SetupInfluenceZoneCollider();
    }

    private void FixedUpdate()
    {
        // Apply gravitational pull to all affected objects
        ApplyGravitationalPull();
    }

    /// Sets up the collider for the gravitational influence zone
    private void SetupInfluenceZoneCollider()
    {
        // Create a new GameObject for the influence zone
        GameObject influenceZoneObject = new GameObject("GravitationalInfluenceZone");
        influenceZoneObject.transform.parent = transform;
        influenceZoneObject.transform.localPosition = Vector3.zero;
        
        // Add the trigger collider
        influenceZoneCollider = influenceZoneObject.AddComponent<CircleCollider2D>();
        influenceZoneCollider.isTrigger = true;
        influenceZoneCollider.radius = influenceRadius;
        
        // Add a trigger handler component
        GravityTriggerHandler triggerHandler = influenceZoneObject.AddComponent<GravityTriggerHandler>();
        triggerHandler.Initialize(this);
    }

    /// Applies gravitational pull to all objects within the influence zone
    private void ApplyGravitationalPull()
    {
        for (int i = affectedObjects.Count - 1; i >= 0; i--)
        {
            Rigidbody2D rb = affectedObjects[i];
            
            // Skip if the object is null or has been destroyed
            if (rb == null)
            {
                affectedObjects.RemoveAt(i);
                continue;
            }
            
            // Calculate direction towards the planet
            Vector2 direction = (Vector2)transform.position - rb.position;
            float distance = direction.magnitude;
            
            // Normalize the direction
            direction.Normalize();
            
            // Calculate force strength (linear falloff with distance)
            float strengthFactor = 1f - (distance / influenceRadius);
            strengthFactor = Mathf.Clamp01(strengthFactor); // Ensure it's between 0 and 1
            
            // Get the debris-specific gravity response factor
            float gravityResponseFactor = 1.0f; // Default value
            DebrisBase debris = rb.GetComponent<DebrisBase>();
            if (debris != null)
            {
                gravityResponseFactor = debris.GetGravityResponseFactor();
            }
            
            // Calculate the final force, considering debris mass
            Vector2 force = direction * gravitationalStrength * strengthFactor * gravityResponseFactor;
            
            // Apply the force to the object
            rb.AddForce(force);
        }
    }
    /// Adds an object to the list of affected objects
    public void AddAffectedObject(Rigidbody2D rb)
    {
        if (rb != null && !affectedObjects.Contains(rb))
        {
            affectedObjects.Add(rb);
        }
    }

    /// Removes an object from the list of affected objects
    public void RemoveAffectedObject(Rigidbody2D rb)
    {
        if (rb != null)
        {
            affectedObjects.Remove(rb);
        }
    }

    /// Updates the radius of the influence zone
    public void SetInfluenceRadius(float newRadius)
    {
        influenceRadius = newRadius;
        if (influenceZoneCollider != null)
        {
            influenceZoneCollider.radius = influenceRadius;
        }
    }

    /// Gets the current influence radius
    public float GetInfluenceRadius()
    {
        return influenceRadius;
    }

    /// Sets the gravitational strength
    public void SetGravitationalStrength(float newStrength)
    {
        gravitationalStrength = newStrength;
    }

    /// Gets the current gravitational strength
    public float GetGravitationalStrength()
    {
        return gravitationalStrength;
    }

    /// Draws the influence zone as a gizmo
    private void OnDrawGizmos()
    {
        if (!showInfluenceZone)
            return;
            
        Gizmos.color = influenceZoneColor;
        Gizmos.DrawSphere(transform.position, influenceRadius);
    }
}

/// Helper component that handles the trigger events for the gravitational zone
public class GravityTriggerHandler : MonoBehaviour
{
    private PlanetGravity planetGravity;

    /// Initializes the trigger handler with a reference to the parent gravity component
    public void Initialize(PlanetGravity gravity)
    {
        planetGravity = gravity;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only affect objects with Rigidbody2D that are debris or consumable
        if (other.CompareTag("Debris"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                planetGravity.AddAffectedObject(rb);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Remove objects that exit the zone
        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            planetGravity.RemoveAffectedObject(rb);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Ensure the object is still in the list (redundancy check)
        if (other.CompareTag("Debris"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                planetGravity.AddAffectedObject(rb);
            }
        }
    }
}