using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private List<EnemyLevelConfig> enemyLevels;
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private Canvas endScreen;
    [SerializeField] private Canvas timerUi;
    [SerializeField] private GameObject playerSpawnPoint;
    [SerializeField] private ArenaBuilder arenaBuilder;

    public int goldAmount;
    [HideInInspector] public List<GameObject> enemies = new();
    public GameMode gameMode { set; get; }

    private PlayerScript _player;
    [HideInInspector] public List<MineScript> mines;
    public static GameManager instance { get; private set; }
    private int _mineCost = 1;
    private int _mineUpgradeCost = 4;

    public enum GameMode
    {
        Active,
        Shop,
        End
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        _player = FindFirstObjectByType<PlayerScript>();
        StartCoroutine(HandleLevels());

        _player.OnDeath += () =>
        {
            gameMode = GameMode.End;
            endScreen.gameObject.SetActive(true);
            StartCoroutine(Restart());
            return;

            IEnumerator Restart()
            {
                yield return new WaitForSeconds(2f);
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        };
    }

    private IEnumerator HandleLevels()
    {
        yield return enemyLevels.Select(levelConfig => StartCoroutine(RunLevel(levelConfig))).GetEnumerator();
    }

    private IEnumerator RunTimer(SpawnerScript spawner, EnemyLevelConfig config)
    {
        var textMeshProUGUI = timerUi.GetComponentInChildren<TextMeshProUGUI>();
        textMeshProUGUI.color = Color.antiqueWhite;
        for (var i = 5; i > 0 && !spawner.triggerActivated; i--)
        {
            textMeshProUGUI.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        textMeshProUGUI.text = "GO!!!";
        textMeshProUGUI.color = Color.chartreuse;

        arenaBuilder.Build();
        var characterController = _player.GetComponent<CharacterController>();
        characterController.enabled = false;
        _player.transform.position = playerSpawnPoint.transform.position;
        characterController.enabled = true;
        yield return spawner.SpawnAll(config);
    }

    private IEnumerator RunLevel(EnemyLevelConfig levelConfig)
    {
        var spawner = Instantiate(levelConfig.enemyPreviewPrefab);
        var spawnerScript = spawner.AddComponent<SpawnerScript>();
        spawnerScript.config = levelConfig;
        spawnerScript.spawnPosition = spawnPosition.position;
        var enemyScript = spawner.gameObject.GetComponentInChildren<EnemyScript>();
        Destroy(enemyScript);
        spawner.gameObject.SetActive(true);

        StartCoroutine(RunTimer(spawnerScript, levelConfig));


        while (!spawnerScript.triggerActivated)
        {
            yield return null;
        }

        while (enemies.Count > 0)
        {
            yield return null;
        }

        spawner.gameObject.SetActive(false);
        yield return null;
    }

    public void AddMine()
    {
        if (goldAmount < _mineCost)
        {
            return;
        }

        var pos = mines.Count == 0 ? Vector3.zero : mines[^1].transform.position;
        switch (Random.Range(1, 2))
        {
            case 1:
                pos.x += 5f;
                break;
            case 2:
                pos.z += 5f;
                break;
        }

        var mine = Instantiate(Resources.Load<GameObject>("Prefabs/Mine"), pos, Quaternion.identity);
        IncrementGold(-_mineCost);
        _mineCost = _mineCost * 3 + 1;
    }

    public void UpgradeMines()
    {
        if (goldAmount < _mineUpgradeCost)
        {
            return;
        }

        IncrementGold(-_mineUpgradeCost);
        _mineUpgradeCost *= 4;

        MineScript.GoldIncrement++;
    }

    public void IncrementGold(int amount)
    {
        goldAmount += amount;
        scoreText.text = goldAmount.ToString();
    }

    public GameObject GetClosestEnemyTo(Transform target)
    {
        return enemies
            .Where(enemy => enemy)
            .OrderBy(enemy => Vector3.Distance(target.position, enemy.transform.position))
            .FirstOrDefault();
    }

    private void OnEnable()
    {
        EnemyScript.OnEnemyKilled += EnemyScriptOnOnEnemyKilled;
    }

    private void OnDisable()
    {
        EnemyScript.OnEnemyKilled -= EnemyScriptOnOnEnemyKilled;
    }

    private void EnemyScriptOnOnEnemyKilled(int goldAMount)
    {
        goldAmount += goldAMount;
        scoreText.text = goldAmount.ToString();
    }
}