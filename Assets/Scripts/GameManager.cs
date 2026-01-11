using System;
using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private List<EnemyLevelConfig> enemyLevels;
    private int _goldAmount;

    public static GameManager instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    public void IncreaseGold(int amount)
    {
        _goldAmount += amount;
        scoreText.text = _goldAmount.ToString();
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