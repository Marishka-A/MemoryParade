using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerСharacteristics : MonoBehaviour
{
    public static PlayerСharacteristics Instance;

    // Сохраненные значения между сценами
    private static int savedHealthPoints = 100;
    private static int savedStrength = 10;
    private static int savedMana = 30;
    private static int savedMaxMana = 30;
    private static int savedSlimeKills = 0;
    private static bool savedFirstSlimeBonusGiven = false;
    private static bool savedAllSlimesBonusGiven = false;
    private static int savedNumberOfWins = 0;

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

        // Восстанавливаем сохраненные значения
        healthPoints = savedHealthPoints;
        strength = savedStrength;
        mana = savedMana;
        maxMana = savedMaxMana;
        slimeKills = savedSlimeKills;
        firstSlimeBonusGiven = savedFirstSlimeBonusGiven;
        allSlimesBonusGiven = savedAllSlimesBonusGiven;
        numberOfWins = savedNumberOfWins;
    }

    private void OnDestroy()
    {
        // Сохраняем значения перед уничтожением объекта при смене сцены
        SaveCurrentState();

        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        FindNotificationText();
    }

    private void SaveCurrentState()
    {
        savedHealthPoints = healthPoints;
        savedStrength = strength;
        savedMana = mana;
        savedMaxMana = maxMana;
        savedSlimeKills = slimeKills;
        savedFirstSlimeBonusGiven = firstSlimeBonusGiven;
        savedAllSlimesBonusGiven = allSlimesBonusGiven;
        savedNumberOfWins = numberOfWins;
    }

    public void ResetOnlyHealth()
    {
        healthPoints = 100;
        savedHealthPoints = 100;
    }

    public void AddStrength(int value)
    {
        strength += value;
        savedStrength = strength;
    }

    public void RestoreMana(int value)
    {
        mana = Mathf.Min(mana + value, maxMana);
        savedMana = mana;
    }

    public bool SpendMana(int value)
    {
        if (mana < value) return false;
        mana -= value;
        savedMana = mana;
        return true;
    }

    public void AddScore()
    {
        numberOfWins++;
        savedNumberOfWins = numberOfWins;
    }

    public void RegisterEnemyShardPickup(string enemyName, bool isLastEnemyOnMap, bool allSlimesDead)
    {
        AddScore();

        bool isSlime = enemyName.ToLower().Contains("slime");

        if (isSlime)
        {
            slimeKills++;
            savedSlimeKills = slimeKills;

            if (!firstSlimeBonusGiven)
            {
                AddStrength(10);
                firstSlimeBonusGiven = true;
                savedFirstSlimeBonusGiven = true;
                ShowNotification("Сила +10");
            }
        }

        if (allSlimesDead && !allSlimesBonusGiven)
        {
            AddStrength(25);
            allSlimesBonusGiven = true;
            savedAllSlimesBonusGiven = true;
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