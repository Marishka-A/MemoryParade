using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyAutoDialogueTrigger : MonoBehaviour
{
    [TextArea(2, 5)]
    [SerializeField] private List<string> lines = new List<string>();

    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private float lineDuration = 2.5f;

    private bool _triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;
        StartCoroutine(PlayDialogue());
    }

    private IEnumerator PlayDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        foreach (var line in lines)
        {
            if (dialogueText != null)
                dialogueText.text = line;

            yield return new WaitForSeconds(lineDuration);
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
}