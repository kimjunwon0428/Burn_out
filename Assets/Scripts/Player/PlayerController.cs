using UnityEngine;

/// <summary>
/// 플레이어의 중앙 컨트롤러 - 모든 플레이어 서브시스템 조율
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckDistance = 0.1f;

    // 컴포넌트 참조
    private Rigidbody2D _rb;
    private PlayerMovement _movement;
    private Health _health;
    private Animator _animator;
    private CapsuleCollider2D _collider;

    // 상태 머신
    private PlayerStateMachine _stateMachine;

    // Ground 체크
    public bool IsGrounded { get; private set; }

    // 프로퍼티
    public PlayerMovement Movement => _movement;
    public PlayerStateMachine StateMachine => _stateMachine;
    public Health Health => _health;
    public Animator Animator => _animator;

    private void Awake()
    {
        // 컴포넌트 참조 캐시
        _rb = GetComponent<Rigidbody2D>();
        _movement = GetComponent<PlayerMovement>();
        _health = GetComponent<Health>();
        _animator = GetComponent<Animator>();
        _collider = GetComponent<CapsuleCollider2D>();

        // 상태 머신 초기화
        _stateMachine = new PlayerStateMachine();

        // 체력 이벤트 구독
        _health.OnDeath += OnPlayerDeath;

        // Player 레이어 설정 (Enemy와 충돌 방지)
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        gameObject.layer = playerLayer;

        // 런타임에 Player-Enemy 충돌 비활성화
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        Debug.Log($"[Player] Layer: {playerLayer}, Ignoring collision with Enemy({enemyLayer})");
    }

    private void Start()
    {
        // 시작 상태를 Idle로 설정
        _stateMachine.Initialize(new IdleState(this, _stateMachine, _movement));

        // 모든 트리거 초기화 (의도치 않은 애니메이션 방지)
        ResetAllAnimatorTriggers();

        Debug.Log("PlayerController initialized");
    }

    /// <summary>
    /// 모든 Animator 트리거를 리셋하여 의도치 않은 애니메이션 재생 방지
    /// </summary>
    private void ResetAllAnimatorTriggers()
    {
        if (_animator == null) return;

        _animator.ResetTrigger("AttackTrigger");
        _animator.ResetTrigger("HeavyAttackTrigger");
        _animator.ResetTrigger("SpecialAttackTrigger");
        _animator.ResetTrigger("DodgeTrigger");
        _animator.ResetTrigger("HitTrigger");
        _animator.ResetTrigger("ParryTrigger");
        _animator.ResetTrigger("ExecutionTrigger");
    }

    private void Update()
    {
        // Ground 체크
        CheckGround();

        // 상태 머신 업데이트
        _stateMachine.Update();
    }

    /// <summary>
    /// 바닥 감지 - Raycast로 아래쪽 충돌 체크
    /// </summary>
    private void CheckGround()
    {
        // 캐릭터 바닥에서 Raycast 시작
        Vector2 origin = (Vector2)transform.position + _collider.offset - new Vector2(0, _collider.size.y / 2);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, _groundCheckDistance, _groundLayer);

        IsGrounded = hit.collider != null;

        // Animator 파라미터 업데이트
        if (_animator != null)
        {
            _animator.SetBool("IsGrounded", IsGrounded);
        }
    }

    private void FixedUpdate()
    {
        // 상태 머신 물리 업데이트
        _stateMachine.FixedUpdate();

        // 이동 처리
        _movement.FixedUpdateMovement();
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (_health != null)
        {
            _health.OnDeath -= OnPlayerDeath;
        }
    }

    /// <summary>
    /// 플레이어 사망 처리
    /// </summary>
    private void OnPlayerDeath()
    {
        Debug.Log("Player died!");

        // 1. 이동 잠금
        _movement.LockMovement();

        // 2. 사망 애니메이션
        _animator?.SetTrigger("DeathTrigger");

        // 3. 런 종료 처리 (실패)
        if (RunManager.Instance != null)
        {
            RunManager.Instance.EndRun(false);
        }

        // 4. 임시 버프 초기화
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ClearTemporaryModifiers();
        }

        // 5. 게임오버 이벤트 (GameManager가 구독하여 UI 표시 등 처리)
        // GameManager.Instance?.OnPlayerDeath();
    }

    /// <summary>
    /// 플레이어 피격 처리 (외부에서 호출)
    /// </summary>
    /// <param name="damage">받을 데미지</param>
    /// <param name="canBeGuarded">가드 가능 여부</param>
    /// <param name="attacker">공격자 (퍼펙트 가드 시 내구력 데미지용)</param>
    public void TakeDamage(float damage, bool canBeGuarded = true, EnemyController attacker = null)
    {
        // 현재 상태 확인
        var currentState = _stateMachine.CurrentState;

        // 닷지 상태에서 무적 판정
        if (currentState is DodgeState dodgeState)
        {
            if (dodgeState.TryDodgeAttack())
            {
                return;  // 회피 성공
            }
        }

        // 가드 상태 처리
        bool isGuarding = currentState is GuardState;
        bool isPerfectGuard = false;

        if (isGuarding && currentState is GuardState guardState)
        {
            isPerfectGuard = guardState.IsInPerfectGuardWindow;

            // 가드 상태에서 데미지 처리 (공격자 정보 전달)
            float actualDamage = guardState.ProcessGuardedDamage(damage, canBeGuarded, attacker);
            if (actualDamage > 0)
            {
                _health.TakeDamage(actualDamage, canBeGuarded, true, false);
            }
            return;
        }

        // Health 컴포넌트에 데미지 전달
        _health.TakeDamage(damage, canBeGuarded, isGuarding, isPerfectGuard);

        // 피격 애니메이션 재생
        _animator?.SetTrigger("HitTrigger");
    }
}
