using UnityEngine;
using System;

/// <summary>
/// 내구력 컴포넌트 - 적에게 사용
/// 내구력이 0이 되면 그로기 상태로 전환되어 처형 가능
/// </summary>
public class Durability : MonoBehaviour
{
    [Header("내구력 설정")]
    [SerializeField] private float _maxDurability = 100f;
    [SerializeField] private float _currentDurability;

    [Header("회복 설정")]
    [SerializeField] private float _recoveryDelay = 3f;      // 피격 후 회복 시작까지 딜레이
    [SerializeField] private float _recoveryRate = 10f;      // 초당 회복량
    [SerializeField] private bool _autoRecover = true;       // 자동 회복 여부

    [Header("그로기 설정")]
    [SerializeField] private float _groggyDuration = 5f;     // 그로기 지속 시간

    // 상태
    private float _lastDamageTime;
    private bool _isGroggy;
    private float _groggyTimer;

    // 프로퍼티
    public float MaxDurability => _maxDurability;
    public float CurrentDurability => _currentDurability;
    public float DurabilityPercent => _currentDurability / _maxDurability;
    public bool IsGroggy => _isGroggy;
    public float GroggyTimeRemaining => _isGroggy ? (_groggyDuration - _groggyTimer) : 0f;

    // 이벤트
    public event Action<float, float> OnDurabilityChanged;  // (current, max)
    public event Action<float> OnDurabilityDamage;          // (damageAmount)
    public event Action OnGroggyStart;
    public event Action OnGroggyEnd;
    public event Action OnExecuted;  // 처형당했을 때

    private void Awake()
    {
        _currentDurability = _maxDurability;
    }

    private void Update()
    {
        if (_isGroggy)
        {
            UpdateGroggy();
        }
        else if (_autoRecover)
        {
            UpdateRecovery();
        }
    }

    /// <summary>
    /// 내구력 데미지
    /// </summary>
    public void TakeDurabilityDamage(float damage)
    {
        if (_isGroggy) return;  // 그로기 중에는 내구력 데미지 무시

        _currentDurability = Mathf.Max(0, _currentDurability - damage);
        _lastDamageTime = Time.time;

        OnDurabilityDamage?.Invoke(damage);
        OnDurabilityChanged?.Invoke(_currentDurability, _maxDurability);

        Debug.Log($"{gameObject.name}: Durability damage {damage}. Durability: {_currentDurability}/{_maxDurability}");

        // 내구력 0 -> 그로기
        if (_currentDurability <= 0)
        {
            EnterGroggy();
        }
    }

    /// <summary>
    /// 퍼펙트 가드로 인한 추가 내구력 데미지
    /// </summary>
    public void TakePerfectGuardDamage(float baseDamage, float multiplier = 2f)
    {
        TakeDurabilityDamage(baseDamage * multiplier);
    }

    /// <summary>
    /// 그로기 상태 진입
    /// </summary>
    private void EnterGroggy()
    {
        _isGroggy = true;
        _groggyTimer = 0f;

        Debug.Log($"{gameObject.name}: Entered GROGGY state!");
        OnGroggyStart?.Invoke();
    }

    /// <summary>
    /// 그로기 상태 업데이트
    /// </summary>
    private void UpdateGroggy()
    {
        _groggyTimer += Time.deltaTime;

        if (_groggyTimer >= _groggyDuration)
        {
            ExitGroggy();
        }
    }

    /// <summary>
    /// 그로기 상태 종료
    /// </summary>
    private void ExitGroggy()
    {
        _isGroggy = false;
        _currentDurability = _maxDurability;  // 내구력 완전 회복

        Debug.Log($"{gameObject.name}: Exited GROGGY state. Durability restored.");
        OnGroggyEnd?.Invoke();
        OnDurabilityChanged?.Invoke(_currentDurability, _maxDurability);
    }

    /// <summary>
    /// 내구력 자동 회복 업데이트
    /// </summary>
    private void UpdateRecovery()
    {
        // 피격 후 딜레이 체크
        if (Time.time - _lastDamageTime < _recoveryDelay)
            return;

        // 이미 최대면 회복 불필요
        if (_currentDurability >= _maxDurability)
            return;

        float recovered = _recoveryRate * Time.deltaTime;
        _currentDurability = Mathf.Min(_maxDurability, _currentDurability + recovered);

        OnDurabilityChanged?.Invoke(_currentDurability, _maxDurability);
    }

    /// <summary>
    /// 처형당했을 때 호출
    /// </summary>
    public void OnExecute()
    {
        Debug.Log($"{gameObject.name}: EXECUTED!");
        OnExecuted?.Invoke();

        // 그로기 상태였으면 OnGroggyEnd 이벤트 발생
        bool wasGroggy = _isGroggy;
        _isGroggy = false;
        _currentDurability = _maxDurability;

        if (wasGroggy)
        {
            OnGroggyEnd?.Invoke();
        }
        OnDurabilityChanged?.Invoke(_currentDurability, _maxDurability);
    }

    /// <summary>
    /// 처형 가능 여부
    /// </summary>
    public bool CanBeExecuted()
    {
        return _isGroggy;
    }

    /// <summary>
    /// 내구력 설정
    /// </summary>
    public void SetDurability(float durability)
    {
        _currentDurability = Mathf.Clamp(durability, 0, _maxDurability);
        OnDurabilityChanged?.Invoke(_currentDurability, _maxDurability);
    }

    /// <summary>
    /// 내구력 완전 회복
    /// </summary>
    public void FullRestore()
    {
        _currentDurability = _maxDurability;
        OnDurabilityChanged?.Invoke(_currentDurability, _maxDurability);
    }
}
