// planetOrbit.cs

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PlanetOrbit: MonoBehaviour
{
    // Orbit properties
    public Transform barycenter; // The center of the orbit (void position)
    public float semiMajorAxis = 5f; // Horizaontal axis length
    public float semiMinorAxis = 3f; // Vertical axis length
    public float speed = 0.5f; // Absolute speed, always positive
    public int direction = 1; // 1 for left, -1 for right
    private float angle = 0f; // Angle of the planet's movement

    private bool isAnchored = false; // Tracks if planet is anchored

    // Cooldown management
    private Dictionary<string, float> cooldowns = new Dictionary<string, float>(); // Dictionary to track active cooldowns
    private bool isCooldownActive = false; // Prevents multiple coroutines instances from handling cooldowns

    //Cooldown durations
    public float speedCooldownTime = 1.5f; // Changing speed
    public float directionCooldownTime = 5; // Changing direction
    public float anchorTime = 3f; // Anchor time
    public float anchorCooldownTime = 5f; // Anchoring planet

    //  UI elements
    public TMP_Text speedMeterText; // Planet speed
    public TMP_Text speedCooldownText; // Speed cooldown
    public TMP_Text directionMeter; // Planet direction
    public TMP_Text directionCooldownText; // Direction cooldown

    public TMP_Text anchorTimeText; // Anchor time
    public TMP_Text anchorCooldownText; // Anchor cooldown

    void Start()
    {
        // Initialize UI elements
        UpdateSpeedMeter(); // Planet speed
        UpdateDirectionMeter(); // Planet direction
        speedCooldownText.text = ""; // Speed cooldown
        anchorTimeText.text = ""; // Anchor time
        anchorCooldownText.text = ""; // Anchor cooldown
        directionCooldownText.text = ""; // Dirciton cooldown
    }

    void Update()
    {
        if(isAnchored) return; // prevent movement if anchored

        angle += speed * direction * Time.deltaTime; // Update planet's position along orbit

        // Keep the angle within 0 - 2π range to ensure a full loop
        if (angle > Mathf.PI * 2)
        {
            angle -= Mathf.PI * 2;
        }

        // Calculate new positions  based on ellipse equation
        float x = semiMajorAxis * Mathf.Cos(angle);
        float y = semiMinorAxis * Mathf.Sin(angle);

        transform.position = barycenter.position + new Vector3(x, y, 0); // Update planet's position relative to barycenter
    }

    // Change planet spped
    public void ChangeSpeed(float change)
    {
        if (cooldowns.ContainsKey("speed")) return; // Check if cooldown is active

        speed = Mathf.Clamp(speed + change, 0.25f, 0.75f); // Updates speed, min: 0.25 max: 0.75
        UpdateSpeedMeter(); // Update speed UI
        StartCooldown("speed", speedCooldownTime, speedCooldownText); // Start cooldown
    }

    // Speed meter update
    void UpdateSpeedMeter()
    {
        int plusCount = Mathf.RoundToInt(speed * 4); // 1 arrow per 0.25 speed
        speedMeterText.text = new string('+', plusCount);
    }

    // Function to change planet direction
    public void ChangeDirection()
    {
        if(cooldowns.ContainsKey("direction")) return; // Check if cooldown is active

        direction *= -1; // Flip direction
        UpdateDirectionMeter(); // Update direction UI
        StartCooldown("direction", directionCooldownTime, directionCooldownText); // Start cooldown
    }

    // Update direction meter UI
    void UpdateDirectionMeter()
    {
        if (direction  == 1)
        {
            directionMeter.text = "<--";
        }
        else if (direction == -1)
        {
            directionMeter.text = "-->";
        }
        else 
        {
            directionMeter.text = "===";
        }
    }

    // Anchor the planet in place
    public void anchorPlanet()
    {
        if (cooldowns.ContainsKey("anchor")) return; // Check if cooldown is active

        isAnchored = true; // Prevent movement
        StartCoroutine(AnchorProcess()); // Start anchor process
    }

    // Handle anchor time and cooldown
    IEnumerator AnchorProcess()
    {
        StartCooldown("anchor", anchorTime, anchorTimeText); // Start anchortimer
        yield return new WaitForSeconds(anchorTime);
        isAnchored = false; // Allow movement again
        StartCooldown("anchorCooldown", anchorCooldownTime, anchorCooldownText); // Start cooldown after anchoring
    }

    // Start a cooldown for an action
    void StartCooldown(string key, float duration, TMP_Text uiText)
    {
        cooldowns[key] = duration;
        if(!isCooldownActive) StartCoroutine(HandleCooldowns()); // Start cooldown coroutine if not running
    }

    // General coroutin to handle all active cooldowns
    IEnumerator HandleCooldowns()
    {
        isCooldownActive = true; // Set the cooldown process to active
        // Loop as long as there are active cooldowns
        while (cooldowns.Count > 0)
        {
            List<string> keys = new List<string>(cooldowns.Keys);
            foreach (var key in keys)
            {
                cooldowns[key] -= 0.1f; // Reduce coolown time
                // Remove a cooldown if time is expired
                if (cooldowns[key] <= 0)
                {
                    cooldowns.Remove(key);
                    UpdateCooldownUI(key, ""); // Clear cooldown UI
                }
                else
                {
                    UpdateCooldownUI(key, $"Cooldown: {cooldowns[key]:F1}s"); // Update Ui with remaining time
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
        isCooldownActive = false; // No more active cooldowns
    }

    // Update cooldown UI elements based on cooldown type
    void UpdateCooldownUI(string key, string text)
    {
        if (key == "speed") speedCooldownText.text = text;
        else if (key == "direction") directionCooldownText.text = text;
        else if (key == "anchor") anchorTimeText.text = text;
        else if (key == "anchorCooldown") anchorCooldownText.text = text;
    }
}
