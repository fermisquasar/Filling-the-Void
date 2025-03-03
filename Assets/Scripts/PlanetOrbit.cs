using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PlanetOrbit: MonoBehaviour
{
    public Transform barycenter; // The center of the orbit (void position)
    public float semiMajorAxis = 5f; // The distance of the horizontal axis
    public float semiMinorAxis = 3f; // The distance of the verical axis
    public float orbitalSpeed = 0.5f; // The speed of the orbit
    private float angle = 0f; // The angle that the object is moving

    private bool canChangeSpeed = true; // Speed cooldown status
    public float speedCooldownTime = 1.5f; // 1.5 second cooldown for changing speed

    private bool canAnchorPlanet = true; // Anchor cooldown status
    public float anchorCooldownTime = 5f; // 5 second cooldown for anchoring planet

    public TMP_Text speedMeterText; // UI elemnt to display speed meter
    public TMP_Text speedCooldownText; // UI element for speed cooldown timer
    public TMP_Text anchorCooldownText; // UI element for anchor cooldown timer

    void Start()
    {
        UpdateSpeedMeter(); // Set the speed meter correctly when the game starts
        speedCooldownText.text = ""; // Make sure the speed cooldown text is blank when the game starts
    }

    void Update()
    {
        angle += orbitalSpeed * Time.deltaTime; // Update the angle over time

        // Keep the angle within 0 - 2π range to ensure a full loop
        if (angle > Mathf.PI * 2)
        {
            angle -= Mathf.PI * 2;
        }

        //Calculate new positions
        float x = semiMajorAxis * Mathf.Cos(angle);
        float y = semiMinorAxis * Mathf.Sin(angle);

        transform.position = barycenter.position + new Vector3(x, y, 0); // Update planet's position relative to barycenter
    }

    // Method to change speed
    public void ChangeSpeed(float change)
    {
        // Check if speed cooldown is active
        if(!canChangeSpeed) 
        {
            return;
        }

        orbitalSpeed += change; // Update speed by change

        // Guarantee the planet does not stop. Checks if speed would be 0, if so change direction of planet in direction of change
        if (orbitalSpeed == 0)
        {
            if (change > 0)
            {   
                orbitalSpeed = 0.5f;
            } else {
                orbitalSpeed = -0.5f;
            }
        }

        orbitalSpeed = Mathf.Clamp(orbitalSpeed, -1.5f, 1.5f); // Cap speed between -2 and 2

        UpdateSpeedMeter(); //Update UI meter
        StartCoroutine(SpeedCooldown()); // Start speed cooldown
    }

    // UI Meter Update
    void UpdateSpeedMeter()
    {
        int arrowCount = Mathf.RoundToInt(Mathf.Abs(orbitalSpeed * 2)); // 1 arrow per 0.5 speed
        string direction = orbitalSpeed > 0 ? "→" : "←";
        speedMeterText.text = new string(direction[0], arrowCount); // Update the text in the UI
    }

    // Cooldown Coroutine
    IEnumerator SpeedCooldown()
    {
        canChangeSpeed = false;
        float remainingTime = speedCooldownTime;

        // Display the remainingTime while there is still an speed cooldown
        while (remainingTime > 0)
        {
            speedCooldownText.text = $"Cooldown: {remainingTime:F1}s";
            yield return new WaitForSeconds(0.1f);
            remainingTime -= 0.1f;
        }

        speedCooldownText.text = ""; // Hide text after speed cooldown
        canChangeSpeed = true; // Allows the player to change the planet's speed again
    }



}
