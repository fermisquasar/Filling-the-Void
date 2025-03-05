// PlaneController.cs

using UnityEngine;

/// Core controller for the planet system. Acts as a central coordinator for all planet components.
public class PlanetController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private Transform blackHoleTransform;

    // Properties accessible to other components
    public Transform BlackHoleTransform => blackHoleTransform;

    // Component references
    private PlanetOrbit orbitComponent;
    private PlanetVisuals visualsComponent;

    private void Awake()
    {
        // Get references to attached components
        orbitComponent = GetComponent<PlanetOrbit>();
        visualsComponent = GetComponent<PlanetVisuals>();

        // Ensure we have the required components
        if (orbitComponent == null)
        {
            Debug.LogError("PlanetOrbit component is missing from planet GameObject!");
        }
    }

    private void Start()
    {
        // Validate that we have reference to the black hole
        if (blackHoleTransform == null)
        {
            Debug.LogError("Black hole transform reference not set in PlanetController!");
        }

        // Initialize position
        orbitComponent.InitializePosition();
    }

    private void Update()
    {
        // Core update logic can go here if needed
    }

    /// Gets the current position of the planet
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    /// Sets the position of the planet
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    /// Sets the rotation of the planet
    public void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }
}