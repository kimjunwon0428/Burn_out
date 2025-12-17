using UnityEngine;

/// <summary>
/// Guard 상태 - 플레이어 방어
/// 가드 버튼을 누르고 있는 동안 유지
/// </summary>
public class GuardState : PlayerState
{
    private PlayerMovement _movement;

    // 가드 설정 (기본값)
    private float _baseDamageReduction = 0.5f;  // 50% 데미지 감소
    private float _guardMoveSpeedMultiplier = 0.3f;  // 가드 중 이동 속도 30%

    // 퍼펙트 가드 설정 (기본값)
    private float _basePerfectGuardWindow = 0.2f;  // 퍼펙트 가드 타이밍 윈도우
    private float _guardStartTime;

    // 스탯 기반 수치
    private float GuardDamageReduction => PlayerStats.Instance != null
        ? PlayerStats.Instance.GetStat(StatType.GuardDamageReduction)
        : _baseDamageReduction;

    private float PerfectGuardWindow => PlayerStats.Instance != null
        ? PlayerStats.Instance.GetStat(StatType.PerfectGuardWindow)
        : _basePerfectGuardWindow;

    /// <summary>
    /// 현재 퍼펙트 가드 윈도우 내인지 확인 (스탯 기반)
    /// </summary>
    public bool IsInPerfectGuardWindow => (Time.time - _guardStartTime) <= PerfectGuardWindow;

    /// <summary>
    /// 가드 중 데미지 감소율 (스탯 기반)
    /// </summary>
    public float DamageReduction => GuardDamageReduction;

    public GuardState(PlayerController controller, PlayerStateMachine stateMachine, PlayerMovement movement)
        : base(controller, stateMachine)
    {
        _movement = movement;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _guardStartTime = Time.time;

        // 가드 애니메이션 트리거
        _controller.Animator.SetBool("IsGuarding", true);

        Debug.Log("Guard started");
    }

    public override void HandleInput()
    {
        // 가드 중에도 제한된 이동 가능
        Vector2 moveInput = InputManager.Instance.MoveInput;

        // 가드 중 이동 입력 적용 (속도 감소)
        _movement.SetMoveInput(moveInput * _guardMoveSpeedMultiplier);
    }

    public override void CheckTransitions()
    {
        // 가드 버튼을 떼면 상태 종료
        if (!InputManager.Instance.GuardHeld)
        {
            // 이동 입력이 있으면 Move, 없으면 Idle
            if (InputManager.Instance.MoveInput.magnitude > 0.1f)
            {
                _stateMachine.ChangeState(new MoveState(_controller, _stateMachine, _movement));
            }
            else
            {
                _stateMachine.ChangeState(new IdleState(_controller, _stateMachine, _movement));
            }
        }

        // 가드 중 닷지 입력 시 닷지로 전환 (캔슬)
        if (InputManager.Instance.DodgePressed)
        {
            _stateMachine.ChangeState(new DodgeState(_controller, _stateMachine, _movement));
        }
    }

    public override void OnExit()
    {
        base.OnExit();

        // 가드 애니메이션 해제
        _controller.Animator.SetBool("IsGuarding", false);

        Debug.Log("Guard ended");
    }

    /// <summary>
    /// 가드 상태에서 피격 처리 (외부에서 호출)
    /// </summary>
    /// <param name="incomingDamage">받는 데미지</param>
    /// <param name="canBeGuarded">가드 가능한 공격인지</param>
    /// <param name="attacker">공격자 (퍼펙트 가드 시 내구력 데미지용)</param>
    /// <returns>실제로 받을 데미지</returns>
    public float ProcessGuardedDamage(float incomingDamage, bool canBeGuarded = true, EnemyController attacker = null)
    {
        if (!canBeGuarded)
        {
            // 가드 불가 공격은 풀 데미지
            Debug.Log("Unblockable attack! Full damage taken.");
            return incomingDamage;
        }

        if (IsInPerfectGuardWindow)
        {
            // 퍼펙트 가드 성공 - 데미지 무효
            Debug.Log("Perfect Guard! Damage negated.");
            OnPerfectGuard(attacker);
            return 0f;
        }
        else
        {
            // 일반 가드 - 데미지 감소 (스탯 기반)
            float reducedDamage = incomingDamage * (1f - GuardDamageReduction);
            Debug.Log($"Guard blocked! Damage reduced: {incomingDamage} -> {reducedDamage}");
            return reducedDamage;
        }
    }

    /// <summary>
    /// 퍼펙트 가드 성공 시 호출
    /// </summary>
    /// <param name="attacker">공격자 (내구력 데미지를 줄 대상)</param>
    private void OnPerfectGuard(EnemyController attacker)
    {
        // 패링 애니메이션 트리거
        _controller.Animator.SetTrigger("ParryTrigger");

        // 적 내구력에 추가 데미지 (퍼펙트 가드 보너스)
        if (attacker != null && attacker.Durability != null)
        {
            // 기본 내구력 데미지 (스탯 기반)
            float baseDurabilityDamage = PlayerStats.Instance != null
                ? PlayerStats.Instance.GetStat(StatType.DurabilityDamage)
                : 10f;

            // TakePerfectGuardDamage는 2배 데미지를 적용함
            attacker.Durability.TakePerfectGuardDamage(baseDurabilityDamage);
            Debug.Log($"Perfect Guard dealt {baseDurabilityDamage * 2} durability damage to {attacker.gameObject.name}");
        }

        // 런 통계 기록
        if (RunManager.Instance != null)
        {
            RunManager.Instance.RecordPerfectGuard();
        }

        Debug.Log("Perfect Guard bonus activated!");
    }
}
