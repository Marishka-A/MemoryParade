using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Assets.MemoryParade.Scripts.Game.GameRoot;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerHPText;
    [SerializeField] private TextMeshProUGUI enemyHPText;
    [SerializeField] private TextMeshProUGUI manaText;

    [SerializeField] private int shardsForWin = 1; // сколько осколков давать за победу

    public GameObject battleCanvas;

    private int playerHP;
    private int enemyHP = 100;
    private int playerDamage;

    private Animator playerAnimator;
    private Animator enemyAnimator;

    private BattleTrigger battle;
    private PlayerСharacteristics сharacteristics;

    public bool BattleIsEnd = false;
    public bool PlayerLose = false;

    private bool canAttack = true;

    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        battle = FindAnyObjectByType<BattleTrigger>();

        сharacteristics = PlayerСharacteristics.Instance;
        if (сharacteristics == null)
            сharacteristics = GetComponent<PlayerСharacteristics>();

        if (сharacteristics != null)
            playerHP = сharacteristics.healthPoints;
        else
            playerHP = 100;

        if (battleCanvas == null)
        {
            Canvas[] canvases = Resources.FindObjectsOfTypeAll<Canvas>();
            foreach (var canvas in canvases)
            {
                if (canvas.name == "BattleCanvas")
                {
                    battleCanvas = canvas.gameObject;
                    break;
                }
            }
        }

        if (playerHPText == null)
        {
            var allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            foreach (var text in allTexts)
            {
                if (text.name == "PlayerHP")
                {
                    playerHPText = text;
                    break;
                }
            }
        }

        if (enemyHPText == null)
        {
            var allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            foreach (var text in allTexts)
            {
                if (text.name == "EnemyHP")
                {
                    enemyHPText = text;
                    break;
                }
            }
        }

        if (manaText == null)
        {
            var allTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            foreach (var text in allTexts)
            {
                if (text.name == "ManaText")
                {
                    manaText = text;
                    break;
                }
            }
        }

        if (SceneManager.GetActiveScene().name == Scenes.GAMEPLAY && battleCanvas != null)
            battleCanvas.SetActive(false);
    }

    void Update()
    {
        if (playerHPText != null)
            playerHPText.text = playerHP.ToString();

        if (enemyHPText != null)
            enemyHPText.text = enemyHP.ToString();

        if (manaText != null && сharacteristics != null)
            manaText.text = сharacteristics.mana.ToString();
    }

    public void SetCurrentEnemyAnimator(BattleTrigger enemy)
    {
        if (enemy == null) return;

        enemyAnimator = enemy.GetComponent<Animator>();
        battle = enemy;
    }

    public void PlayerPhysicalAttack()
    {
        if (!canAttack || сharacteristics == null) return;

        canAttack = false;
        Attack();

        playerDamage = сharacteristics.strength;

        Invoke(nameof(EnemyAttack), 1f);
        EventSystem.current?.SetSelectedGameObject(null);
    }

    public void PlayerMagicAttack()
    {
        if (!canAttack || сharacteristics == null) return;

        int manaCost = 5;

        if (!сharacteristics.SpendMana(manaCost))
        {
            Debug.Log("Недостаточно маны");
            return;
        }

        canAttack = false;
        Attack();

        playerDamage = сharacteristics.strength * 2;

        Invoke(nameof(EnemyAttack), 1f);
        EventSystem.current?.SetSelectedGameObject(null);
    }

    void Attack()
    {
        if (playerAnimator == null) return;

        playerAnimator.ResetTrigger("Attack");
        playerAnimator.SetTrigger("Attack");
    }

    public void BattleEnd()
    {
        canAttack = true;
        enemyHP = 100;
    }

    public void EnemyAttack()
    {
        enemyHP -= playerDamage;

        if (enemyHP <= 0)
        {
            enemyHP = 0;
            EnemyDie();
            BattleIsEnd = true;

            if (сharacteristics != null)
                сharacteristics.healthPoints = playerHP;

            if (playerAnimator != null)
                playerAnimator.SetTrigger("battleIsEnd");

            return;
        }

        if (enemyAnimator != null)
            enemyAnimator.SetBool("turn", true);

        canAttack = true;
        Invoke(nameof(ResetEnemyAnimation), 1f);
    }

    void ResetEnemyAnimation()
    {
        if (enemyAnimator != null)
            enemyAnimator.SetBool("turn", false);

        int damage = Random.Range(1, 9);
        playerHP -= damage;

        if (playerHP <= 0)
        {
            playerHP = 0;
            PlayerDie();
            PlayerLose = true;
        }
    }

    void EnemyDie()
    {
        // Сразу начисляем осколки за победу
        ShardProgress.AddPlayerShards(shardsForWin);

        if (enemyAnimator != null)
            enemyAnimator.SetBool("die", true);
    }

    void PlayerDie()
    {
        if (playerAnimator == null) return;

        playerAnimator.SetTrigger("die");
    }
}