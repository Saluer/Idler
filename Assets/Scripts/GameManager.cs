using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private List<EnemyLevelConfig> enemyLevels;
    private int _goldAmount;
    public List<GameObject> enemies = new();
    public GameMode gameMode { set; get; }

    public static GameManager instance { get; private set; }

    public enum GameMode
    {
        Active,
        Shop
    }

    private void Awake()
    {
        instance = this;
    }

    public void IncreaseGold(int amount)
    {
        _goldAmount += amount;
        scoreText.text = _goldAmount.ToString();
    }

    public GameObject GetClosestEnemyTo(Transform target)
    {
        return enemies
            .Where(enemy => enemy != null)
            .OrderBy(enemy => Vector3.Distance(target.position, enemy.transform.position))
            .FirstOrDefault();
    }

    private void OnEnable()
    {
        EnemyScript.OnEnemyKilled += EnemyScriptOnOnEnemyKilled();
    }

    private void OnDisable()
    {
        EnemyScript.OnEnemyKilled -= EnemyScriptOnOnEnemyKilled();
    }

    private Action EnemyScriptOnOnEnemyKilled()
    {
        return () =>
        {
            _goldAmount++;
            scoreText.text = _goldAmount.ToString();
        };
    }
}