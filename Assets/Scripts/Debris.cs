// Debris.cs

using System.Numerics;
using UnityEngine;


public class Debris : MonoBehaviour
{
    public float mass = 1f; // Mass of the debris, affecting its response to gravity
    [SerializeField] private float maxSpeed = 10f; // Maximum speed the debris can reach, prevents things from getting too crazy
    private UnityEngine.Vector2 velocity; // Current velocity of debris
    private bool isActive = false;  // Tracks is the debris is active in the scene
    public UnityEngine.Vector2 Position => transform.position; // Public property to get the position of the debris

    // Initializes debris with a postion and velocity
    public void Initialize(UnityEngine.Vector2 position, UnityEngine.Vector2 initialVelocity)
    {
        transform.position = position; // Set starting position
        velocity = initialVelocity; // Set starting speed
        isActive = true; // For keeping track of active status
        gameObject.SetActive(true); // Activate the debris in the scene
    }

    // Applies force to debris, changing it's velocity
    public void ApplyForce(UnityEngine.Vector2 force)
    {
        UnityEngine.Vector2 acceleration = force / mass; // Newton's Second Law; F = ma, rewritten as a = F/m
        velocity += acceleration * Time.fixedDeltaTime; // Update velocity based on accelration

        // Cap max speed
        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }
    }

    // Handles movement and checks boundarys
    private void FixedUpdate()
    {
        if(!isActive) return;  // Return if debris is not active
        
        transform.position += (UnityEngine.Vector3)velocity * Time.fixedDeltaTime; // Move debris based on velocity

        // Store screen boundaris
        UnityEngine.Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new UnityEngine.Vector3(Screen.width, Screen.height));
        // Check if the debris is off screen, if so reset it
        if (Mathf.Abs(transform.position.x) > screenBounds.x * 1.5f ||
            Mathf.Abs(transform.position.y) > screenBounds.y * 1.5f)
            {
                ResetDebris();
            }
    }

    // Deactivates and resets debris, returning it to the pool
    public void ResetDebris()
    {
        isActive = false;
        gameObject.SetActive(false); // Return to object pool or destroy
    }
}