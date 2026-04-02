using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.MemoryParade.Scripts.Game.GameRoot;

public class DialogueIntroController : MonoBehaviour
{
    public enum Speaker
    {
        None,
        TreeGod,
        EyeGod,
        Hero
    }

    [System.Serializable]
    public class DialogueLine
    {
        public Speaker speaker;
        [TextArea(2, 6)] public string text;

        public Sprite background;
        public Sprite leftPortrait;
        public Sprite rightPortrait;

        public string speakerName;
        public bool leftActive;
        public bool rightActive;
    }

    [Header("UI")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image leftPortraitImage;
    [SerializeField] private Image rightPortraitImage;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Button nextButton;

    [Header("Visual")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 0.45f);

    [Header("Dialogue")]
    [SerializeField] private List<DialogueLine> lines = new List<DialogueLine>();

    private int _currentIndex = 0;

    private void Start()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(NextLine);

        if (lines.Count > 0)
            ShowLine(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            NextLine();
        }
    }

    public void NextLine()
    {
        _currentIndex++;

        if (_currentIndex >= lines.Count)
        {
            FinishDialogue();
            return;
        }

        ShowLine(_currentIndex);
    }

    private void ShowLine(int index)
    {
        var line = lines[index];

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (backgroundImage != null && line.background != null)
            backgroundImage.sprite = line.background;

        if (leftPortraitImage != null)
        {
            leftPortraitImage.sprite = line.leftPortrait;
            leftPortraitImage.enabled = line.leftPortrait != null;
            leftPortraitImage.color = line.leftActive ? activeColor : inactiveColor;
        }

        if (rightPortraitImage != null)
        {
            rightPortraitImage.sprite = line.rightPortrait;
            rightPortraitImage.enabled = line.rightPortrait != null;
            rightPortraitImage.color = line.rightActive ? activeColor : inactiveColor;
        }

        if (speakerNameText != null)
            speakerNameText.text = line.speakerName;

        if (dialogueText != null)
            dialogueText.text = line.text;
    }

    private void FinishDialogue()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.GoToScene(Scenes.LOBBY);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(Scenes.LOBBY);
    }
}