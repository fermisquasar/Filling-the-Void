// PlanetVisuals.cs

using UnityEngine;

/// Handles the visualization aspects of the planet, including orbit path and gravity zone visualization
public class PlanetVisuals : MonoBehaviour
{
    [Header("Editor Visualization")]
    [SerializeField] private bool showOrbitInEditor = true;
    [SerializeField] private bool showGravityZoneInEditor = true;
    
    [Header("In-Game Visualization")]
    [SerializeField] private bool showOrbitInGame = false;
    [SerializeField] private bool showGravityZoneInGame = false;
    
    [Header("Orbit Visualization")]
    [SerializeField] private Color orbitPathColor = Color.cyan;
    [SerializeField] private int orbitLineSegments = 60;
    
    [Header("Gravity Zone Visualization")]
    [SerializeField] private Color gravityZoneColor = new Color(0.5f, 0.5f, 1f, 0.2f);
    [SerializeField] private float gravityZoneLineWidth = 0.03f;
    
    // Reference to the core controller and components
    private PlanetController planetController;
    private PlanetOrbit orbitComponent;
    private PlanetGravity gravityComponent;
    
    // LineRenderer for in-game visualization
    private LineRenderer orbitLineRenderer;
    private LineRenderer gravityZoneLineRenderer;

    private void Awake()
    {
        // Get references to required components
        planetController = GetComponent<PlanetController>();
        orbitComponent = GetComponent<PlanetOrbit>();
        gravityComponent = GetComponent<PlanetGravity>();
        
        // Ensure we have the necessary components
        if (planetController == null)
        {
            Debug.LogError("PlanetController component is missing from planet GameObject!");
        }
        
        if (orbitComponent == null)
        {
            Debug.LogError("PlanetOrbit component is missing from planet GameObject!");
        }
        
        // Set up LineRenderers for in-game visualization
        InitializeLineRenderers();
    }
    
    /// Initializes the LineRenderer components for in-game visualizations
    private void InitializeLineRenderers()
    {
        // Initialize orbit line renderer
        orbitLineRenderer = gameObject.AddComponent<LineRenderer>();
        ConfigureOrbitLineRenderer();
        
        // Initialize gravity zone line renderer
        if (gravityComponent != null)
        {
            // Create a separate game object for the gravity zone renderer to avoid component conflicts
            GameObject gravityRendererObj = new GameObject("GravityZoneRenderer");
            gravityRendererObj.transform.parent = transform;
            gravityRendererObj.transform.localPosition = Vector3.zero;
            
            gravityZoneLineRenderer = gravityRendererObj.AddComponent<LineRenderer>();
            ConfigureGravityZoneLineRenderer();
        }
    }
    
    /// Configures the orbit LineRenderer
    private void ConfigureOrbitLineRenderer()
    {
        // Configure the orbit LineRenderer
        orbitLineRenderer.useWorldSpace = true;
        orbitLineRenderer.startWidth = 0.05f;
        orbitLineRenderer.endWidth = 0.05f;
        orbitLineRenderer.positionCount = orbitLineSegments + 1;
        orbitLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        orbitLineRenderer.startColor = orbitPathColor;
        orbitLineRenderer.endColor = orbitPathColor;
        
        // Hide it initially
        orbitLineRenderer.enabled = false;
    }
    
    /// Configures the gravity zone LineRenderer
    private void ConfigureGravityZoneLineRenderer()
    {
        // Configure the gravity zone LineRenderer
        gravityZoneLineRenderer.useWorldSpace = true;
        gravityZoneLineRenderer.startWidth = gravityZoneLineWidth;
        gravityZoneLineRenderer.endWidth = gravityZoneLineWidth;
        gravityZoneLineRenderer.positionCount = orbitLineSegments + 1;
        gravityZoneLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        gravityZoneLineRenderer.startColor = gravityZoneColor;
        gravityZoneLineRenderer.endColor = gravityZoneColor;
        
        // Hide it initially
        gravityZoneLineRenderer.enabled = false;
    }

    private void Update()
    {
        // Update visualizations
        UpdateOrbitLineVisibility();
        UpdateGravityZoneVisibility();
    }
    
    /// Updates the orbit line visibility and points
    private void UpdateOrbitLineVisibility()
    {
        if (orbitLineRenderer != null)
        {
            orbitLineRenderer.enabled = showOrbitInGame;
            
            // Update the orbit line points if visible
            if (showOrbitInGame)
            {
                UpdateOrbitLine();
            }
        }
    }
    
    /// Updates the gravity zone visibility and points
    private void UpdateGravityZoneVisibility()
    {
        if (gravityZoneLineRenderer != null && gravityComponent != null)
        {
            gravityZoneLineRenderer.enabled = showGravityZoneInGame;
            
            // Update the gravity zone line points if visible
            if (showGravityZoneInGame)
            {
                UpdateGravityZoneLine();
            }
        }
    }
    
    /// Updates the LineRenderer points to show the current orbit
    private void UpdateOrbitLine()
    {
        if (orbitLineRenderer == null || orbitComponent == null || planetController == null)
            return;
            
        // Update color in case it changed
        orbitLineRenderer.startColor = orbitPathColor;
        orbitLineRenderer.endColor = orbitPathColor;
        
        // Set the positions for the line renderer
        for (int i = 0; i <= orbitLineSegments; i++)
        {
            float angle = (i / (float)orbitLineSegments) * 2f * Mathf.PI;
            Vector3 position = orbitComponent.GetPositionOnOrbit(angle);
            orbitLineRenderer.SetPosition(i, position);
        }
    }
    
