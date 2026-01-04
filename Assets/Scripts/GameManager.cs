using System;
using DefaultNamespace;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    private int _goldAmount;

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