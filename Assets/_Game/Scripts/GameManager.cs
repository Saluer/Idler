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
        [SerializeField] private WaveModifierManager waveModifierManager;

        public int goldAmount;
        public int diamondsAmount;
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

            // Roll wave modifiers
            if (waveModifierManager != null)
                waveModifierManager.RollModifiers();

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

            _betweenRoundsTimer = StartCoroutine(BetweenRoundsTimer(spawner));

            while (!spawner.CanStart && !spawner.triggerActivated)
                yield return null;

            if (_betweenRoundsTimer != null)
            {
                StopCoroutine(_betweenRoundsTimer);
                _betweenRoundsTimer = null;
            }

            if (!spawner.triggerActivated)
                spawner.Begin();

            OnWaveStarted();

            while (!spawner.triggerActivated)
                yield return null;

            while (spawner.SpawnedEnemies < spawner.TotalEnemies)
                yield return null;

            while (enemies.Count > 0)
            {
                yield return null;
            }

            spawnerGo.SetActive(false);

            // Clear modifiers after wave
            if (waveModifierManager != null)
                waveModifierManager.ClearModifiers();
        }

        private IEnumerator BetweenRoundsTimer(SpawnerScript spawner)
        {
            var text = timerUi.GetComponentInChildren<TextMeshProUGUI>();
            text.color = Color.antiqueWhite;

            // Show modifier announcement for first few seconds
            if (waveModifierManager != null)
            {
                var announcement = waveModifierManager.GetAnnouncementText();
                if (!string.IsNullOrEmpty(announcement))
                {
                    text.text = announcement;
                    yield return new WaitForSeconds(4f);

                    for (var i = timeBetweenRounds - 4; i > 0; i--)
                    {
                        text.text = i.ToString();
                        yield return new WaitForSeconds(1f);
                    }

                    spawner.AllowStart();
                    yield break;
                }
            }

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
            UpdateScoreText();
        }

        public void IncrementDiamonds(int amount)
        {
            diamondsAmount += amount;
            UpdateScoreText();
        }

        private void UpdateScoreText()
        {
            if (scoreText != null)
                scoreText.text = $"Gold: {goldAmount}  Diamonds: {diamondsAmount}";
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

            MineScript.DiamondsIncrement++;
        }

        // ---------------- ABILITIES ----------------

        public void FreezeAllEnemies(float duration)
        {
            foreach (var enemyGo in enemies)
            {
                if (enemyGo && enemyGo.TryGetComponent<EnemyScript>(out var enemy))
                    enemy.Freeze(duration);
            }
        }

        public bool ConvertGoldToDiamonds(int goldCost, int diamondYield)
        {
            if (goldAmount < goldCost)
                return false;

            IncrementGold(-goldCost);
            IncrementDiamonds(diamondYield);
            return true;
        }

        public bool CanSpendGold(int amount)
        {
            return goldAmount >= amount;
        }

        public bool CanSpendDiamonds(int amount)
        {
            return diamondsAmount >= amount;
        }

    }
}
