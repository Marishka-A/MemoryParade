using Assets.MemoryParade.Scripts.Game.Gameplay.Enemy;
using Assets.MemoryParade.Scripts.Game.GameRoot;
using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleTrigger : MonoBehaviour
{
    private Follow enemyFollow;
    private CharacterMove playerMove;
    private GameObject player;
    private CharacterAttack characterAttack;
    private SpriteRenderer spriteRendererEnemy;

    private GameObject gameplayHUD;

    public bool BattleIsStart = false;
    private BattleSystem battleSystem;

    private CinemachineVirtualCamera cinemachineVirtualCamera;
    private Camera main;

    private bool init = false;
    private bool traesure = false;
    private bool enemyDeathSequenceStarted = false;
    private bool playerLoseSequenceStarted = false;

    [SerializeField] private float battleStartDistance = 0.8f;

    void Init()
{
    enemyFollow = GetComponent<Follow>();
    gameplayHUD = GameObject.Find("GameplayHUD");

    if (enemyFollow == null)
    {
        Debug.LogWarning("Не найден скрипт Follow");
        return;
    }

    player = GameObject.FindGameObjectWithTag("Player");
    if (player == null)
    {
        Debug.LogWarning("Player не найден");
        return;
    }

    playerMove = player.GetComponent<CharacterMove>();
    if (playerMove == null)
    {
        Debug.LogWarning("Не найден CharacterMove у Player");
        return;
    }

    characterAttack = FindObjectOfType<CharacterAttack>();
    cinemachineVirtualCamera = FindAnyObjectByType<CinemachineVirtualCamera>();

    battleSystem = player.GetComponent<BattleSystem>();
    if (battleSystem == null)
    {
        Debug.LogWarning("Не найден BattleSystem у Player");
        return;
    }

    spriteRendererEnemy = gameObject.GetComponent<SpriteRenderer>();
    main = FindObjectOfType<Camera>();

    init = true;
}


    void Update()
    {
        if (!init) Init();
        if (!init) return;

        if (enemyFollow == null || playerMove == null || battleSystem == null || battleSystem.battleCanvas == null)
            return;

        enemyFollow.SetCurrentFollowEnemy(this);

        float distanceToPlayer = Vector2.Distance(playerMove.transform.position, transform.position);

        if (!battleSystem.BattleIsEnd &&
            enemyFollow.canBattle &&
            !battleSystem.battleCanvas.activeSelf &&
            distanceToPlayer < 0.8f)
        {
            battleSystem.SetCurrentEnemyAnimator(this);
            StartBattle();
        }

        if (traesure && Vector2.Distance(playerMove.transform.position, enemyFollow.transform.position) < 0.1f)
        {
            traesure = false;

            string enemyName = gameObject.name;

            GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            bool isLastEnemyOnMap = allEnemies.Length <= 1;

            bool allSlimesDead = true;
            foreach (var enemy in allEnemies)
            {
                if (enemy != gameObject && enemy.name.ToLower().Contains("slime"))
                {
                    allSlimesDead = false;
                    break;
                }
            }

            if (PlayerСharacteristics.Instance != null)
            {
                PlayerСharacteristics.Instance.RegisterEnemyShardPickup(enemyName, isLastEnemyOnMap, allSlimesDead);
            }

            Destroy(gameObject);

            if (isLastEnemyOnMap)
            {
                SceneTransitionManager.Instance.GoToScene(Scenes.LOBBY);
            }
        }

        if (battleSystem.BattleIsEnd && battleSystem.battleCanvas.activeSelf && enemyFollow.canBattle && !enemyDeathSequenceStarted)
        {
            enemyDeathSequenceStarted = true;
            StartCoroutine(WaiterEnemyDie());
        }

        if (battleSystem.PlayerLose && battleSystem.battleCanvas.activeSelf && enemyFollow.canBattle && !playerLoseSequenceStarted)
        {
            playerLoseSequenceStarted = true;
            StartCoroutine(WaiterPlayerDie());
        }
    }

    public void ChangePositionGameObject(GameObject obj)
    {
        if (obj == null || battleSystem == null || battleSystem.battleCanvas == null)
            return;

        float cameraApp = 1.533734f / 3.5f;

        BoxCollider2D boxCollider = obj.GetComponent<BoxCollider2D>();
        if (boxCollider == null) return;

        float halfHeight = boxCollider.size.y / 2 * obj.transform.localScale.y;

        Vector3 shiftPositionPlayer = new Vector3(2.26f * cameraApp, -0.08f * cameraApp + halfHeight, 0);
        Vector3 shiftPositionEnemy = new Vector3(-1.85f * cameraApp, -0.08f * cameraApp + halfHeight, 0);
        Vector3 pos = battleSystem.battleCanvas.transform.position;
        pos.z = 0;

        if (obj.CompareTag("Player"))
        {
            obj.transform.position = pos + shiftPositionPlayer;
        }
        else if (obj.CompareTag("Enemy"))
        {
            obj.transform.position = pos + shiftPositionEnemy;

            if (obj.name.ToLower().Contains("flame"))
            {
                Vector3 scale = obj.transform.localScale;
                scale.x = -Mathf.Abs(scale.x);
                obj.transform.localScale = scale;
            }
        }
    }

    void StartBattle()
    {
        if (playerMove != null)
        {
            playerMove.canMove = false;
            playerMove.enabled = false;
        }

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (battleSystem == null || battleSystem.battleCanvas == null || player == null || playerMove == null)
            return;

        Debug.Log("StartBattle");

        UpdateCamera(true);

        battleSystem.battleCanvas.SetActive(true);

        if (gameplayHUD != null)
            gameplayHUD.SetActive(false);

        ChangePositionGameObject(player);
        playerMove.enabled = false;

        if (characterAttack != null)
            characterAttack.enabled = true;

        ChangePositionGameObject(gameObject);

        enemyFollow.enabled = false;

        battleSystem.BattleEnd();

        if (spriteRendererEnemy != null)
            spriteRendererEnemy.sortingOrder = 5;

        BattleIsStart = true;
        enemyDeathSequenceStarted = false;
        playerLoseSequenceStarted = false;
    }

    void EndBattle()
    {
        if (playerMove != null)
        {
            playerMove.enabled = true;
            playerMove.canMove = true;
        }

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        if (battleSystem == null || battleSystem.battleCanvas == null || player == null)
            return;

        traesure = true;

        Vector3 pos = battleSystem.battleCanvas.transform.position;
        pos.z = 0;
        player.transform.position = pos;

        UpdateCamera(false);

        battleSystem.battleCanvas.SetActive(false);

        if (gameplayHUD != null)
            gameplayHUD.SetActive(true);

        if (characterAttack != null)
            characterAttack.enabled = false;

        // ВАЖНО — вернуть Animator
        var anim = player.GetComponent<Animator>();
        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        if (spriteRendererEnemy != null)
            spriteRendererEnemy.sortingOrder = 1;

        battleSystem.BattleIsEnd = false;
        BattleIsStart = false;
        init = false;

        var enemySr = gameObject.GetComponent<SpriteRenderer>();
        if (gameObject.name.ToLower().Contains("flame"))
        {
            Vector3 scale = gameObject.transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            gameObject.transform.localScale = scale;
        }

        enemyDeathSequenceStarted = false;
        playerLoseSequenceStarted = false;
    }

    private void UpdateCamera(bool zoom)
    {
        if (main == null) main = Camera.main;
        if (main == null) return;

        if (zoom)
        {
            if (cinemachineVirtualCamera != null)
                cinemachineVirtualCamera.enabled = false;

            main.orthographicSize = BattleCanvasManager.orthographicSize;
        }
        else
        {
            main.orthographicSize = 3.5f;

            if (player != null)
                main.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, main.transform.position.z);

            main.ResetAspect();

            if (cinemachineVirtualCamera != null)
                cinemachineVirtualCamera.enabled = true;
        }

        var brain = main.GetComponent<CinemachineBrain>();
        if (brain != null)
            brain.ManualUpdate();
    }

    IEnumerator WaiterEnemyDie()
    {
        enemyFollow.canBattle = false;
        yield return new WaitForSeconds(3f);
        EndBattle();
    }

    IEnumerator WaiterPlayerDie()
    {
        yield return new WaitForSeconds(3f);
        PlayerСharacteristics.Instance.healthPoints = 100;
        SceneTransitionManager.Instance.GoToScene(Scenes.LOBBY);
    }
}