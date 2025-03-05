// DebrisField.cs

using UnityEngine;

/// Defines and visualizes the debris field boundaries in-game.
public class DebrisField : MonoBehaviour
{
    [Header("Field Visualization")]
    [SerializeField] private bool showFieldInGame = true;
    [SerializeField] private Color fieldLineColor = new Color(1f, 0.5f, 0f, 0.3f);
    [SerializeField] private float lineWidth = 0.1f;
    
    // Reference to the debris manager
    private DebrisManager debrisManager;
    
    // Line renderer for visualizing the field
    private LineRenderer fieldLineRenderer;
    
    private void Awake()
    {
        // Get reference to the debris manager (should be on the same GameObject)
        debrisManager = GetComponent<DebrisManager>();
        if (debrisManager == null)
        {
            //Debug.LogError("DebrisManager component not found on same GameObject!");
        }
        
        // Setup line renderer for field visualization
        InitializeLineRenderer();
    }
    
    private void Start()
    {
        // Update the visualization
        UpdateFieldVisualization();
    }
    
    /// Initializes the line renderer component for field visualization
    private void InitializeLineRenderer()
    {
        // Create the line renderer component
        fieldLineRenderer = gameObject.AddComponent<LineRenderer>();
        
        // Configure the line renderer
        fieldLineRenderer.startWidth = lineWidth;
        fieldLineRenderer.endWidth = lineWidth;
        fieldLineRenderer.positionCount = 5; // 5 points to form a rectangle (with the last point same as first)
        fieldLineRenderer.useWorldSpace = true;
        fieldLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        fieldLineRenderer.startColor = fieldLineColor;
        fieldLineRenderer.endColor = fieldLineColor;
        
        // Hide it initially
        fieldLineRenderer.enabled = false;
    }
    
    /// Updates the field visualization based on settings
    private void UpdateFieldVisualization()
    {
        if (fieldLineRenderer != null)
        {
            fieldLineRenderer.enabled = showFieldInGame;
            
            if (showFieldInGame)
            {
                // Get the field center and size from the parent manager
                Vector3 center = transform.position;
                Vector2 fieldSize = GetFieldSize();
                
                float halfWidth = fieldSize.x / 2f;
                float halfHeight = fieldSize.y / 2f;
                
                // Set the positions for the line renderer (clockwise rectangle)
                fieldLineRenderer.SetPosition(0, center + new Vector3(-halfWidth, halfHeight, 0)); // Top left
                fieldLineRenderer.SetPosition(1, center + new Vector3(halfWidth, halfHeight, 0));  // Top right
                fieldLineRenderer.SetPosition(2, center + new Vector3(halfWidth, -halfHeight, 0)); // Bottom right
                fieldLineRenderer.SetPosition(3, center + new Vector3(-halfWidth, -halfHeight, 0)); // Bottom left
                fieldLineRenderer.SetPosition(4, center + new Vector3(-halfWidth, halfHeight, 0)); // Back to top left
                
                // Update colors
                fieldLineRenderer.startColor = fieldLineColor;
                fieldLineRenderer.endColor = fieldLineColor;
            }
        }
    }
    
    /// Gets the field size from the debris manager
    private Vector2 GetFieldSize()
    {
        // Default size in case we can't access the actual size
        Vector2 defaultSize = new Vector2(20f, 20f);
        
        // Try to get the field size through reflection (since it's private in DebrisManager)
        if (debrisManager != null)
        {
            System.Reflection.FieldInfo fieldSizeInfo = typeof(DebrisManager).GetField("fieldSize", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (fieldSizeInfo != null)
            {
                Vector2 fieldSize = (Vector2)fieldSizeInfo.GetValue(debrisManager);
                return fieldSize;
            }
        }
        
        //Debug.LogWarning("Couldn't access fieldSize from DebrisManager, using default size.");
        return defaultSize;
    }
    
    /// Updates the field visualization when values change
    public void OnValidate()
    {
        if (Application.isPlaying && fieldLineRenderer != null)
        {
            UpdateFieldVisualization();
        }
    }
}