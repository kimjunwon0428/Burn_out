using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 런 매니저 - 현재 런(게임 회차)의 데이터 관리
/// 휘발성 성장 데이터 (아이템, 버프, 골드 등)
/// </summary>
public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    [Header("런 상태")]
    [SerializeField] private bool _isRunActive = false;
    [SerializeField] private int _currentStage = 1;
    [SerializeField] private int _currentRoom = 0;

    [Header("재화")]
    [SerializeField] private int _gold = 0;

    // 런 데이터
    private PlaystyleType _selectedPlaystyle;
    private List<StatModifier> _runModifiers = new List<StatModifier>();
    private float _runStartTime;
    private int _enemiesDefeated;
    private int _perfectGuards;
    private int _perfectDodges;

    // 이벤트
    public event Action OnRunStarted;
    public event Action OnRunEnded;
    public event Action<int> OnGoldChanged;
    public event Action<int, int> OnStageChanged;  // (stage, room)

    // 프로퍼티
    public bool IsRunActive => _isRunActive;
    public int CurrentStage => _currentStage;
    public int CurrentRoom => _currentRoom;
    public int Gold => _gold;
    public PlaystyleType SelectedPlaystyle => _selectedPlaystyle;
    public float RunDuration => _isRunActive ? Time.time - _runStartTime : 0f;
    public int EnemiesDefeated => _enemiesDefeated;
    public int PerfectGuards => _perfectGuards;
    public int PerfectDodges => _perfectDodges;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 새 런 시작
    /// </summary>
    public void StartRun(PlaystyleType playstyle)
    {
        if (_isRunActive)
        {
            Debug.LogWarning("Run already active! End current run first.");
            return;
        }

        _isRunActive = true;
        _selectedPlaystyle = playstyle;
        _currentStage = 1;
        _currentRoom = 0;
        _gold = 0;
        _runStartTime = Time.time;
        _enemiesDefeated = 0;
        _perfectGuards = 0;
        _perfectDodges = 0;
        _runModifiers.Clear();

        // PlayerStats에 플레이스타일 설정
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.SetPlaystyle(playstyle);
            PlayerStats.Instance.ClearTemporaryModifiers();
        }

        Debug.Log($"Run started with playstyle: {playstyle}");
        OnRunStarted?.Invoke();
    }

    /// <summary>
    /// 런 종료 (클리어 또는 사망)
    /// </summary>
    public void EndRun(bool isVictory)
    {
        if (!_isRunActive)
        {
            Debug.LogWarning("No active run to end.");
            return;
        }

        _isRunActive = false;

        // 결과 로깅
        Debug.Log($"Run ended - Victory: {isVictory}");
        Debug.Log($"  Duration: {RunDuration:F1}s");
        Debug.Log($"  Stage: {_currentStage}-{_currentRoom}");
        Debug.Log($"  Enemies: {_enemiesDefeated}");
        Debug.Log($"  Perfect Guards: {_perfectGuards}");
        Debug.Log($"  Perfect Dodges: {_perfectDodges}");
        Debug.Log($"  Gold: {_gold}");

        // 임시 수정자 정리
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ClearTemporaryModifiers();
        }

        // 메타 진행 업데이트 (예: 획득 재화 저장)
        if (MetaProgressionManager.Instance != null)
        {
            MetaProgressionManager.Instance.AddCurrency(_gold);
        }

        OnRunEnded?.Invoke();
    }

    /// <summary>
    /// 다음 방으로 이동
    /// </summary>
    public void AdvanceRoom()
    {
        _currentRoom++;
        OnStageChanged?.Invoke(_currentStage, _currentRoom);
        Debug.Log($"Advanced to room {_currentStage}-{_currentRoom}");
    }

    /// <summary>
    /// 다음 스테이지로 이동
    /// </summary>
    public void AdvanceStage()
    {
        _currentStage++;
        _currentRoom = 0;
        OnStageChanged?.Invoke(_currentStage, _currentRoom);
        Debug.Log($"Advanced to stage {_currentStage}");
    }

    /// <summary>
    /// 골드 획득
    /// </summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        // 골드 획득량 스탯 적용
        float goldGainMultiplier = PlayerStats.Instance != null
            ? PlayerStats.Instance.GetStat(StatType.GoldGain)
            : 1f;

        int actualAmount = Mathf.RoundToInt(amount * goldGainMultiplier);
        _gold += actualAmount;

        Debug.Log($"Gold gained: {actualAmount} (base: {amount}, multiplier: {goldGainMultiplier:F2})");
        OnGoldChanged?.Invoke(_gold);
    }

    /// <summary>
    /// 골드 사용
    /// </summary>
    public bool SpendGold(int amount)
    {
        if (amount <= 0 || _gold < amount) return false;

        _gold -= amount;
        Debug.Log($"Gold spent: {amount}. Remaining: {_gold}");
        OnGoldChanged?.Invoke(_gold);
        return true;
    }

    /// <summary>
    /// 런 중 스탯 수정자 추가 (아이템 등)
    /// </summary>
    public void AddRunModifier(StatModifier modifier)
    {
        _runModifiers.Add(modifier);

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddTemporaryModifier(modifier);
        }
    }

    /// <summary>
    /// 적 처치 기록
    /// </summary>
    public void RecordEnemyDefeated()
    {
        _enemiesDefeated++;
    }

    /// <summary>
    /// 퍼펙트 가드 기록
    /// </summary>
    public void RecordPerfectGuard()
    {
        _perfectGuards++;
    }

    /// <summary>
    /// 퍼펙트 닷지 기록
    /// </summary>
    public void RecordPerfectDodge()
    {
        _perfectDodges++;
    }

    /// <summary>
    /// 현재 런 통계 반환
    /// </summary>
    public RunStatistics GetRunStatistics()
    {
        return new RunStatistics
        {
            Playstyle = _selectedPlaystyle,
            Duration = RunDuration,
            Stage = _currentStage,
            Room = _currentRoom,
            EnemiesDefeated = _enemiesDefeated,
            PerfectGuards = _perfectGuards,
            PerfectDodges = _perfectDodges,
            GoldCollected = _gold
        };
    }
}

/// <summary>
/// 런 통계 구조체
/// </summary>
[Serializable]
public struct RunStatistics
{
    public PlaystyleType Playstyle;
    public float Duration;
    public int Stage;
    public int Room;
    public int EnemiesDefeated;
    public int PerfectGuards;
    public int PerfectDodges;
    public int GoldCollected;
}
