using System.Collections.Generic;
using DefaultNamespace;
using TMPro;
using UnityEngine;

public enum AbilityType
{
    Frost,
    Explosion,
    ConvertToDiamonds
}

public class ActiveAbilityManager : MonoBehaviour
{
    [Header("Costs (gold)")]
    [SerializeField] private int frostCost = 15;
    [SerializeField] private int explosionCost = 20;
    [SerializeField] private int convertCost = 10;

    [Header("Ability Settings")]
    [SerializeField] private float frostDuration = 4f;
    [SerializeField] private float explosionRadius = 8f;
    [SerializeField] private int explosionDamage = 3;
    [SerializeField] private int conversionRate = 10;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI abilityHudText;

    private readonly Dictionary<AbilityType, int> _charges = new()
    {
        { AbilityType.Frost, 0 },
        { AbilityType.Explosion, 0 }
    };

    private void Update()
    {
        if (GameManager.instance.gameMode != GameManager.GameMode.Active)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            UseFrost();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            UseExplosion();

        if (Input.GetKeyDown(KeyCode.Alpha3))
            BuyConversion();
    }

    // ---------------- SHOP PURCHASES ----------------

    public void BuyFrost()
    {
        if (!GameManager.instance.CanSpendGold(frostCost))
            return;

        GameManager.instance.IncrementGold(-frostCost);
        _charges[AbilityType.Frost]++;
        UpdateHud();
    }

    public void BuyExplosion()
    {
        if (!GameManager.instance.CanSpendGold(explosionCost))
            return;

        GameManager.instance.IncrementGold(-explosionCost);
        _charges[AbilityType.Explosion]++;
        UpdateHud();
    }

    public void BuyConversion()
    {
        GameManager.instance.ConvertGoldToDiamonds(conversionRate, 1);
    }

    // ---------------- ACTIVATION ----------------

    private void UseFrost()
    {
        if (_charges[AbilityType.Frost] <= 0)
            return;

        _charges[AbilityType.Frost]--;
        GameManager.instance.FreezeAllEnemies(frostDuration);
        UpdateHud();
    }

    private void UseExplosion()
    {
        if (_charges[AbilityType.Explosion] <= 0)
            return;

        _charges[AbilityType.Explosion]--;

        var playerPos = FindFirstObjectByType<PlayerScript>().transform.position;
        var hits = Physics.OverlapSphere(playerPos, explosionRadius);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<EnemyScript>(out var enemy))
                enemy.HandleHealthChange(-explosionDamage);
        }

        UpdateHud();
    }

    // ---------------- HUD ----------------

    private void UpdateHud()
    {
        if (!abilityHudText)
            return;

        abilityHudText.text =
            $"[1] Frost: x{_charges[AbilityType.Frost]}  " +
            $"[2] Boom: x{_charges[AbilityType.Explosion]}";
    }
}