    /// Updates the LineRenderer points to show the gravity influence zone
    private void UpdateGravityZoneLine()
    {
        if (gravityZoneLineRenderer == null || gravityComponent == null)
            return;
            
        // Update color in case it changed
        gravityZoneLineRenderer.startColor = gravityZoneColor;
        gravityZoneLineRenderer.endColor = gravityZoneColor;
        
        float radius = gravityComponent.GetInfluenceRadius();
        
        // Set the positions for the line renderer to form a circle
        for (int i = 0; i <= orbitLineSegments; i++)
        {
            float angle = (i / (float)orbitLineSegments) * 2f * Mathf.PI;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            
            gravityZoneLineRenderer.SetPosition(i, transform.position + new Vector3(x, y, 0));
        }
    }

    /// Draws visualizations in the scene view
    private void OnDrawGizmos()
    {
        // Skip if we don't want to show visuals in editor
        if (!showOrbitInEditor && !showGravityZoneInEditor)
            return;
            
        // Draw orbit in editor mode if enabled
        if (showOrbitInEditor)
        {
            // Skip if we don't have required components in editor mode
            if (!Application.isPlaying)
            {
                var tempOrbit = GetComponent<PlanetOrbit>();
                var tempController = GetComponent<PlanetController>();
                
                if (tempOrbit == null || tempController == null || tempController.BlackHoleTransform == null)
                    return;
                    
                // Draw using available data in editor mode
                DrawEditorOrbitGizmos();
            }
            else if (planetController != null && orbitComponent != null && planetController.BlackHoleTransform != null)
            {
                DrawOrbitPath();
            }
        }
        
        // Draw gravity zone in editor mode if enabled
        if (showGravityZoneInEditor)
        {
            if (!Application.isPlaying)
            {
                var tempGravity = GetComponent<PlanetGravity>();
                if (tempGravity != null)
                {
                    DrawEditorGravityZoneGizmos(tempGravity.GetInfluenceRadius());
                }
            }
            else if (gravityComponent != null)
            {
                DrawGravityZone();
            }
        }
    }
    
    /// Draws orbit path gizmos in editor mode (before play)
    private void DrawEditorOrbitGizmos()
    {
        var tempOrbit = GetComponent<PlanetOrbit>();
        var tempController = GetComponent<PlanetController>();
        
        Gizmos.color = orbitPathColor;
        
        const int segments = 60;
        
        // Calculate first point
        float semiMajorAxis = tempOrbit.GetSemiMajorAxis();
        float semiMinorAxis = tempOrbit.GetSemiMinorAxis();
        Vector3 blackHolePos = tempController.BlackHoleTransform.position;
        
        Vector3 previousPosition = blackHolePos + new Vector3(semiMajorAxis, 0f, 0f);
        
        // Draw the ellipse
        for (int i = 1; i <= segments; i++)
        {
            float angle = (i / (float)segments) * 2f * Mathf.PI;
            float x = Mathf.Cos(angle) * semiMajorAxis;
            float y = Mathf.Sin(angle) * semiMinorAxis;
            
            Vector3 currentPosition = blackHolePos + new Vector3(x, y, 0f);
            
            Gizmos.DrawLine(previousPosition, currentPosition);
            previousPosition = currentPosition;
        }
    }
    
    /// Draws the gravity zone gizmos in editor mode (before play)
    private void DrawEditorGravityZoneGizmos(float radius)
    {
        // Set the gizmo color and transparency
        Color tempColor = gravityZoneColor;
        Gizmos.color = tempColor;
        
        // Draw the circle as a series of line segments
        const int segments = 60;
        Vector3 previousPosition = transform.position + new Vector3(radius, 0f, 0f);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = (i / (float)segments) * 2f * Mathf.PI;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            
            Vector3 currentPosition = transform.position + new Vector3(x, y, 0f);
            
            Gizmos.DrawLine(previousPosition, currentPosition);
            previousPosition = currentPosition;
        }
    }
    
    /// Draws the elliptical orbit path
    private void DrawOrbitPath()
    {
        // Set the gizmo color
        Gizmos.color = orbitPathColor;
        
        // Number of segments to draw the ellipse (more segments = smoother curve)
        const int segments = 60;
        
        // Draw the orbit path as a series of line segments
        Vector3 previousPosition = orbitComponent.GetPositionOnOrbit(0);
        for (int i = 1; i <= segments; i++)
        {
            float angle = (i / (float)segments) * 2f * Mathf.PI;
            Vector3 currentPosition = orbitComponent.GetPositionOnOrbit(angle);
            
            Gizmos.DrawLine(previousPosition, currentPosition);
            previousPosition = currentPosition;
        }
    }
    
    /// Draws the gravity influence zone
    private void DrawGravityZone()
    {
        // Set the gizmo color
        Gizmos.color = gravityZoneColor;
        
        float radius = gravityComponent.GetInfluenceRadius();
        
        // Draw the influence zone as a series of line segments
        const int segments = 60;
        Vector3 previousPosition = transform.position + new Vector3(radius, 0f, 0f);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = (i / (float)segments) * 2f * Mathf.PI;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            
            Vector3 currentPosition = transform.position + new Vector3(x, y, 0f);
            
            Gizmos.DrawLine(previousPosition, currentPosition);
            previousPosition = currentPosition;
        }
    }
}