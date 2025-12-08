using UnityEngine;
using System;

/// <summary>
/// 체력 컴포넌트 - 플레이어와 적 모두 사용
/// </summary>
public class Health : MonoBehaviour
{
    [Header("체력 설정")]
    [SerializeField] private float _baseMaxHealth = 100f;
    [SerializeField] private float _currentHealth;
    [SerializeField] private bool _usePlayerStats = false;  // 플레이어용: PlayerStats 사용

    [Header("방어 설정")]
    [SerializeField] private float _baseDefense = 0f;  // 기본 방어력 (데미지 감소 %)
    [SerializeField] private float _guardDamageReduction = 0.5f;  // 가드 시 데미지 감소율

    // 프로퍼티 (스탯 기반)
    public float MaxHealth => _usePlayerStats && PlayerStats.Instance != null
        ? PlayerStats.Instance.GetStat(StatType.MaxHealth)
        : _baseMaxHealth;

    public float Defense => _usePlayerStats && PlayerStats.Instance != null
        ? PlayerStats.Instance.GetStat(StatType.Defense)
        : _baseDefense;

    public float CurrentHealth => _currentHealth;
    public float HealthPercent => _currentHealth / MaxHealth;
    public bool IsAlive => _currentHealth > 0;
    public bool IsDead => _currentHealth <= 0;

    // 이벤트
    public event Action<float, float> OnHealthChanged;  // (currentHealth, maxHealth)
    public event Action<float> OnDamageTaken;           // (damageAmount)
    public event Action<float> OnHealed;                // (healAmount)
    public event Action OnDeath;

    private void Awake()
    {
        _currentHealth = MaxHealth;
    }

    private void Start()
    {
        // PlayerStats 변경 이벤트 구독 (플레이어용)
        if (_usePlayerStats && PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnStatsRecalculated += OnStatsRecalculated;
        }
    }

    private void OnDestroy()
    {
        if (_usePlayerStats && PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnStatsRecalculated -= OnStatsRecalculated;
        }
    }

    /// <summary>
    /// 스탯 재계산 시 최대 체력 변경 반영
    /// </summary>
    private void OnStatsRecalculated()
    {
        float newMaxHealth = MaxHealth;
        if (_currentHealth > newMaxHealth)
        {
            _currentHealth = newMaxHealth;
        }
        OnHealthChanged?.Invoke(_currentHealth, newMaxHealth);
    }

    /// <summary>
    /// 데미지 적용
    /// </summary>
    /// <param name="damage">받을 데미지</param>
    /// <param name="canBeGuarded">가드 가능 여부</param>
    /// <param name="isGuarding">현재 가드 중인지</param>
    /// <param name="isPerfectGuard">퍼펙트 가드 성공인지</param>
    /// <returns>실제로 받은 데미지</returns>
    public float TakeDamage(float damage, bool canBeGuarded = true, bool isGuarding = false, bool isPerfectGuard = false)
    {
        if (IsDead) return 0f;

        float actualDamage = damage;

        // 방어력 적용 (데미지 감소)
        if (Defense > 0)
        {
            actualDamage = damage * (1f - Mathf.Clamp01(Defense));
        }

        // 가드 처리
        if (isGuarding && canBeGuarded)
        {
            if (isPerfectGuard)
            {
                // 퍼펙트 가드 - 데미지 무효
                actualDamage = 0f;
                Debug.Log($"{gameObject.name}: Perfect Guard! Damage negated.");
            }
            else
            {
                // 일반 가드 - 데미지 감소
                actualDamage = damage * (1f - _guardDamageReduction);
                Debug.Log($"{gameObject.name}: Guard! Damage reduced {damage} -> {actualDamage}");
            }
        }

        // 데미지 적용
        _currentHealth = Mathf.Max(0, _currentHealth - actualDamage);

        // 이벤트 발생
        if (actualDamage > 0)
        {
            OnDamageTaken?.Invoke(actualDamage);
        }
        OnHealthChanged?.Invoke(_currentHealth, MaxHealth);

        Debug.Log($"{gameObject.name}: Took {actualDamage} damage. Health: {_currentHealth}/{MaxHealth}");

        // 사망 체크
        if (IsDead)
        {
            Die();
        }

        return actualDamage;
    }

    /// <summary>
    /// 데미지 적용 (간단 버전)
    /// </summary>
    public float TakeDamage(float damage)
    {
        return TakeDamage(damage, true, false, false);
    }

    /// <summary>
    /// 체력 회복
    /// </summary>
    public void Heal(float amount)
    {
        if (IsDead) return;

        // 회복 효율 적용 (플레이어 전용)
        float healMultiplier = _usePlayerStats && PlayerStats.Instance != null
            ? PlayerStats.Instance.GetStat(StatType.HealEfficiency)
            : 1f;

        float effectiveAmount = amount * healMultiplier;
        float actualHeal = Mathf.Min(effectiveAmount, MaxHealth - _currentHealth);
        _currentHealth += actualHeal;

        if (actualHeal > 0)
        {
            OnHealed?.Invoke(actualHeal);
        }
        OnHealthChanged?.Invoke(_currentHealth, MaxHealth);

        Debug.Log($"{gameObject.name}: Healed {actualHeal}. Health: {_currentHealth}/{MaxHealth}");
    }

    /// <summary>
    /// 체력 완전 회복
    /// </summary>
    public void FullHeal()
    {
        _currentHealth = MaxHealth;
        OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
    }

    /// <summary>
    /// 체력 설정 (초기화용)
    /// </summary>
    public void SetHealth(float health)
    {
        _currentHealth = Mathf.Clamp(health, 0, MaxHealth);
        OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
    }

    /// <summary>
    /// 기본 최대 체력 변경 (적 전용, 플레이어는 PlayerStats 사용)
    /// </summary>
    public void SetBaseMaxHealth(float maxHealth, bool healToFull = false)
    {
        _baseMaxHealth = maxHealth;
        if (healToFull)
        {
            _currentHealth = MaxHealth;
        }
        else
        {
            _currentHealth = Mathf.Min(_currentHealth, MaxHealth);
        }
        OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
    }

    /// <summary>
    /// 사망 처리
    /// </summary>
    private void Die()
    {
        Debug.Log($"{gameObject.name}: Died!");
        OnDeath?.Invoke();
    }

    /// <summary>
    /// 부활 (체력 비율 지정)
    /// </summary>
    public void Revive(float healthPercent = 1f)
    {
        _currentHealth = MaxHealth * Mathf.Clamp01(healthPercent);
        OnHealthChanged?.Invoke(_currentHealth, MaxHealth);
        Debug.Log($"{gameObject.name}: Revived with {_currentHealth} health.");
    }

    /// <summary>
    /// PlayerStats 사용 설정 (런타임)
    /// </summary>
    public void SetUsePlayerStats(bool usePlayerStats)
    {
        _usePlayerStats = usePlayerStats;
    }
}
