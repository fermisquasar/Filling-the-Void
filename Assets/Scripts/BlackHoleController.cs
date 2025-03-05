// BlackHoleController.cs (updated)

using UnityEngine;
using TMPro;

public class BlackHoleController : MonoBehaviour
{
    [Header("Scoring")]
    [SerializeField] private int standardDebrisScore = 1;
    [SerializeField] private int heavyDebrisScore = 3;
    [SerializeField] private int stickyDebrisScore = 1;
    [SerializeField] private int fragileDebrisScore = 5;
    [SerializeField] public TMP_Text scoreText;

    private int currentScore = 0;

    private void Start()
    {
        // Initialize the score display
        UpdateScoreDisplay();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object can be consumed
        if (other.CompareTag("Debris") || other.CompareTag("Consumable"))
        {
            // Add score based on debris type
            DebrisBase debris = other.GetComponent<DebrisBase>();
            if (debris != null)
            {
                AddScoreForDebris(debris);
            }
            
            // Destroy on contact
            Destroy(other.gameObject);
        }
    }

    private void AddScoreForDebris(DebrisBase debris)
    {
        // Determine score based on debris type
        if (debris is StandardDebris)
        {
            currentScore += standardDebrisScore;
        }
        else if (debris is HeavyDebris)
        {
            currentScore += heavyDebrisScore;
        }
        else if (debris is StickyDebris)
        {
            currentScore += stickyDebrisScore;
        }
        else if (debris is FragileDebris)
        {
            currentScore += fragileDebrisScore;
        }

        // Update the score display
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }

    // Public method to get current score (for other components)
    public int GetCurrentScore()
    {
        return currentScore;
    }
}