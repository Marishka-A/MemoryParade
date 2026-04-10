using System;
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

    public enum DialogueActionType
    {
        None,

        ShowTopTreeGod,
        HideTopTreeGod,
        ShowTopEyeGod,
        HideTopEyeGod,
        ShowTopHero,
        HideTopHero,

        ShowBarrier,
        HideBarrier,
        ShowDoor,
        HideDoor,

        EyeGodMoveToBarrier,
        EyeGodLeaveRight,
        EyeGodMoveToHero,
        HeroMoveToDoor,

        Wait
    }

    [System.Serializable]
    public class DialogueAction
    {
        public DialogueActionType actionType = DialogueActionType.None;
        public float actionDuration = 0.8f;
    }

    [System.Serializable]
    public class DialogueLine
    {
        [Header("Dialogue")]
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
        public float shakeStrength = 18f;

        public float delayBeforeShow = 0f;

        [Header("Actions")]
        public List<DialogueAction> actions = new List<DialogueAction>();
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

    [Header("Typing")]
    [SerializeField] private float typingSpeed = 0.02f;

    [Header("Visual")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 0.45f);

    [Header("Dialogue")]
    [SerializeField] private List<DialogueLine> lines = new List<DialogueLine>();

    [Header("Top Scene Objects")]
    [SerializeField] private RectTransform topTreeGod;
    [SerializeField] private RectTransform topEyeGod;
    [SerializeField] private RectTransform topHero;
    [SerializeField] private RectTransform topBarrier;
    [SerializeField] private RectTransform topDoor;

    [Header("Top Scene Targets")]
    [SerializeField] private RectTransform eyeGodBarrierTarget;
    [SerializeField] private RectTransform eyeGodOffscreenRightTarget;
    [SerializeField] private RectTransform eyeGodHeroTarget;
    [SerializeField] private RectTransform heroDoorTarget;

    [Header("Top Scene Movement")]
    [SerializeField] private float topMoveSpeed = 500f;

    private int _currentIndex = 0;
    private bool _isBusy = false;
    private bool _isTyping = false;
    private string _fullCurrentText = "";
    private Coroutine _typingCoroutine;

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

        InitializeTopSceneObjects();

        _currentIndex = 0;
        StartCoroutine(BeginDialogue());
    }

    private void InitializeTopSceneObjects()
    {
        SetTopObjectActive(topTreeGod, true);
        SetTopObjectActive(topEyeGod, true);

        SetTopObjectActive(topHero, false);
        SetTopObjectActive(topBarrier, false);
        SetTopObjectActive(topDoor, false);
    }

    private void Update()
    {
        if (_isBusy) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnNextPressed();
        }
    }

    private IEnumerator BeginDialogue()
    {
        _isBusy = true;
        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(FadeFromBlack(1.4f));
        _isBusy = false;

        if (lines.Count > 0)
            yield return StartCoroutine(ShowLineRoutine(0));
    }

    private void OnNextPressed()
    {
        if (_isBusy) return;

        if (_isTyping)
        {
            CompleteTypingInstantly();
            return;
        }

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
        {
            yield return StartCoroutine(Fade(0f, 1f, line.fadeDuration * 0.5f));

            if (line.actions != null && line.actions.Count > 0)
                yield return StartCoroutine(HandleActions(line.actions));

            ApplyLine(line, instantText: false);

            yield return StartCoroutine(Fade(1f, 0f, line.fadeDuration * 0.5f));
        }
        else
        {
            if (line.actions != null && line.actions.Count > 0)
                yield return StartCoroutine(HandleActions(line.actions));

            ApplyLine(line, instantText: false);
        }

        if (line.shakeCamera)
            yield return StartCoroutine(ShakeUI(line.shakeDuration, line.shakeStrength));

        _isBusy = false;
    }

    private void ApplyLine(DialogueLine line, bool instantText)
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

        _fullCurrentText = line.text ?? "";

        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);

        if (dialogueText != null)
        {
            if (instantText || string.IsNullOrEmpty(_fullCurrentText))
            {
                dialogueText.text = _fullCurrentText;
                _isTyping = false;
            }
            else
            {
                dialogueText.text = "";
                _typingCoroutine = StartCoroutine(TypeText(_fullCurrentText));
            }
        }
    }

    private IEnumerator TypeText(string text)
    {
        _isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        _isTyping = false;
        _typingCoroutine = null;
    }

    private void CompleteTypingInstantly()
    {
        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);

        if (dialogueText != null)
            dialogueText.text = _fullCurrentText;

        _isTyping = false;
        _typingCoroutine = null;
    }

    private IEnumerator HandleActions(List<DialogueAction> actions)
    {
        foreach (var action in actions)
        {
            yield return StartCoroutine(HandleSingleAction(action));
        }
    }

    private IEnumerator HandleSingleAction(DialogueAction action)
    {
        switch (action.actionType)
        {
            case DialogueActionType.ShowTopTreeGod:
                SetTopObjectActive(topTreeGod, true);
                yield break;

            case DialogueActionType.HideTopTreeGod:
                SetTopObjectActive(topTreeGod, false);
                yield break;

            case DialogueActionType.ShowTopEyeGod:
                SetTopObjectActive(topEyeGod, true);
                yield break;

            case DialogueActionType.HideTopEyeGod:
                SetTopObjectActive(topEyeGod, false);
                yield break;

            case DialogueActionType.ShowTopHero:
                SetTopObjectActive(topHero, true);
                yield break;

            case DialogueActionType.HideTopHero:
                SetTopObjectActive(topHero, false);
                yield break;

            case DialogueActionType.ShowBarrier:
                SetTopObjectActive(topBarrier, true);
                yield break;

            case DialogueActionType.HideBarrier:
                SetTopObjectActive(topBarrier, false);
                yield break;

            case DialogueActionType.ShowDoor:
                SetTopObjectActive(topDoor, true);
                yield break;

            case DialogueActionType.HideDoor:
                SetTopObjectActive(topDoor, false);
                yield break;

            case DialogueActionType.EyeGodMoveToBarrier:
                if (topEyeGod != null && eyeGodBarrierTarget != null)
                    yield return StartCoroutine(MoveRect(topEyeGod, eyeGodBarrierTarget.anchoredPosition));
                yield break;

            case DialogueActionType.EyeGodLeaveRight:
                if (topEyeGod != null && eyeGodOffscreenRightTarget != null)
                    yield return StartCoroutine(MoveRect(topEyeGod, eyeGodOffscreenRightTarget.anchoredPosition));
                yield break;

            case DialogueActionType.EyeGodMoveToHero:
                if (topEyeGod != null && eyeGodHeroTarget != null)
                    yield return StartCoroutine(MoveRect(topEyeGod, eyeGodHeroTarget.anchoredPosition));
                yield break;

            case DialogueActionType.HeroMoveToDoor:
                if (topHero != null && heroDoorTarget != null)
                {
                    yield return StartCoroutine(MoveRect(topHero, heroDoorTarget.anchoredPosition));
                    SetTopObjectActive(topHero, false);
                }
                yield break;

            case DialogueActionType.Wait:
                yield return new WaitForSeconds(action.actionDuration);
                yield break;
        }
    }

    private IEnumerator MoveRect(RectTransform rect, Vector2 targetAnchoredPos)
    {
        if (rect == null)
            yield break;

        while (Vector2.Distance(rect.anchoredPosition, targetAnchoredPos) > 2f)
        {
            rect.anchoredPosition = Vector2.MoveTowards(
                rect.anchoredPosition,
                targetAnchoredPos,
                topMoveSpeed * Time.deltaTime
            );

            yield return null;
        }

        rect.anchoredPosition = targetAnchoredPos;
    }

    private void SetTopObjectActive(RectTransform rect, bool state)
    {
        if (rect != null)
            rect.gameObject.SetActive(state);
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

    private IEnumerator FadeToBlackAndBack(float duration, Action midAction)
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

            float offsetX = UnityEngine.Random.Range(-strength, strength);
            float offsetY = UnityEngine.Random.Range(-strength, strength);

            contentRoot.anchoredPosition = originalPos + new Vector2(offsetX, offsetY);
            yield return null;
        }

        contentRoot.anchoredPosition = originalPos;
    }
}