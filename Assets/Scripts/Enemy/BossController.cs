using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 보스 컨트롤러 - EnemyController 확장
/// 페이즈 시스템, 랜덤 공격 패턴, 보스 전용 기능 제공
/// </summary>
public class BossController : EnemyController
{
    [Header("보스 설정")]
    [SerializeField] private string _bossName = "Boss";

    [Header("페이즈 설정")]
    [SerializeField] private List<BossPhaseData> _phases = new List<BossPhaseData>();
    private int _currentPhaseIndex = 0;

    [Header("공격 설정")]
    [SerializeField] private float _attackSelectionCooldown = 0.5f;

    // 상태
    private BossStateMachine _bossStateMachine;
    private int _lastAttackIndex = -1;

    // 프로퍼티
    public string BossName => _bossName;
    public int CurrentPhase => _currentPhaseIndex;
    public BossPhaseData CurrentPhaseData =>
        _currentPhaseIndex < _phases.Count ? _phases[_currentPhaseIndex] : null;
    public BossStateMachine BossStateMachine => _bossStateMachine;

    // 이벤트
    public event System.Action<int> OnPhaseChanged;
    public event System.Action<int> OnAttackSelected;

    protected override void Awake()
    {
        base.Awake();
        _bossStateMachine = new BossStateMachine();
        InitializePhases();

        // 보스 전용 피격 이벤트 구독
        Health.OnDamageTaken += OnBossDamageTaken;

        // Enemy 레이어 설정 (Player와 충돌 방지)
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        gameObject.layer = enemyLayer;
        Debug.Log($"[Boss] Layer set to: {enemyLayer} ({LayerMask.LayerToName(gameObject.layer)})");
    }

    protected override void Start()
    {
        // 플레이어 찾기
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            SetTarget(player.transform);
            Debug.Log($"[Boss] Target set to: {player.name} at {player.transform.position}");
        }
        else
        {
            Debug.LogError("[Boss] PlayerController not found!");
        }

        // 보스 전용 상태 머신 초기화
        _bossStateMachine.Initialize(new BossIdleState(this, _bossStateMachine));

        // Health 이벤트 구독 (페이즈 전환용)
        Health.OnDamageTaken += CheckPhaseTransition;

