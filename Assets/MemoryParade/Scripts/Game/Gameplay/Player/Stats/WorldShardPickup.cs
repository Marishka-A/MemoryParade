using UnityEngine;

public class WorldShardPickup : MonoBehaviour
{
    [SerializeField] private int shardValue = 1;
    [SerializeField] private float lifeTime = 20f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        ShardProgress.AddPlayerShards(shardValue);
        Destroy(gameObject);
    }
}