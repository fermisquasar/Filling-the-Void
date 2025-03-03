using UnityEngine;

public class PlanetOrbit: MonoBehaviour
{
    public Transform barycenter; // The center of the orbit (void position)
    public float semiMajorAxis = 5f; // The distance of the horizontal axis
    public float semiMinorAxis = 3f; // The distance of the verical axis
    public float orbitalSpeed = 1f; // The speed of the orbit
    private float angle = 0f; // The angle that the object is moving

    void Update()
    {
        // Update the angle over time
        angle += orbitalSpeed * Time.deltaTime;

        // Keep the angle within 0 - 2π range to ensure a full loop
        if (angle > Mathf.PI * 2)
        {
            angle -= Mathf.PI * 2;
        }

        //Calculate new positions
        float x = semiMajorAxis * Mathf.Cos(angle);
        float y = semiMinorAxis * Mathf.Sin(angle);

        // Update planet's position relative to barycenter
        transform.position = barycenter.position + new Vector3(x, y, 0);
    }
}
