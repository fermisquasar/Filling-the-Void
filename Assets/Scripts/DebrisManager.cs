// DebrisManager.cs

using UnityEngine;
using System.Collections.Generic;

/// Manages the debris system, including spawning, field boundaries, and global debris behavior.
public class DebrisManager : MonoBehaviour
{
    [Header("Debris Field")]
    [SerializeField] private Vector2 fieldSize = new Vector2(20f, 20f);
    [SerializeField] private bool showFieldBoundaries = true;
    [SerializeField] private Color fieldBoundaryColor = new Color(1f, 0.5f, 0f, 0.5f);
    
    [Header("Spawn Points")]
    [SerializeField] private int spawnPointsPerEdge = 5;
    [SerializeField] private bool showSpawnPoints = true;
    [SerializeField] private Color spawnPointColor = new Color(0f, 1f, 0f, 0.5f);
    [SerializeField] private float spawnPointSize = 0.3f;
    
    [Header("Path Calculation")]
    [SerializeField] private Transform blackHoleTransform; // Reference to the black hole
    [SerializeField] private float blackHoleAvoidanceRadius = 3f; // Radius to stay away from black hole
    [SerializeField] private float minCurveFactor = 0.2f; // Minimum curve factor for trajectories
    [SerializeField] private float maxCurveFactor = 0.8f; // Maximum curve factor for trajectories
    [SerializeField] private bool showPathPrediction = false; // Debug visualization of paths
    [SerializeField] private Color pathPredictionColor = new Color(1f, 1f, 0f, 0.3f);
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    // Cached list of spawn points for efficiency
    private List<Vector2> spawnPoints = new List<Vector2>();
    
    // Field boundaries
    private Bounds fieldBounds;

    private void Awake()
    {
        // Initialize field boundaries
        fieldBounds = new Bounds(transform.position, new Vector3(fieldSize.x, fieldSize.y, 1f));
        
        // Generate spawn points
        GenerateSpawnPoints();
        
        // Verify we have a black hole reference
        if (blackHoleTransform == null)
        {
            // Try to find the black hole by tag
            GameObject blackHole = GameObject.FindWithTag("BlackHole");
            if (blackHole != null)
            {
                blackHoleTransform = blackHole.transform;
            }
            else
            {
                Debug.LogWarning("Black hole transform reference not set and could not be found automatically.");
            }
        }
    }

    /// Creates spawn points along the edges of the field
    private void GenerateSpawnPoints()
    {
        spawnPoints.Clear();
        
        // Calculate half-sizes for easier positioning
        float halfWidth = fieldSize.x * 0.5f;
        float halfHeight = fieldSize.y * 0.5f;
        
        // Center of the field
        Vector2 center = new Vector2(transform.position.x, transform.position.y);
        
        // Calculate spacing between spawn points on each edge
        float horizontalSpacing = fieldSize.x / (spawnPointsPerEdge - 1);
        float verticalSpacing = fieldSize.y / (spawnPointsPerEdge - 1);
        
        // Top edge spawn points
        for (int i = 0; i < spawnPointsPerEdge; i++)
        {
            float x = center.x - halfWidth + (i * horizontalSpacing);
            float y = center.y + halfHeight;
            spawnPoints.Add(new Vector2(x, y));
        }
        
        // Right edge spawn points
        for (int i = 0; i < spawnPointsPerEdge; i++)
        {
            float x = center.x + halfWidth;
            float y = center.y + halfHeight - (i * verticalSpacing);
            spawnPoints.Add(new Vector2(x, y));
        }
        
        // Bottom edge spawn points
        for (int i = 0; i < spawnPointsPerEdge; i++)
        {
            float x = center.x + halfWidth - (i * horizontalSpacing);
            float y = center.y - halfHeight;
            spawnPoints.Add(new Vector2(x, y));
        }
        
        // Left edge spawn points
        for (int i = 0; i < spawnPointsPerEdge; i++)
        {
            float x = center.x - halfWidth;
            float y = center.y - halfHeight + (i * verticalSpacing);
            spawnPoints.Add(new Vector2(x, y));
        }
        
        if (debugMode)
        {
            Debug.Log($"Generated {spawnPoints.Count} spawn points around the debris field.");
        }
    }

    /// Checks if a position is within the field boundaries
    public bool IsWithinFieldBoundaries(Vector3 position)
    {
        return fieldBounds.Contains(position);
    }
    
