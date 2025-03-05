// DebrisVisuals.cs (corrected version)

using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// Handles UI display for debris collection and capacity
public class DebrisVisuals : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text capacityText;
    [SerializeField] private Image capacityFillBar;
    [SerializeField] private GameObject collectingIndicator;
    
    [Header("UI Styling")]
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color fullColor = Color.red;
    [SerializeField] private float warningThreshold = 0.7f; // 70% full
    
    // Reference to the debris collector
    private DebrisCollector debrisCollector;
    
    private void Start()
    {
        // Find the debris collector in the scene
        debrisCollector = FindFirstObjectByType<DebrisCollector>();
        
        if (debrisCollector == null)
        {
            Debug.LogError("DebrisCollector component not found in the scene!");
            return;
        }
        
        // Directly assign UI references to the collector
        debrisCollector.capacityText = capacityText;
        debrisCollector.capacityFillBar = capacityFillBar;
        debrisCollector.collectingIndicator = collectingIndicator;
        
        // Set initial colors
        if (capacityFillBar != null)
        {
            capacityFillBar.color = normalColor;
        }
        
        // Hide collecting indicator initially
        if (collectingIndicator != null)
        {
            collectingIndicator.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Update UI every frame
        UpdateUI();
    }

    /// Updates the UI elements with current capacity information
    private void UpdateUI()
    {
        if (debrisCollector == null)
            return;
        
        // Get current values
        int currentCount = debrisCollector.CurrentDebrisCount;
        int maxCapacity = debrisCollector.MaxDebrisCapacity;
        bool isAtCapacity = debrisCollector.IsAtCapacity;
        
        // Update fill amount
        float fillAmount = (float)currentCount / maxCapacity;
        
        // Update capacity text
        if (capacityText != null)
        {
            capacityText.text = $"{currentCount} / {maxCapacity}";
        }
        
        // Update fill bar
        if (capacityFillBar != null)
        {
            // Set fill amount
            capacityFillBar.fillAmount = fillAmount;
            
            // Update fill color based on capacity
            if (isAtCapacity)
            {
                capacityFillBar.color = fullColor;
            }
            else if (fillAmount >= warningThreshold)
            {
                capacityFillBar.color = warningColor;
            }
            else
            {
                capacityFillBar.color = normalColor;
            }
        }
    }
}