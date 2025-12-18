using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 플레이어 스탯 관리 - 기본값, 플레이스타일, 영구 업그레이드, 임시 버프 통합 관리
/// </summary>
public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("현재 플레이스타일")]
    [SerializeField] private PlaystyleType _currentPlaystyle = PlaystyleType.Light;

    // 기본 스탯 값
    private Dictionary<StatType, float> _baseStats;

    // 플레이스타일 수정자
    private Dictionary<PlaystyleType, List<StatModifier>> _playstyleModifiers;

    // 영구 업그레이드 보너스 (Flat)
    private Dictionary<StatType, float> _permanentBonuses;

    // 임시 수정자 (아이템, 버프 등)
    private List<StatModifier> _temporaryModifiers;

    // 캐시된 최종 스탯
    private Dictionary<StatType, float> _cachedFinalStats;
    private bool _isDirty = true;

    // 이벤트
    public event Action<StatType, float> OnStatChanged;
    public event Action OnStatsRecalculated;
    public event Action<float, float> OnSpecialResourceChanged;

    // 특수 자원 현재값
    private float _currentSpecialResource = 0f;

    // 프로퍼티
    public PlaystyleType CurrentPlaystyle => _currentPlaystyle;
    public float CurrentSpecialResource => _currentSpecialResource;
    public float MaxSpecialResource => GetStat(StatType.SpecialResourceMax);
    public float SpecialResourcePercent => MaxSpecialResource > 0 ? _currentSpecialResource / MaxSpecialResource : 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeBaseStats();
        InitializePlaystyleModifiers();
        _permanentBonuses = new Dictionary<StatType, float>();
        _temporaryModifiers = new List<StatModifier>();
        _cachedFinalStats = new Dictionary<StatType, float>();
    }

    /// <summary>
    /// 기본 스탯 초기화
    /// </summary>
    private void InitializeBaseStats()
    {
        _baseStats = new Dictionary<StatType, float>
        {
            // 전투 스탯
            { StatType.MaxHealth, 100f },
            { StatType.AttackPower, 10f },
            { StatType.Defense, 0f },
            { StatType.AttackSpeed, 1f },

            // 이동 스탯
            { StatType.MoveSpeed, 5f },
            { StatType.SprintMultiplier, 1.5f },
            { StatType.DodgeDistance, 3f },
            { StatType.DodgeCooldown, 0f },

            // 방어 스탯
            { StatType.GuardDamageReduction, 0.5f },
            { StatType.PerfectGuardWindow, 0.2f },
            { StatType.PerfectDodgeWindow, 0.15f },
            { StatType.InvincibilityDuration, 0.15f },

            // 특수 스탯
            { StatType.DurabilityDamage, 1f },
            { StatType.ExecutionDamage, 1f },
            { StatType.SpecialResourceMax, 100f },
            { StatType.SpecialResourceGain, 1f },
            { StatType.SpecialAttackPower, 2f },

            // 로그라이트 스탯
            { StatType.ItemDropRate, 1f },
            { StatType.GoldGain, 1f },
            { StatType.HealEfficiency, 1f },
            { StatType.ShopDiscount, 0f }
        };
    }

    /// <summary>
    /// 플레이스타일별 수정자 초기화
    /// </summary>
    private void InitializePlaystyleModifiers()
    {
        _playstyleModifiers = new Dictionary<PlaystyleType, List<StatModifier>>();

        // 경량 (Light) - 빠른 속도, 회피 특화
        _playstyleModifiers[PlaystyleType.Light] = new List<StatModifier>
        {
            StatModifier.Percent(StatType.AttackPower, -0.2f),
            StatModifier.Percent(StatType.AttackSpeed, 0.5f),
            StatModifier.Percent(StatType.MoveSpeed, 0.3f),
            StatModifier.Percent(StatType.Defense, -0.3f),
            StatModifier.Percent(StatType.DodgeDistance, 0.5f),
            StatModifier.Percent(StatType.PerfectDodgeWindow, 0.3f),
            StatModifier.Percent(StatType.SpecialResourceGain, 1.0f),  // +100% 자원 획득
            StatModifier.Percent(StatType.SpecialAttackPower, 1.0f)
        };

        // 중량 (Heavy) - 높은 공격/방어, 느린 속도
        _playstyleModifiers[PlaystyleType.Heavy] = new List<StatModifier>
        {
            StatModifier.Percent(StatType.AttackPower, 0.5f),
            StatModifier.Percent(StatType.AttackSpeed, -0.3f),
            StatModifier.Percent(StatType.MoveSpeed, -0.2f),
            StatModifier.Percent(StatType.Defense, 0.5f),
            StatModifier.Percent(StatType.MaxHealth, 0.3f),
            StatModifier.Percent(StatType.GuardDamageReduction, 0.2f),
            StatModifier.Percent(StatType.DodgeDistance, -0.3f)
        };

        // 가드 (Guard) - 퍼펙트 가드 특화
        _playstyleModifiers[PlaystyleType.Guard] = new List<StatModifier>
        {
            StatModifier.Percent(StatType.PerfectGuardWindow, 0.5f),
            StatModifier.Percent(StatType.GuardDamageReduction, 0.3f),
            StatModifier.Percent(StatType.DurabilityDamage, 1.0f),
            StatModifier.Percent(StatType.Defense, 0.2f),
            StatModifier.Percent(StatType.AttackPower, -0.1f)
        };

        // 스캐빈저 (Scavenger) - 아이템/골드 특화
        _playstyleModifiers[PlaystyleType.Scavenger] = new List<StatModifier>
        {
            StatModifier.Percent(StatType.ItemDropRate, 0.5f),
            StatModifier.Percent(StatType.GoldGain, 0.3f),
            StatModifier.Flat(StatType.ShopDiscount, 0.15f),
            StatModifier.Percent(StatType.HealEfficiency, 0.2f),
            StatModifier.Percent(StatType.MaxHealth, -0.2f),
            StatModifier.Percent(StatType.AttackPower, -0.2f)
        };
    }

    /// <summary>
    /// 최종 스탯 값 계산
    /// </summary>
    public float GetStat(StatType statType)
    {
        if (_isDirty)
        {
            RecalculateAllStats();
        }

        return _cachedFinalStats.TryGetValue(statType, out float value) ? value : 0f;
    }

    /// <summary>
    /// 모든 스탯 재계산
    /// </summary>
    private void RecalculateAllStats()
    {
        _cachedFinalStats.Clear();

        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            float finalValue = CalculateStat(statType);
            _cachedFinalStats[statType] = finalValue;
        }

        _isDirty = false;
        OnStatsRecalculated?.Invoke();
    }

    /// <summary>
    /// 개별 스탯 계산
    /// 공식: ((Base + PermanentBonus + FlatSum) × (1 + PercentSum)) × MultiplyProduct
    /// </summary>
    private float CalculateStat(StatType statType)
    {
        // 1. 기본값
        float baseValue = _baseStats.TryGetValue(statType, out float bv) ? bv : 0f;

        // 2. 영구 업그레이드 보너스
        float permanentBonus = _permanentBonuses.TryGetValue(statType, out float pb) ? pb : 0f;

        // 3. 모든 수정자 수집 (플레이스타일 + 임시)
        List<StatModifier> allModifiers = new List<StatModifier>();

        if (_playstyleModifiers.TryGetValue(_currentPlaystyle, out var playstyleModifiers))
        {
            foreach (var mod in playstyleModifiers)
            {
                if (mod.StatType == statType)
                    allModifiers.Add(mod);
            }
        }

        foreach (var mod in _temporaryModifiers)
        {
            if (mod.StatType == statType)
                allModifiers.Add(mod);
        }

        // 4. 수정자 우선순위 정렬
        allModifiers.Sort();

        // 5. 수정자 적용
        float flatSum = 0f;
        float percentSum = 0f;
        float multiplyProduct = 1f;

        foreach (var mod in allModifiers)
        {
            switch (mod.ModifierType)
            {
                case ModifierType.Flat:
                    flatSum += mod.Value;
                    break;
                case ModifierType.Percent:
                    percentSum += mod.Value;
                    break;
                case ModifierType.Multiply:
                    multiplyProduct *= mod.Value;
                    break;
            }
        }

        // 6. 최종 계산
        float finalValue = ((baseValue + permanentBonus + flatSum) * (1f + percentSum)) * multiplyProduct;

        return finalValue;
    }

    /// <summary>
    /// 플레이스타일 변경
    /// </summary>
    public void SetPlaystyle(PlaystyleType playstyle)
    {
        if (_currentPlaystyle != playstyle)
        {
            _currentPlaystyle = playstyle;
            MarkDirty();
            Debug.Log($"Playstyle changed to: {playstyle}");
        }
    }

    /// <summary>
    /// 영구 업그레이드 추가
    /// </summary>
    public void AddPermanentBonus(StatType statType, float value)
    {
        if (_permanentBonuses.ContainsKey(statType))
        {
            _permanentBonuses[statType] += value;
        }
        else
        {
            _permanentBonuses[statType] = value;
        }
        MarkDirty();
    }

    /// <summary>
    /// 임시 수정자 추가 (아이템, 버프 등)
    /// </summary>
    public void AddTemporaryModifier(StatModifier modifier)
    {
        _temporaryModifiers.Add(modifier);
        MarkDirty();
        Debug.Log($"Modifier added: {modifier}");
    }

    /// <summary>
    /// 지속 시간이 있는 임시 수정자 추가 (버프 등)
    /// </summary>
    /// <param name="modifier">스탯 수정자</param>
    /// <param name="duration">지속 시간 (초)</param>
    public void AddTemporaryModifier(StatModifier modifier, float duration)
    {
        AddTemporaryModifier(modifier);
        StartCoroutine(RemoveModifierAfterDuration(modifier, duration));
    }

    /// <summary>
    /// 지정 시간 후 수정자 자동 제거
    /// </summary>
    private IEnumerator RemoveModifierAfterDuration(StatModifier modifier, float duration)
    {
        yield return new WaitForSeconds(duration);
        RemoveTemporaryModifier(modifier);
        Debug.Log($"Temporary modifier expired: {modifier}");
    }

    /// <summary>
    /// 임시 수정자 제거
    /// </summary>
    public void RemoveTemporaryModifier(StatModifier modifier)
    {
        _temporaryModifiers.Remove(modifier);
        MarkDirty();
    }

    /// <summary>
    /// 모든 임시 수정자 초기화 (런 종료 시)
    /// </summary>
    public void ClearTemporaryModifiers()
    {
        _temporaryModifiers.Clear();
        MarkDirty();
        Debug.Log("All temporary modifiers cleared");
    }

    /// <summary>
    /// 스탯 재계산 필요 표시
    /// </summary>
    public void MarkDirty()
    {
        _isDirty = true;
    }

    /// <summary>
    /// 디버그용 스탯 출력
    /// </summary>
    public void DebugPrintStats()
    {
        Debug.Log($"=== Player Stats ({_currentPlaystyle}) ===");
        foreach (StatType statType in Enum.GetValues(typeof(StatType)))
        {
            float value = GetStat(statType);
            Debug.Log($"  {statType}: {value:F2}");
        }
    }

    /// <summary>
    /// 특수 자원 추가 (퍼펙트 회피 등에서 사용)
    /// </summary>
    public void AddSpecialResource(float amount)
    {
        float maxResource = GetStat(StatType.SpecialResourceMax);
        float oldValue = _currentSpecialResource;
        _currentSpecialResource = Mathf.Clamp(_currentSpecialResource + amount, 0, maxResource);
        if (oldValue != _currentSpecialResource)
        {
            OnSpecialResourceChanged?.Invoke(_currentSpecialResource, maxResource);
        }
    }

    /// <summary>
    /// 특수 자원 소모 시도 (특수 공격에서 사용)
    /// </summary>
    /// <returns>소모 성공 여부</returns>
    public bool TryConsumeSpecialResource(float amount)
    {
        if (_currentSpecialResource >= amount)
        {
            _currentSpecialResource -= amount;
            OnSpecialResourceChanged?.Invoke(_currentSpecialResource, MaxSpecialResource);
            return true;
        }
        return false;
    }

    /// <summary>
    /// 특수 자원 초기화 (런 시작 시)
    /// </summary>
    public void ResetSpecialResource()
    {
        _currentSpecialResource = 0f;
        OnSpecialResourceChanged?.Invoke(0f, MaxSpecialResource);
    }
}
