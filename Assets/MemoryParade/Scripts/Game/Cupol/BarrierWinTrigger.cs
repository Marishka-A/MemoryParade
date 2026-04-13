using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BarrierWinTrigger : MonoBehaviour
{
    [SerializeField] private int requiredShards = 12;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private float winDelay = 2.5f;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winText;

    private bool playerInside = false;
    private bool winStarted = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
    }

    private void Update()
    {
        if (!playerInside || winStarted)
            return;

        if (!Input.GetKeyDown(KeyCode.E))
            return;

        if (ShardProgress.PlayerShards < requiredShards)
        {
            Debug.Log("Недостаточно осколков");
            return;
        }

        StartCoroutine(WinRoutine());
    }

    private IEnumerator WinRoutine()
    {
        winStarted = true;

        // Если нужно, можно обнулить осколки после возврата в барьер
        ShardProgress.PlayerShards = 0;

        if (winPanel != null)
            winPanel.SetActive(true);

        if (winText != null)
            winText.text = "Поздравляю, вы выиграли!";

        yield return new WaitForSeconds(winDelay);

        SceneManager.LoadScene(mainMenuSceneName);
    }
}