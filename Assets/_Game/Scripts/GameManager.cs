using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class GameManager : MonoBehaviour
    {
        public static event System.Action OnForceCloseShop;

        [SerializeField] private int timeBetweenRounds = 60;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private List<EnemyLevelConfig> enemyLevels;
        [SerializeField] private Transform spawnPosition;
        [SerializeField] private Canvas endScreen;
        [SerializeField] private Canvas timerUi;
        [SerializeField] private GameObject playerSpawnPoint;
        [SerializeField] private Canvas exitConfirmCanvas;

        public int goldAmount;
        [HideInInspector] public List<GameObject> enemies = new();
        public GameMode gameMode { set; get; }

        private PlayerScript _player;
        [HideInInspector] public List<MineScript> mines;

        public static GameManager instance { get; private set; }

        private int _mineCost = 1;
        private int _mineUpgradeCost = 4;
        private bool _isExitMenuOpen;

        private Coroutine _betweenRoundsTimer;

        public enum GameMode
        {
            Active,
            Shop,
            End,
            MainMenu
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

        // ---------------- LEVEL FLOW ----------------

        private IEnumerator HandleLevels()
        {
            foreach (var levelConfig in enemyLevels)
            {
                yield return StartCoroutine(RunLevel(levelConfig));
            }

            ToggleExitMenu();
        }

        private IEnumerator RunLevel(EnemyLevelConfig levelConfig)
        {
            enemies.Clear();

            var spawnerGo = Instantiate(levelConfig.enemyPreviewPrefab);
            var spawner = spawnerGo.AddComponent<SpawnerScript>();

            spawner.Init(
                config: levelConfig,
                spawnPosition: spawnPosition.position,
                spawnRadius: 30f,
                baseSpawnDelay: 0.5f,
                minSpawnDelay: 0.1f,
                killAccelerationMultiplier: 0.85f
            );

            var previewEnemy = spawnerGo.GetComponentInChildren<EnemyScript>();
            if (previewEnemy != null)
                Destroy(previewEnemy);

            spawnerGo.SetActive(true);

            // запускаем таймер МЕЖДУ волнами
            _betweenRoundsTimer = StartCoroutine(BetweenRoundsTimer(spawner));

            // ждём либо ручного старта, либо окончания таймера
            while (!spawner.CanStart && !spawner.triggerActivated)
                yield return null;

            // если волна стартовала вручную — убиваем таймер
            if (_betweenRoundsTimer != null)
            {
                StopCoroutine(_betweenRoundsTimer);
                _betweenRoundsTimer = null;
            }

            // если старт разрешён таймером — стартуем волну
            if (!spawner.triggerActivated)
                spawner.Begin();

            OnWaveStarted();

            // ждём реального старта
            while (!spawner.triggerActivated)
                yield return null;

            // ждём, пока заспавнится весь пул
            while (spawner.SpawnedEnemies < spawner.TotalEnemies)
                yield return null;

            // ждём, пока всех убьют
            while (enemies.Count > 0)
            {
                yield return null;
            }

            spawnerGo.SetActive(false);
        }

        private IEnumerator BetweenRoundsTimer(SpawnerScript spawner)
        {
            var text = timerUi.GetComponentInChildren<TextMeshProUGUI>();
            text.color = Color.antiqueWhite;

            for (var i = timeBetweenRounds; i > 0; i--)
            {
                text.text = i.ToString();
                yield return new WaitForSeconds(1f);
            }

            spawner.AllowStart();
        }

        private void OnWaveStarted()
        {
            var text = timerUi.GetComponentInChildren<TextMeshProUGUI>();
            text.text = "GO!!!";
            text.color = Color.chartreuse;

            var cc = _player.GetComponent<CharacterController>();
            cc.enabled = false;
            _player.transform.position = playerSpawnPoint.transform.position;
            cc.enabled = true;
        }


        // ---------------- ESC / PAUSE ----------------

        private void Update()
        {
            if (gameMode == GameMode.End)
                return;

            if (!Input.GetKeyDown(KeyCode.Escape))
                return;

            if (_isExitMenuOpen)
            {
                CloseExitMenu();
                return;
            }

            if (gameMode == GameMode.Shop)
            {
                OnForceCloseShop?.Invoke();
            }
            else
            {
                OpenExitMenu();
            }
        }

        private void OpenExitMenu()
        {
            _isExitMenuOpen = true;
            gameMode = GameMode.MainMenu;

            exitConfirmCanvas.gameObject.SetActive(true);
            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void CloseExitMenu()
        {
            _isExitMenuOpen = false;
            gameMode = GameMode.Active;

            exitConfirmCanvas.gameObject.SetActive(false);
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void ToggleExitMenu()
        {
            if (_isExitMenuOpen)
                CloseExitMenu();
            else
                OpenExitMenu();
        }

        // ---------------- ECONOMY ----------------

        public void IncrementGold(int amount)
        {
            goldAmount += amount;
            scoreText.text = goldAmount.ToString();
        }

        private void OnEnable()
        {
            EnemyScript.OnEnemyKilled += OnEnemyKilled;
        }

        private void OnDisable()
        {
            EnemyScript.OnEnemyKilled -= OnEnemyKilled;
        }

        private void OnEnemyKilled(int goldAmount)
        {
            IncrementGold(goldAmount);
        }

        public GameObject GetClosestEnemyTo(Transform target)
        {
            return enemies
                .Where(e => e)
                .OrderBy(e => Vector3.Distance(target.position, e.transform.position))
                .FirstOrDefault();
        }
        
        // ---------------- MINES ----------------

        public void AddMine()
        {
            if (goldAmount < _mineCost)
                return;

            if (mines == null)
                mines = new List<MineScript>();

            var pos = mines.Count == 0
                ? Vector3.zero
                : mines[^1].transform.position;

            switch (Random.Range(1, 3))
            {
                case 1:
                    pos.x += 5f;
                    break;
                case 2:
                    pos.z += 5f;
                    break;
            }

            var mineGo = Instantiate(
                Resources.Load<GameObject>("Prefabs/Mine"),
                pos,
                Quaternion.identity
            );

            var mine = mineGo.GetComponent<MineScript>();
            mines.Add(mine);

            IncrementGold(-_mineCost);
            _mineCost = _mineCost * 3 + 1;
        }

        public void UpgradeMines()
        {
            if (goldAmount < _mineUpgradeCost)
                return;

            IncrementGold(-_mineUpgradeCost);
            _mineUpgradeCost *= 4;

            MineScript.GoldIncrement++;
        }

    }
}