        Debug.Log($"[Boss] Detection range: {DetectionRange}, Position: {transform.position}");
    }

    protected override void Update()
    {
        if (IsFrozenForExecution) return;

        _bossStateMachine.Update();
        UpdateBossFacing();
    }

    /// <summary>
    /// 보스 전용 방향 업데이트 (스프라이트가 오른쪽을 바라보도록 그려져 있어서 반전)
    /// </summary>
    private void UpdateBossFacing()
    {
        if (Target == null) return;

        float direction = Target.position.x - transform.position.x;
        if (Mathf.Abs(direction) > 0.1f)
        {
            // 보스 스프라이트는 오른쪽 기본이므로 로직 반전
            // 플레이어가 오른쪽 -> localScale.x = 1 (원본)
            // 플레이어가 왼쪽 -> localScale.x = -1 (반전)
            int facingDir = direction > 0 ? 1 : -1;
            transform.localScale = new Vector3(-facingDir, 1, 1);
        }
    }

    protected override void FixedUpdate()
    {
        if (IsFrozenForExecution) return;

        _bossStateMachine.FixedUpdate();
    }

    private void OnDestroy()
    {
        if (Health != null)
        {
            Health.OnDamageTaken -= CheckPhaseTransition;
            Health.OnDamageTaken -= OnBossDamageTaken;
        }
    }

    /// <summary>
    /// 피격 시 - BossStateMachine에 전달
    /// </summary>
    private void OnBossDamageTaken(float damage)
    {
        _bossStateMachine.OnHit(damage);
    }

    /// <summary>
    /// 페이즈 초기화
    /// </summary>
    private void InitializePhases()
    {
        if (_phases.Count == 0)
        {
            // 기본 페이즈 설정
            _phases.Add(new BossPhaseData
            {
                phaseName = "Phase 1",
                healthThreshold = 0.7f,
                attackSpeedMultiplier = 1.0f,
                moveSpeedMultiplier = 1.0f,
                availableAttacks = new List<int> { 0, 1 }  // Attack_1, Attack_2
            });
            _phases.Add(new BossPhaseData
            {
                phaseName = "Phase 2",
                healthThreshold = 0.3f,
                attackSpeedMultiplier = 1.3f,
                moveSpeedMultiplier = 1.2f,
                availableAttacks = new List<int> { 0, 1, 2 }  // Attack_1, Attack_2, Attack_3
            });
        }
    }

    /// <summary>
    /// 페이즈 전환 체크
    /// </summary>
    private void CheckPhaseTransition(float damage)
    {
        float healthPercent = Health.HealthPercent;

        for (int i = _currentPhaseIndex + 1; i < _phases.Count; i++)
        {
            if (healthPercent <= _phases[i].healthThreshold)
            {
                TransitionToPhase(i);
                break;
            }
        }
    }

    /// <summary>
    /// 페이즈 전환
    /// </summary>
    private void TransitionToPhase(int newPhase)
    {
        if (newPhase == _currentPhaseIndex) return;

        int oldPhase = _currentPhaseIndex;
        _currentPhaseIndex = newPhase;

        Debug.Log($"[Boss] Phase transition: {oldPhase + 1} -> {newPhase + 1}");

        OnPhaseChanged?.Invoke(_currentPhaseIndex);
    }

    /// <summary>
    /// 랜덤 공격 선택 (페이즈 기반)
    /// </summary>
    public int SelectRandomAttack()
    {
        var currentPhase = CurrentPhaseData;
        if (currentPhase == null || currentPhase.availableAttacks.Count == 0)
        {
            return Random.Range(0, 3);  // 기본: Attack_1, 2, 3 중 랜덤
        }

        List<int> availableAttacks = new List<int>(currentPhase.availableAttacks);

        int selectedAttack = availableAttacks[Random.Range(0, availableAttacks.Count)];
        _lastAttackIndex = selectedAttack;

        OnAttackSelected?.Invoke(selectedAttack);
        return selectedAttack;
    }

    /// <summary>
    /// 공격 트리거 설정 (인덱스 기반)
    /// </summary>
    public void TriggerAttack(int attackIndex)
    {
        string triggerName = attackIndex switch
        {
            0 => "Attack1Trigger",
            1 => "Attack2Trigger",
            2 => "Attack3Trigger",
            _ => "Attack1Trigger"
        };

        Animator?.SetTrigger(triggerName);
        Debug.Log($"[Boss] Triggered attack: {triggerName}");
    }

    /// <summary>
    /// 그로기 시작 (오버라이드)
    /// </summary>
    protected override void OnGroggyStart()
    {
        _bossStateMachine.ChangeState(new BossGroggyState(this, _bossStateMachine));
    }

    /// <summary>
    /// 그로기 종료 (오버라이드)
    /// </summary>
    protected override void OnGroggyEnd()
    {
        _bossStateMachine.ChangeState(new BossIdleState(this, _bossStateMachine));
    }

    /// <summary>
    /// 사망 (오버라이드)
    /// </summary>
    protected override void OnDeath()
    {
        Debug.Log($"[Boss] {_bossName} defeated!");
        Animator?.SetBool("IsDead", true);

        // 보스 클리어 처리
        // GameManager.Instance?.OnBossDefeated();

        Destroy(gameObject, 2f);  // 보스는 좀 더 긴 시간 후 제거
    }
}

/// <summary>
/// 보스 페이즈 데이터
/// </summary>
[System.Serializable]
public class BossPhaseData
{
    public string phaseName;
    public float healthThreshold;           // HP% 이하일 때 이 페이즈로 전환
    public float attackSpeedMultiplier = 1f;// 공격 속도 배율
    public float moveSpeedMultiplier = 1f;  // 이동 속도 배율
    public List<int> availableAttacks;      // 사용 가능한 공격 인덱스 (0=Attack_1, 1=Attack_2, 2=Attack_3)
}
