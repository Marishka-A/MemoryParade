using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BarrierInteraction : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject barrierPanel;
    [SerializeField] private TMP_Text barrierText;
    [SerializeField] private Button depositButton;

    private bool _playerInside = false;

    private void Start()
    {
        if (barrierPanel != null)
            barrierPanel.SetActive(false);

        if (depositButton != null)
            depositButton.onClick.AddListener(OnDepositClicked);

        RefreshUI();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Barrier trigger enter: " + other.name);

        if (!other.CompareTag("Player")) return;

        _playerInside = true;

        if (barrierPanel != null)
            barrierPanel.SetActive(true);

        RefreshUI();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("Barrier trigger exit: " + other.name);

        if (!other.CompareTag("Player")) return;

        _playerInside = false;

        if (barrierPanel != null)
            barrierPanel.SetActive(false);
    }

    private void OnDepositClicked()
    {
        if (!_playerInside) return;
        if (ShardProgress.PlayerShards <= 0) return;

        ShardProgress.DepositAll();
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (barrierText != null)
            barrierText.text = $"Собрано {ShardProgress.BarrierShards} из {ShardProgress.TargetShards}";

        if (depositButton != null)
        {
            depositButton.interactable = ShardProgress.PlayerShards > 0;

            ColorBlock colors = depositButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.85f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0.7f, 0.95f, 0.95f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = Color.white;
            depositButton.colors = colors;
        }
    }
}