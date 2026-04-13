using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerСharacteristics : MonoBehaviour
{
    public static PlayerСharacteristics Instance;

    public int healthPoints = 100;

    public int strength = 10;
    public int mana = 30;
    public int maxMana = 30;

    public int slimeKills = 0;
    public bool firstSlimeBonusGiven = false;
    public bool allSlimesBonusGiven = false;

    public int numberOfWins = 0;

    private TextMeshProUGUI notificationText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        FindNotificationText();
    }

    public void AddStrength(int value)
    {
        strength += value;
    }

    public void RestoreMana(int value)
    {
        mana = Mathf.Min(mana + value, maxMana);
    }

    public bool SpendMana(int value)
    {
        if (mana < value) return false;
        mana -= value;
        return true;
    }

    public void AddScore()
    {
        numberOfWins++;
    }

    public void RegisterEnemyShardPickup(string enemyName, bool isLastEnemyOnMap, bool allSlimesDead)
    {
        AddScore();

        bool isSlime = enemyName.ToLower().Contains("slime");

        if (isSlime)
        {
            slimeKills++;

            if (!firstSlimeBonusGiven)
            {
                AddStrength(10);
                firstSlimeBonusGiven = true;
                ShowNotification("Сила +10");
            }
        }

        if (allSlimesDead && !allSlimesBonusGiven)
        {
            AddStrength(25);
            allSlimesBonusGiven = true;
            ShowNotification("Сила +25");
        }
    }

    public void ShowNotification(string message)
    {
        if (notificationText == null)
            FindNotificationText();

        if (notificationText == null)
        {
            Debug.Log(message);
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ShowNotificationRoutine(message));
    }

    private IEnumerator ShowNotificationRoutine(string message)
    {
        notificationText.gameObject.SetActive(true);
        notificationText.text = message;

        yield return new WaitForSeconds(2f);

        notificationText.text = "";
        notificationText.gameObject.SetActive(false);
    }

    private void FindNotificationText()
    {
        var allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        foreach (var text in allTexts)
        {
            if (text.name == "NotificationText")
            {
                notificationText = text;
                notificationText.gameObject.SetActive(false);
                break;
            }
        }
    }
}