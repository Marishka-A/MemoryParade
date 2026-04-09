using System.Collections;
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
        public string speakerName;

        [TextArea(2, 6)]
        public string text;

        public Sprite background;
        public Sprite leftPortrait;
        public Sprite rightPortrait;

        public bool leftActive;
        public bool rightActive;

        [Header("Effects")]
        public bool useFade;
        public float fadeDuration = 0.35f;

        public bool shakeCamera;
        public float shakeDuration = 0.2f;
        public float shakeStrength = 8f;

        public float delayBeforeShow = 0f;
    }

    [Header("UI")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image leftPortraitImage;
    [SerializeField] private Image rightPortraitImage;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Button nextButton;
    [SerializeField] private Image fadeOverlay;
    [SerializeField] private RectTransform contentRoot;

    [Header("Visual")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 0.45f);

    [Header("Dialogue")]
    [SerializeField] private List<DialogueLine> lines = new List<DialogueLine>();

    private int _currentIndex = 0;
    private bool _isBusy = false;

    private void Start()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextPressed);

        if (fadeOverlay != null)
        {
            var c = fadeOverlay.color;
            c.a = 1f;
            fadeOverlay.color = c;
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (lines.Count > 0)
        {
            _currentIndex = 0;
            ApplyLine(lines[0]);
        }

        StartCoroutine(BeginDialogue());
    }

    private IEnumerator BeginDialogue()
    {
        _isBusy = true;
        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(FadeFromBlack(1.0f));
        _isBusy = false;
    }

    private void Update()
    {
        if (_isBusy) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnNextPressed();
        }
    }

    private void OnNextPressed()
    {
        if (_isBusy) return;

        _currentIndex++;

        if (_currentIndex >= lines.Count)
        {
            StartCoroutine(FinishDialogueRoutine());
            return;
        }

        StartCoroutine(ShowLineRoutine(_currentIndex));
    }

    private IEnumerator ShowLineRoutine(int index)
    {
        _isBusy = true;

        DialogueLine line = lines[index];

        if (line.delayBeforeShow > 0f)
            yield return new WaitForSeconds(line.delayBeforeShow);

        if (line.useFade)
            yield return StartCoroutine(FadeToBlackAndBack(line.fadeDuration, () => ApplyLine(line)));
        else
            ApplyLine(line);

        if (line.shakeCamera)
            yield return StartCoroutine(ShakeUI(line.shakeDuration, line.shakeStrength));

        _isBusy = false;
    }

    private void ApplyLine(DialogueLine line)
    {
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

    private IEnumerator FinishDialogueRoutine()
    {
        _isBusy = true;
        yield return StartCoroutine(FadeToBlack(0.6f));

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.GoToScene(Scenes.LOBBY);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(Scenes.LOBBY);
    }

    private IEnumerator FadeToBlackAndBack(float duration, System.Action midAction)
    {
        yield return StartCoroutine(Fade(0f, 1f, duration * 0.5f));

        midAction?.Invoke();

        yield return StartCoroutine(Fade(1f, 0f, duration * 0.5f));
    }

    private IEnumerator FadeToBlack(float duration)
    {
        yield return StartCoroutine(Fade(0f, 1f, duration));
    }

    private IEnumerator FadeFromBlack(float duration)
    {
        yield return StartCoroutine(Fade(1f, 0f, duration));
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeOverlay == null)
            yield break;

        float time = 0f;
        Color color = fadeOverlay.color;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            color.a = Mathf.Lerp(from, to, t);
            fadeOverlay.color = color;
            yield return null;
        }

        color.a = to;
        fadeOverlay.color = color;
    }

    private IEnumerator ShakeUI(float duration, float strength)
    {
        if (contentRoot == null)
            yield break;

        Vector2 originalPos = contentRoot.anchoredPosition;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            float offsetX = Random.Range(-strength, strength);
            float offsetY = Random.Range(-strength, strength);

            contentRoot.anchoredPosition = originalPos + new Vector2(offsetX, offsetY);
            yield return null;
        }

        contentRoot.anchoredPosition = originalPos;
    }
}