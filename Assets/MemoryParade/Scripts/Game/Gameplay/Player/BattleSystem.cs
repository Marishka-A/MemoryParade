using Assets.MemoryParade.Scripts.Game.Gameplay.Player;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.MemoryParade.Scripts.Game.GameRoot;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerHPText;
    [SerializeField] private TextMeshProUGUI enemyHPText;

    public int attackCount = 0;
    public int powerAttackCount = 0;

    public GameObject battleCanvas;

    private int playerHP;
    private int enemyHP = 100;
    private int playerDamage;

    private Animator playerAnimator;
    private Animator enemyAnimator;

    private BattleTrigger battle;
    private PowerAttack powerAttack;
    private SuperAttack superAttack;
    private PlayerСharacteristics сharacteristics;

    public bool BattleIsEnd = false;
    public bool PlayerLose = false;

    private bool canAttack = true;

    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        battle = FindAnyObjectByType<BattleTrigger>();
        сharacteristics = PlayerСharacteristics.Instance;

        // Ищем BattleCanvas даже если он неактивен
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

        if (battleCanvas == null)
        {
            Debug.LogError("Не найден BattleCanvas");
        }

        if (сharacteristics != null)
            playerHP = сharacteristics.healthPoints;
        else
            playerHP = 100;

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

        powerAttack = FindAnyObjectByType<PowerAttack>();
        superAttack = FindAnyObjectByType<SuperAttack>();

        if (SceneManager.GetActiveScene().name == Scenes.GAMEPLAY && battleCanvas != null)
            battleCanvas.SetActive(false);
    }

    void Update()
    {
        if (playerHPText != null)
            playerHPText.text = playerHP.ToString();

        if (enemyHPText != null)
            enemyHPText.text = enemyHP.ToString();

        if (Input.GetKeyDown(KeyCode.Space) && canAttack && battle != null && battle.BattleIsStart)
        {
            PlayerAttack();
        }
        else if (playerAnimator != null && powerAttack != null && superAttack != null &&
                 !powerAttack.click && !superAttack.click)
        {
            playerAnimator.SetBool("turn", false);
        }
    }

    public void SetCurrentEnemyAnimator(BattleTrigger enemy)
    {
        if (enemy == null) return;

        enemyAnimator = enemy.GetComponent<Animator>();
        battle = enemy;
    }

    public void Attack()
    {
        if (playerAnimator == null) return;

        playerAnimator.SetBool("turn", true);
        playerAnimator.SetTrigger("Attack");
    }

    public void PlayerSuperAttack()
    {
        canAttack = false;
        Attack();
        playerDamage = 50;
        Invoke(nameof(EnemyAttack), 1f);
    }

    public void PlayerPowerAttack()
    {
        powerAttackCount++;
        Debug.Log($"Усиленная атака. powerAttackCount = {powerAttackCount}");

        canAttack = false;
        Attack();
        playerDamage = сharacteristics.baseAttack * 2;
        Invoke(nameof(EnemyAttack), 1f);
    }

    void PlayerAttack()
    {
        attackCount++;
        Debug.Log($"Обычная атака. attackCount = {attackCount}");

        canAttack = false;
        Attack();
        playerDamage = сharacteristics.baseAttack;
        Invoke(nameof(EnemyAttack), 1f);
    }

    public void BattleEnd()
    {
        powerAttackCount = 0;
        attackCount = 0;
        canAttack = true;
        enemyHP = 100;
    }

    void EnemyDie()
    {
        if (enemyAnimator == null) return;

        enemyAnimator.SetBool("die", true);
        Invoke(nameof(Treasure), 3f);
    }

    void PlayerDie()
    {
        if (playerAnimator == null) return;

        playerAnimator.SetTrigger("die");
        playerAnimator.transform.position = new Vector3(
            playerAnimator.transform.position.x,
            playerAnimator.transform.position.y - 0.3f,
            0
        );
    }

    void Treasure()
    {
        if (enemyAnimator == null) return;

        enemyAnimator.SetTrigger("treasure");
        enemyAnimator.transform.localScale = new Vector3(1.5f, 1.5f, 0);
    }

    public void EnemyAttack()
    {
        enemyHP -= playerDamage;

        if (enemyHP <= 0)
        {
            enemyHP = 0;
            Debug.Log("Вы выиграли");
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
            Debug.Log("Вы проиграли");
            PlayerDie();
            PlayerLose = true;
        }
    }
}