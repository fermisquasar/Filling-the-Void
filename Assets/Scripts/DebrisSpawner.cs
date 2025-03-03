// DebrisSpawner.cs

using UnityEngine;
using System.Collections.Generic;

public class DebrisSpawner: MonoBehaviour
{
    [SerializeField] private GameObject debrisPrefab; // Prefab refrence for debris objects
    [SerializeField] private int poolSize = 20; // The amount of debris to maintain in the pool 
    [SerializeField] private float spawnInterval = 2f; // Interval between debris spawning
    [SerializeField] private float minSpeed = 1f; // Min and max speed range for debris
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float safeRadius = 2.5f; // NEW: Minimum safe distance from center

    private List<Debris> debrisPool = new List<Debris>(); // Object pool for debris
    private float nextSpawnTime = 0f; // Tracks when the next debris spawn should occur
    private Camera mainCam; // Refrence to the main camera in the scene. Used for screen boundary calculation

    private void Start()
    {
        mainCam = Camera.main; // Assign the main camera

        // Initialize object pool
        for (int i = 0; i < poolSize; i++)
        {
            GameObject newDebris = Instantiate(debrisPrefab, Vector3.zero, Quaternion.identity);
            Debris debris = newDebris.GetComponent<Debris>();
            newDebris.SetActive(false); // Starts debris as inactive
            debrisPool.Add(debris);
        }
    }

    private void Update()
    {
        // Check if it's time to spawn more debris
        if (Time.time >= nextSpawnTime)
        {
            SpawnDebris();
            nextSpawnTime = Time.time + spawnInterval; // Reset spawn timer
        }
    }

    // Spawns debris object from the pool at a random edge of the screen with a random velocity
    private void SpawnDebris()
    {
        // Find inactive debris from pool
        Debris debris = GetInactiveDebris();
        if (debris == null) return; // If no inactive debris, return

        Vector2 spawnPos = GetRandomEdgePosition(); // Get random spawn position on screen edge

        Vector2 targetPos = GetRandomPositionAwayFromEdge(spawnPos); // Get initial velocity (random direction across screen)
        Vector2 direction = (targetPos - spawnPos).normalized; // Get movement direction
        float speed = Random.Range(minSpeed, maxSpeed); // Assign a random speed within min and max range
        Vector2 velocity = direction * speed; // Compute velocity based on direction and speed

        debris.Initialize(spawnPos, velocity); // Initialize debris
    } 

    // Retrieves an inactive debris object from the pool
    private Debris GetInactiveDebris()
    {
        foreach (Debris debris in debrisPool)
        {
            if (!debris.gameObject.activeInHierarchy)
            {
                return debris; // Returns the firt available debris
            }
        }
        return null; // If all debris are active, return null or expand pool (REQUIRES IMPLEMENTATION)
    }

    // Calculate a random target position away from screen's edge, creates a curve to that position (avoiding the center)
    private Vector2 GetRandomPositionAwayFromEdge(Vector2 spawnPos)
    {
        Vector2 screenBounds = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
        Vector2 center = Vector2.zero; // Void/Center position
        
        Vector2 fromCenter = spawnPos - center; // Calculate the vector from the center to the spawn position
        
        // Calculate perpendicular vectors (clockwise and counter-clockwise)
        Vector2 perpClockwise = new Vector2(-fromCenter.y, fromCenter.x).normalized;
        Vector2 perpCounterClockwise = new Vector2(fromCenter.y, -fromCenter.x).normalized;
        
        // Choose one of these vector randomly, with bias toward curves
        Vector2 perpDirection = Random.value > 0.3f ? perpClockwise : perpCounterClockwise;
        
        // Create a curved path by combining radial and perpendicular components
        float radialDistance = Random.Range(safeRadius * 1.2f, safeRadius * 2.0f);
        float tangentialDistance = Random.Range(screenBounds.magnitude * 0.3f, screenBounds.magnitude * 0.7f);
        
        Vector2 targetPos = center + (fromCenter.normalized * radialDistance) + (perpDirection * tangentialDistance);
        
        // Confirms the target is in screen bounds
        targetPos.x = Mathf.Clamp(targetPos.x, -screenBounds.x * 0.9f, screenBounds.x * 0.9f);
        targetPos.y = Mathf.Clamp(targetPos.y, -screenBounds.y * 0.9f, screenBounds.y * 0.9f);
        
        return targetPos;
    }

    // Generate a random position on the screen's edge
    private Vector2 GetRandomEdgePosition()
    {
        Vector2 screenBounds = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height));
        float x, y;

        // Randomly choose wheter to spawn on the horizontal or vertical edges of screen
        if (Random.value < 0.5f)
        {
            // Spawn on left or right edge
            x = Random.value < 0.5f ? -screenBounds.x : screenBounds.x;
            y = Random.Range(-screenBounds.y, screenBounds.y);
        }
        else
        {
            // Spawn on top or bottom edge
            x = Random.Range(-screenBounds.x, screenBounds.x);
            y = Random.value < 0.5f ? -screenBounds.y: screenBounds.y;
        }

        return new Vector2(x, y);
    }
}