    /// Gets a random spawn point from the available points
    public Vector2 GetRandomSpawnPoint()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("No spawn points available! Returning center of the field.");
            return transform.position;
        }
        
        return spawnPoints[Random.Range(0, spawnPoints.Count)];
    }
    
    /// Calculates a curved path direction from a spawn point to a random point on the opposite edge,
    /// with black hole avoidance
    public Vector2 CalculatePathDirection(Vector2 spawnPoint)
    {
        // Determine which edge the spawn point is on
        Vector2 center = new Vector2(transform.position.x, transform.position.y);
        float halfWidth = fieldSize.x * 0.5f;
        float halfHeight = fieldSize.y * 0.5f;
        
        // Calculate edge positions
        float top = center.y + halfHeight;
        float right = center.x + halfWidth;
        float bottom = center.y - halfHeight;
        float left = center.x - halfWidth;
        
        // Determine which edge to target (opposite of the spawn point's edge)
        Vector2 targetPoint;
        
        // If on top edge, target bottom edge
        if (Mathf.Approximately(spawnPoint.y, top))
        {
            targetPoint = new Vector2(
                center.x - halfWidth + Random.value * fieldSize.x,
                bottom
            );
        }
        // If on right edge, target left edge
        else if (Mathf.Approximately(spawnPoint.x, right))
        {
            targetPoint = new Vector2(
                left,
                center.y - halfHeight + Random.value * fieldSize.y
            );
        }
        // If on bottom edge, target top edge
        else if (Mathf.Approximately(spawnPoint.y, bottom))
        {
            targetPoint = new Vector2(
                center.x - halfWidth + Random.value * fieldSize.x,
                top
            );
        }
        // If on left edge, target right edge
        else
        {
            targetPoint = new Vector2(
                right,
                center.y - halfHeight + Random.value * fieldSize.y
            );
        }
        
        // Calculate base direction
        Vector2 baseDirection = targetPoint - spawnPoint;
        
        // Apply black hole avoidance
        Vector2 direction = ApplyBlackHoleAvoidance(spawnPoint, baseDirection);
        
        // Apply curved trajectory
        direction = ApplyCurvedTrajectory(direction);
        
        if (showPathPrediction && debugMode)
        {
            DrawPathPrediction(spawnPoint, direction);
        }
        
        // Return direction vector
        return direction;
    }
    

    /// Applies a curve factor to the trajectory path
    private Vector2 ApplyCurvedTrajectory(Vector2 direction)
    {
        // Generate a random curve factor
        float curveFactor = Random.Range(minCurveFactor, maxCurveFactor);
        
        // Determine if curve should be clockwise or counterclockwise
        bool clockwise = Random.value > 0.5f;
        
        // Create a perpendicular vector for the curve
        // For 2D, perpendicular is (-y, x) for counterclockwise and (y, -x) for clockwise
        Vector2 perpendicular = clockwise ? 
            new Vector2(direction.y, -direction.x).normalized : 
            new Vector2(-direction.y, direction.x).normalized;
        
        // Apply the curve factor
        Vector2 curvedDirection = direction.normalized + (perpendicular * curveFactor);
        
        // Return the normalized direction
        return curvedDirection.normalized;
    }
    
    /// Applies black hole avoidance to the path direction
    private Vector2 ApplyBlackHoleAvoidance(Vector2 startPoint, Vector2 baseDirection)
    {
        // If no black hole reference, return base direction
        if (blackHoleTransform == null)
        {
            return baseDirection;
        }
        
        // Get black hole position
        Vector2 blackHolePos = blackHoleTransform.position;
        
        // Check if the base path would come too close to the black hole
        // Direction vector must be normalized
        Vector2 dirNormalized = baseDirection.normalized;
        
        // Vector from start to black hole
        Vector2 startToBlackHole = blackHolePos - startPoint;
        
        // Project startToBlackHole onto dirNormalized
        float projection = Vector2.Dot(startToBlackHole, dirNormalized);
        
        // The closest point on the trajectory to the black hole
        Vector2 closestPoint;
        if (projection < 0)
        {
            // Black hole is behind the start point
            closestPoint = startPoint;
        }
        else if (projection > baseDirection.magnitude)
        {
            // Black hole is beyond the end point
            closestPoint = startPoint + baseDirection;
        }
        else
        {
            // Black hole is alongside the path
            closestPoint = startPoint + dirNormalized * projection;
        }
        
        // Distance from closest point to black hole
        float distance = Vector2.Distance(closestPoint, blackHolePos);
        
        // If distance is less than avoidance radius, apply avoidance
        if (distance < blackHoleAvoidanceRadius)
        {
            // Calculate avoidance vector perpendicular to the path
            Vector2 avoidanceDir = (closestPoint - blackHolePos).normalized;
            
            // Blend base direction with avoidance
            float blendFactor = 1.0f - (distance / blackHoleAvoidanceRadius);
            blendFactor = Mathf.Clamp01(blendFactor);
            
            // Higher weight toward the avoidance direction when closer to the black hole
            Vector2 adjustedDirection = Vector2.Lerp(
                baseDirection.normalized, 
                avoidanceDir, 
                blendFactor * 0.5f
            );
            
            return adjustedDirection.normalized * baseDirection.magnitude;
        }
        
        // No avoidance needed, return base direction
        return baseDirection;
    }
    
    /// Draws debug visualization of the predicted path
    private void DrawPathPrediction(Vector2 startPoint, Vector2 direction)
    {
        // Draw a line for the predicted path
        Debug.DrawRay(startPoint, direction.normalized * 10.0f, pathPredictionColor, 1.0f);
    }
    
    /// Draws the field boundaries and spawn points in the Scene view
    private void OnDrawGizmos()
    {
        // Draw field boundaries if enabled
        if (showFieldBoundaries)
        {
            Gizmos.color = fieldBoundaryColor;
            
            // Get field center and size
            Vector3 center = transform.position;
            Vector3 size = new Vector3(fieldSize.x, fieldSize.y, 0.1f);
            
            // Draw a wire cube for the field boundaries
            Gizmos.DrawWireCube(center, size);
        }
        
        // Draw spawn points if enabled
        if (showSpawnPoints && Application.isPlaying)
        {
            Gizmos.color = spawnPointColor;
            
            foreach (Vector2 spawnPoint in spawnPoints)
            {
                Gizmos.DrawSphere(spawnPoint, spawnPointSize);
            }
        }
        
        // Draw black hole avoidance radius if debug mode is on
        if (debugMode && blackHoleTransform != null)
        {
            Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.3f);
            Gizmos.DrawWireSphere(blackHoleTransform.position, blackHoleAvoidanceRadius);
        }
    }
}