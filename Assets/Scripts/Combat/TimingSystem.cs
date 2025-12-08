using UnityEngine;
using System;

/// <summary>
/// 타이밍 판정 결과
/// </summary>
public enum TimingResult
{
    Miss,       // 타이밍 실패
    Normal,     // 일반 성공
    Perfect     // 퍼펙트 성공
}

/// <summary>
/// 퍼펙트 가드/닷지 타이밍 판정 시스템
/// 적의 공격 타이밍과 플레이어 입력 타이밍을 비교하여 판정
/// </summary>
public class TimingSystem : MonoBehaviour
{
    public static TimingSystem Instance { get; private set; }

    [Header("타이밍 윈도우 설정")]
    [SerializeField] private float _perfectWindow = 0.15f;  // 퍼펙트 판정 윈도우 (초)
    [SerializeField] private float _normalWindow = 0.3f;    // 일반 성공 윈도우 (초)

    // 이벤트
    public event Action<TimingResult> OnGuardTiming;
    public event Action<TimingResult> OnDodgeTiming;
    public event Action OnPerfectGuard;
    public event Action OnPerfectDodge;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 가드 타이밍 판정
    /// </summary>
    /// <param name="guardStartTime">가드 시작 시간</param>
    /// <param name="attackHitTime">공격이 히트하는 시간 (현재 시간)</param>
    /// <returns>타이밍 판정 결과</returns>
    public TimingResult JudgeGuardTiming(float guardStartTime, float attackHitTime)
    {
        float timeSinceGuard = attackHitTime - guardStartTime;

        TimingResult result;

        if (timeSinceGuard <= _perfectWindow)
        {
            result = TimingResult.Perfect;
            OnPerfectGuard?.Invoke();
            Debug.Log($"[TimingSystem] Perfect Guard! (timing: {timeSinceGuard:F3}s)");
        }
        else if (timeSinceGuard <= _normalWindow)
        {
            result = TimingResult.Normal;
            Debug.Log($"[TimingSystem] Normal Guard (timing: {timeSinceGuard:F3}s)");
        }
        else
        {
            result = TimingResult.Miss;
            Debug.Log($"[TimingSystem] Guard timing missed (timing: {timeSinceGuard:F3}s)");
        }

        OnGuardTiming?.Invoke(result);
        return result;
    }

    /// <summary>
    /// 닷지 타이밍 판정 (적 공격 회피 시)
    /// </summary>
    /// <param name="dodgeStartTime">닷지 시작 시간</param>
    /// <param name="attackHitTime">공격이 히트하는 시간 (현재 시간)</param>
    /// <returns>타이밍 판정 결과</returns>
    public TimingResult JudgeDodgeTiming(float dodgeStartTime, float attackHitTime)
    {
        float timeSinceDodge = attackHitTime - dodgeStartTime;

        TimingResult result;

        if (timeSinceDodge <= _perfectWindow)
        {
            result = TimingResult.Perfect;
            OnPerfectDodge?.Invoke();
            Debug.Log($"[TimingSystem] Perfect Dodge! (timing: {timeSinceDodge:F3}s)");
        }
        else if (timeSinceDodge <= _normalWindow)
        {
            result = TimingResult.Normal;
            Debug.Log($"[TimingSystem] Normal Dodge (timing: {timeSinceDodge:F3}s)");
        }
        else
        {
            result = TimingResult.Miss;
            Debug.Log($"[TimingSystem] Dodge timing missed (timing: {timeSinceDodge:F3}s)");
        }

        OnDodgeTiming?.Invoke(result);
        return result;
    }

    /// <summary>
    /// 퍼펙트 윈도우 내인지 직접 확인
    /// </summary>
    public bool IsWithinPerfectWindow(float actionStartTime)
    {
        return (Time.time - actionStartTime) <= _perfectWindow;
    }

    /// <summary>
    /// 일반 윈도우 내인지 직접 확인
    /// </summary>
    public bool IsWithinNormalWindow(float actionStartTime)
    {
        return (Time.time - actionStartTime) <= _normalWindow;
    }

    /// <summary>
    /// 타이밍 윈도우 설정값 변경 (밸런싱용)
    /// </summary>
    public void SetTimingWindows(float perfectWindow, float normalWindow)
    {
        _perfectWindow = perfectWindow;
        _normalWindow = normalWindow;
    }
}
