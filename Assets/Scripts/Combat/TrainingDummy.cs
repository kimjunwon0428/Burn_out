using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// 훈련용 더미 (허수아비) - 공격 테스트용
/// 무적 상태로 죽지 않으며, 받은 피해량을 기록
/// </summary>
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Durability))]
public class TrainingDummy : MonoBehaviour
{
    [Header("회복 설정")]
    [SerializeField] private float _recoverDelay = 1f;

    private Health _health;
    private Durability _durability;
    private Coroutine _recoverCoroutine;

    // 마지막 공격 정보 (UI 표시용)
    public float LastDamageTaken { get; private set; }
    public float LastDurabilityDamage { get; private set; }
    public float CurrentHealth => _health != null ? _health.CurrentHealth : 0f;
    public float MaxHealth => _health != null ? _health.MaxHealth : 0f;
    public float CurrentDurability => _durability != null ? _durability.CurrentDurability : 0f;
    public float MaxDurability => _durability != null ? _durability.MaxDurability : 0f;

    // 상태 프로퍼티
    public bool IsDead => _health != null && _health.IsDead;
    public bool IsGroggy => _durability != null && _durability.IsGroggy;

    // 이벤트 (UI 갱신용)
    public event Action OnDummyHit;
    public event Action OnStateChanged;

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
            _durability.OnGroggyStart += OnGroggyStart;
        }

        // 체력바 생성
        if (_health != null && EnemyHealthBarManager.Instance != null)
        {
            EnemyHealthBarManager.Instance.CreateEnemyHealthBar(_health, transform);
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
            _durability.OnGroggyStart -= OnGroggyStart;
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
    /// 사망 시 1초 후 부활
    /// </summary>
    private void OnDeath()
    {
        Debug.Log("[TrainingDummy] 사망! 1초 후 부활...");
        OnStateChanged?.Invoke();
        StartRecovery();
    }

    /// <summary>
    /// 그로기 시 1초 후 회복
    /// </summary>
    private void OnGroggyStart()
    {
        Debug.Log("[TrainingDummy] 그로기! 1초 후 회복...");
        OnStateChanged?.Invoke();
        StartRecovery();
    }

    /// <summary>
    /// 회복 코루틴 시작
    /// </summary>
    private void StartRecovery()
    {
        if (_recoverCoroutine != null)
        {
            StopCoroutine(_recoverCoroutine);
        }
        _recoverCoroutine = StartCoroutine(RecoverAfterDelay());
    }

    /// <summary>
    /// 지연 후 회복
    /// </summary>
    private IEnumerator RecoverAfterDelay()
    {
        yield return new WaitForSeconds(_recoverDelay);

        _health.FullHeal();
        _durability.FullRestore();

        Debug.Log("[TrainingDummy] 회복 완료!");
        OnStateChanged?.Invoke();
        _recoverCoroutine = null;
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
