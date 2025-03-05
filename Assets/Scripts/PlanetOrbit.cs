// PlanetOrbit.cs

using UnityEngine;

/// Handles the orbital mechanics of the planet
public class PlanetOrbit : MonoBehaviour
{
    [Header("Orbital Properties")]
    [SerializeField] private float semiMajorAxis = 5f;
    [SerializeField] private float semiMinorAxis = 3f;
    [SerializeField] private float orbitSpeed = 1f;
    [SerializeField] private bool clockwiseRotation = true;

    // Internally tracked angle for orbit calculation
    private float currentAngle = 0f;
    
    // Reference to the core controller
    private PlanetController planetController;

    private void Awake()
    {
        // Get reference to the planet controller
        planetController = GetComponent<PlanetController>();
        
        // Ensure we have the core controller
        if (planetController == null)
        {
            Debug.LogError("PlanetController component is missing from planet GameObject!");
        }
    }

    private void Update()
    {
        // Update the orbit
        UpdateOrbit();
    }

    /// Initializes the planet's position on its orbit
    public void InitializePosition()
    {
        UpdateOrbitPosition();
    }

    /// Updates the orbital position of the planet
    private void UpdateOrbit()
    {
        // Determine direction modifier
        float directionModifier = clockwiseRotation ? -1f : 1f;
       
        // Update the current angle based on speed and time
        currentAngle += directionModifier * orbitSpeed * Time.deltaTime;
       
        // Keep angle between 0 and 2π
        if (currentAngle >= 2f * Mathf.PI)
        {
            currentAngle -= 2f * Mathf.PI;
        }
        else if (currentAngle < 0f)
        {
            currentAngle += 2f * Mathf.PI;
        }
       
        // Update position based on the new angle
        UpdateOrbitPosition();
       
        // Rotate planet to match orbital direction
        UpdatePlanetRotation(directionModifier);
    }

    /// Sets the planet's position on its elliptical orbital path
    private void UpdateOrbitPosition()
    {
        // Calculate position using parametric equation of an ellipse
        float x = Mathf.Cos(currentAngle) * semiMajorAxis;
        float y = Mathf.Sin(currentAngle) * semiMinorAxis;
       
        // Set position relative to the black hole
        Vector3 newPosition = planetController.BlackHoleTransform.position + new Vector3(x, y, 0f);
        planetController.SetPosition(newPosition);
    }

    /// Rotates the planet to match its orbital movement direction
    private void UpdatePlanetRotation(float directionModifier)
    {
        // Calculate the angle in degrees for Unity's rotation
        float rotationAngle = (currentAngle * Mathf.Rad2Deg) - 90f;
       
        // Adjust rotation based on direction
        Quaternion newRotation = Quaternion.Euler(0f, 0f, rotationAngle);
        planetController.SetRotation(newRotation);
    }

    /// Gets a position on the orbit at the specified angle
    public Vector3 GetPositionOnOrbit(float angle)
    {
        float x = Mathf.Cos(angle) * semiMajorAxis;
        float y = Mathf.Sin(angle) * semiMinorAxis;
        
        return planetController.BlackHoleTransform.position + new Vector3(x, y, 0f);
    }

    /// Gets the current angle of the planet in its orbit
    public float GetCurrentAngle()
    {
        return currentAngle;
    }

    /// Gets the semi-major axis of the orbit
    public float GetSemiMajorAxis()
    {
        return semiMajorAxis;
    }

    /// Gets the semi-minor axis of the orbit
    public float GetSemiMinorAxis()
    {
        return semiMinorAxis;
    }

    /// Sets the orbit speed
    public void SetOrbitSpeed(float speed)
    {
        orbitSpeed = speed;
    }

    /// Gets the current orbit speed
    public float GetOrbitSpeed()
    {
        return orbitSpeed;
    }

    /// Sets the orbit direction
    public void SetOrbitDirection(bool isClockwise)
    {
        clockwiseRotation = isClockwise;
    }

    /// Gets the current orbit direction
    public bool GetOrbitDirection()
    {
        return clockwiseRotation;
    }
}