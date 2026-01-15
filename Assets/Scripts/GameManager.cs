using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private List<EnemyLevelConfig> enemyLevels;
    [SerializeField] private Transform spawnPosition;

    [DoNotSerialize] public int goldAmount { get; private set; }
    [DoNotSerialize] public List<GameObject> enemies = new();
    [DoNotSerialize] public GameMode gameMode { set; get; }
    [SerializeField] private Canvas endScreen;

    private PlayerScript player;
    public static GameManager instance { get; private set; }

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
        player = FindFirstObjectByType<PlayerScript>();
        StartCoroutine(HandleLevels());
        player.OnDeath += () =>
        {
            gameMode = GameMode.End;
            endScreen.gameObject.SetActive(true);
            StartCoroutine(Restart());

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

    private IEnumerator RunLevel(EnemyLevelConfig levelConfig)
    {
        var spawner = Instantiate(levelConfig.enemyPreviewPrefab);
        var spawnerScript = spawner.AddComponent<SpawnerScript>();
        spawnerScript.config = levelConfig;
        spawnerScript.spawnPosition = spawnPosition.position;
        var enemyScript = spawner.gameObject.GetComponentInChildren<EnemyScript>();
        Destroy(enemyScript);
        spawner.gameObject.SetActive(true);

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

    public void IncreaseGold(int amount)
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