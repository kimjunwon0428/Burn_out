using UnityEngine;
using System;

/// <summary>
/// 훈련용 더미 (허수아비) - 공격 테스트용
/// 무적 상태로 죽지 않으며, 받은 피해량을 기록
/// </summary>
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Durability))]
public class TrainingDummy : MonoBehaviour
{
    private Health _health;
    private Durability _durability;

    // 마지막 공격 정보 (UI 표시용)
    public float LastDamageTaken { get; private set; }
    public float LastDurabilityDamage { get; private set; }
    public float CurrentHealth => _health != null ? _health.CurrentHealth : 0f;
    public float MaxHealth => _health != null ? _health.MaxHealth : 0f;
    public float CurrentDurability => _durability != null ? _durability.CurrentDurability : 0f;
    public float MaxDurability => _durability != null ? _durability.MaxDurability : 0f;

    // 이벤트 (UI 갱신용)
    public event Action OnDummyHit;

    private void Awake()
    {
        _health = GetComponent<Health>();
        _durability = GetComponent<Durability>();
    }

    private void Start()
    {
        // 이벤트 구독
        if (_health != null)
        {
            _health.OnDamageTaken += OnHealthDamage;
            _health.OnDeath += OnDeath;
        }

        if (_durability != null)
        {
            _durability.OnDurabilityDamage += OnDurabilityDamage;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (_health != null)
        {
            _health.OnDamageTaken -= OnHealthDamage;
            _health.OnDeath -= OnDeath;
        }

        if (_durability != null)
        {
            _durability.OnDurabilityDamage -= OnDurabilityDamage;
        }
    }

    /// <summary>
    /// 체력 피해 기록
    /// </summary>
    private void OnHealthDamage(float damage)
    {
        LastDamageTaken = damage;
        OnDummyHit?.Invoke();
        Debug.Log($"[TrainingDummy] 피해: {damage}");
    }

    /// <summary>
    /// 내구력 피해 기록
    /// </summary>
    private void OnDurabilityDamage(float damage)
    {
        LastDurabilityDamage = damage;
        OnDummyHit?.Invoke();
        Debug.Log($"[TrainingDummy] 강인도 피해: {damage}");
    }

    /// <summary>
    /// 사망 시 즉시 부활 (무적)
    /// </summary>
    private void OnDeath()
    {
        Debug.Log("[TrainingDummy] 무적 - 즉시 부활!");
        _health.FullHeal();
        _durability.FullRestore();
    }

    /// <summary>
    /// 마지막 피해 정보 초기화
    /// </summary>
    public void ResetLastDamage()
    {
        LastDamageTaken = 0f;
        LastDurabilityDamage = 0f;
    }
}
