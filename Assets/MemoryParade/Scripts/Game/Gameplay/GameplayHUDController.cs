using TMPro;
using UnityEngine;

public class GameplayHUDController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private TextMeshProUGUI shardsText;

    private PlayerСharacteristics stats;

    private void Start()
    {
        stats = PlayerСharacteristics.Instance;

        if (stats == null)
            stats = FindAnyObjectByType<PlayerСharacteristics>();
    }

    private void Update()
    {
        if (stats == null)
        {
            stats = PlayerСharacteristics.Instance;
            if (stats == null)
                stats = FindAnyObjectByType<PlayerСharacteristics>();
        }

        if (stats == null) return;

        if (hpText != null)
            hpText.text = stats.healthPoints.ToString();

        if (manaText != null)
            manaText.text = stats.mana.ToString();

        if (strengthText != null)
            strengthText.text = stats.strength.ToString();

        if (shardsText != null)
            shardsText.text = stats.numberOfWins.ToString();
    }
}