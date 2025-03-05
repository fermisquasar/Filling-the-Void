// PlanetControl.cs

using UnityEngine;

/// Handles player input and controls for the planet
public class PlanetControls : MonoBehaviour
{
    [Header("Speed Controls")]
    [SerializeField] private float minSpeed = 0.5f;
    [SerializeField] private float maxSpeed = 3.0f;
    [SerializeField] private float accelerationRate = 0.5f;
    [SerializeField] private float currentSpeed = 1.0f;

    [Header("Direction Controls")]
    [SerializeField] private bool isClockwise = true;

    [Header("Anchor Controls")]
    [SerializeField] private bool isAnchored = false;

    // References to other components
    private PlanetOrbit orbitComponent;

    // Store previous state for resuming after anchor
    private float previousSpeed;
    private bool previousDirection;

    private void Awake()
    {
        // Get reference to orbit component
        orbitComponent = GetComponent<PlanetOrbit>();

        // Error check
        if (orbitComponent == null)
        {
            Debug.LogError("PlanetOrbit component is missing from planet GameObject!");
        }
    }

    private void Start()
    {
        // Initialize speed in orbit component
        UpdateOrbitSpeed();
        UpdateOrbitDirection();
    }

    private void Update()
    {
        // Process user input
        HandleSpeedInput();
        HandleDirectionInput();
        HandleAnchorInput();
    }

    /// Handles speed controls (Up/Down arrow keys)
    private void HandleSpeedInput()
    {
        // Skip if anchored
        if (isAnchored)
            return;

        // Increase speed with Up arrow
        if (Input.GetKey(KeyCode.UpArrow))
        {
            currentSpeed += accelerationRate * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);
            UpdateOrbitSpeed();
        }
        
        // Decrease speed with Down arrow
        if (Input.GetKey(KeyCode.DownArrow))
        {
            currentSpeed -= accelerationRate * Time.deltaTime;
            currentSpeed = Mathf.Max(currentSpeed, minSpeed);
            UpdateOrbitSpeed();
        }
    }

    /// Handles direction reversal (Left arrow key)
    private void HandleDirectionInput()
    {
        // Skip if anchored
        if (isAnchored)
            return;

        // Change direction with Left arrow - now immediate
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeDirectionImmediate();
        }
    }
    
    /// Handles anchoring the planet (Right arrow key)
    private void HandleAnchorInput()
    {
        // Toggle anchor with Right arrow
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ToggleAnchor();
        }
    }
    
    /// Toggles the anchored state
    private void ToggleAnchor()
    {
        if (!isAnchored)
        {
            // Store current state before anchoring
            previousSpeed = currentSpeed;
            previousDirection = isClockwise;
            
            // Anchor the planet
            isAnchored = true;
            orbitComponent.SetOrbitSpeed(0f);
        }
        else
        {
            // Release anchor and restore previous state
            isAnchored = false;
            currentSpeed = previousSpeed;
            isClockwise = previousDirection;
            
            // Update orbit component with restored values
            UpdateOrbitSpeed();
            UpdateOrbitDirection();
        }
    }
    
    /// Changes direction immediately without any transition
    private void ChangeDirectionImmediate()
    {
        // Immediately change direction
        isClockwise = !isClockwise;
        
        // Update the orbit component
        UpdateOrbitDirection();
    }
    
    /// Updates the orbit component with current speed
    private void UpdateOrbitSpeed()
    {
        if (orbitComponent != null)
        {
            orbitComponent.SetOrbitSpeed(currentSpeed);
        }
    }
    
    /// Updates the orbit component with current direction
    private void UpdateOrbitDirection()
    {
        if (orbitComponent != null)
        {
            orbitComponent.SetOrbitDirection(isClockwise);
        }
    }
}