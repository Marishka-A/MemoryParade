using UnityEngine;

public class ManaPickup : MonoBehaviour
{
    [SerializeField] private int manaRestoreValue = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var stats = other.GetComponent<PlayerСharacteristics>();
        if (stats == null)
            stats = PlayerСharacteristics.Instance;

        if (stats != null)
        {
            stats.RestoreMana(manaRestoreValue);
            stats.ShowNotification($"+{manaRestoreValue} маны");
        }

        Destroy(gameObject);
    }
}