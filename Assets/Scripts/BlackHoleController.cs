// BlackHoleController.cs

using UnityEngine;

public class BlackHoleController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object can be consumed
        if (other.CompareTag("Debris") || other.CompareTag("Consumable"))
        {
            // Destroy on contact
            Destroy(other.gameObject);
            
            // TODO: Add scoring, effects, or other game logic here as needed
        }
    }
}