using UnityEngine;

/// <summary>
/// Dodge 상태 - 플레이어 회피
/// 짧은 거리를 빠르게 이동하며 무적 프레임 적용
/// </summary>
public class DodgeState : PlayerState
{
    private PlayerMovement _movement;
    private Rigidbody2D _rb;

    // 닷지 설정 (기본값)
    private float _baseDodgeDistance = 3f;       // 닷지 이동 거리
    private float _dodgeDuration = 0.25f;    // 닷지 지속 시간
    private float _dodgeTimer;
    private Vector2 _dodgeDirection;

    // 무적 프레임 설정 (기본값)
    private float _invincibilityStart = 0.05f;  // 무적 시작 타이밍
    private float _baseInvincibilityDuration = 0.15f;  // 무적 지속 시간

    // 퍼펙트 닷지 설정 (기본값)
    private float _basePerfectDodgeWindow = 0.15f;  // 퍼펙트 닷지 타이밍 윈도우
    private float _dodgeStartTime;
    private bool _perfectDodgeTriggered;

    // 스탯 기반 수치
    private float DodgeDistance => PlayerStats.Instance != null
        ? PlayerStats.Instance.GetStat(StatType.DodgeDistance)
        : _baseDodgeDistance;

    private float InvincibilityDuration => PlayerStats.Instance != null
        ? PlayerStats.Instance.GetStat(StatType.InvincibilityDuration)
        : _baseInvincibilityDuration;

    private float PerfectDodgeWindow => PlayerStats.Instance != null
        ? PlayerStats.Instance.GetStat(StatType.PerfectDodgeWindow)
        : _basePerfectDodgeWindow;

    // 무적 종료 타이밍 (스탯 기반)
    private float InvincibilityEnd => _invincibilityStart + InvincibilityDuration;

    /// <summary>
    /// 현재 무적 상태인지 확인 (스탯 기반)
    /// </summary>
    public bool IsInvincible => _dodgeTimer >= _invincibilityStart && _dodgeTimer <= InvincibilityEnd;

    /// <summary>
    /// 현재 퍼펙트 닷지 윈도우 내인지 확인 (스탯 기반)
    /// </summary>
    public bool IsInPerfectDodgeWindow => (Time.time - _dodgeStartTime) <= PerfectDodgeWindow;

    public DodgeState(PlayerController controller, PlayerStateMachine stateMachine, PlayerMovement movement)
        : base(controller, stateMachine)
    {
        _movement = movement;
        _rb = controller.GetComponent<Rigidbody2D>();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _dodgeTimer = 0f;
        _dodgeStartTime = Time.time;
        _perfectDodgeTriggered = false;

        // 닷지 방향 결정 (입력 방향 또는 바라보는 방향)
        Vector2 inputDir = InputManager.Instance.MoveInput;
        if (inputDir.magnitude > 0.1f)
        {
            _dodgeDirection = inputDir.normalized;
        }
        else
        {
            // 입력 없으면 바라보는 방향으로
            _dodgeDirection = new Vector2(_movement.FacingDirection, 0);
        }

        // 이동 잠금
        _movement.LockMovement();

        // 닷지 입력 소비
        InputManager.Instance.ConsumeDodgeInput();

        // 닷지 애니메이션 트리거
        _controller.Animator.SetTrigger("DodgeTrigger");

        Debug.Log($"Dodge started - Direction: {_dodgeDirection}");
    }

    public override void OnUpdate()
    {
        _dodgeTimer += Time.deltaTime;
    }

    public override void OnFixedUpdate()
    {
        // 닷지 중 이동 적용 (스탯 기반 거리)
        if (_dodgeTimer < _dodgeDuration)
        {
            float speed = DodgeDistance / _dodgeDuration;
            _rb.linearVelocity = _dodgeDirection * speed;
        }
    }

    public override void CheckTransitions()
    {
        // 닷지 종료 후 상태 전환
        if (_dodgeTimer >= _dodgeDuration)
        {
            // 속도 초기화
            _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);

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
    }

    public override void OnExit()
    {
        base.OnExit();
        _movement.UnlockMovement();  // 이동 잠금 해제
        Debug.Log("Dodge ended");
    }

    /// <summary>
    /// 닷지 중 피격 시 호출 - 무적 판정 및 퍼펙트 닷지 체크
    /// </summary>
    /// <returns>무적 상태로 회피 성공 여부</returns>
    public bool TryDodgeAttack()
    {
        if (IsInvincible)
        {
            if (IsInPerfectDodgeWindow && !_perfectDodgeTriggered)
            {
                // 퍼펙트 닷지 성공
                OnPerfectDodge();
                _perfectDodgeTriggered = true;
            }
            Debug.Log("Attack dodged!");
            return true;
        }
        return false;
    }

    /// <summary>
    /// 퍼펙트 닷지 성공 시 호출
    /// </summary>
    private void OnPerfectDodge()
    {
        // TODO: 퍼펙트 닷지 보상
        // - 특수 자원 충전
        // - 다음 공격 강화 버프
        // - 이펙트/사운드

        Debug.Log("Perfect Dodge! Bonus activated!");

        // 퍼펙트 닷지 보상 이벤트 발생 (Phase 2에서 확장)
        // PlayerController의 특수 자원 충전 메서드 호출 예정
    }
}
