using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BarrierWinTrigger : MonoBehaviour
{
    [SerializeField] private int requiredShards = 12;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float winDelay = 2.5f;

    [Header("UI")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winText;

    private bool winStarted = false;

    private void Start()
    {
        if (winPanel != null)
            winPanel.SetActive(false);
    }

    // 👉 ЭТО БУДЕТ ВЫЗЫВАТЬ КНОПКА "ПОПОЛНИТЬ"
    public void DepositShards()
    {
        if (winStarted)
            return;

        if (PlayerСharacteristics.Instance == null)
            return;

        int shards = PlayerСharacteristics.Instance.numberOfWins;

        if (shards < requiredShards)
        {
            Debug.Log("Недостаточно осколков");
            return;
        }

        StartCoroutine(WinRoutine());
    }

    private IEnumerator WinRoutine()
    {
        winStarted = true;

        if (winPanel != null)
            winPanel.SetActive(true);

        if (winText != null)
            winText.text = "Победа!";

        // Останавливаем игрока
        CharacterMove move = FindAnyObjectByType<CharacterMove>();
        if (move != null)
            move.enabled = false;

        yield return new WaitForSeconds(winDelay);

        SceneManager.LoadScene(mainMenuSceneName);
    }
}