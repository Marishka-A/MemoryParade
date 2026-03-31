using UnityEngine;

public class ManaPickup : MonoBehaviour
{
    [SerializeField] private int manaRestore = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var stats = other.GetComponent<PlayerСharacteristics>();
        if (stats != null)
        {
            stats.RestoreMana(manaRestore);
        }

        Destroy(gameObject);
    }
}