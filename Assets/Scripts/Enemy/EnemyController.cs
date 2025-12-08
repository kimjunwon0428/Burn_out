using UnityEngine;

/// <summary>
/// 적 컨트롤러 - 적 캐릭터의 중앙 관리
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Durability))]
public class EnemyController : MonoBehaviour
{
    [Header("감지 설정")]
    [SerializeField] private float _detectionRange = 8f;   // 플레이어 감지 범위
    [SerializeField] private float _attackRange = 1.5f;    // 공격 범위

    [Header("이동 설정")]
    [SerializeField] private float _moveSpeed = 3f;

    [Header("공격 설정")]
    [SerializeField] private float _attackDamage = 15f;
    [SerializeField] private float _attackCooldown = 2f;

    [Header("거리 유지 설정")]
    [SerializeField] private float _preferredAttackDistance = 1.2f;  // 선호 공격 거리
    [SerializeField] private float _distanceTolerance = 0.3f;        // 허용 오차 (데드존)

    // 컴포넌트 참조
    private Rigidbody2D _rb;
    private Health _health;
    private Durability _durability;
    private Animator _animator;

    // 상태 머신
    private EnemyStateMachine _stateMachine;

    // 타겟 (플레이어)
    private Transform _target;
    private float _lastAttackTime;

    // 프로퍼티
    public Rigidbody2D Rigidbody => _rb;
    public Health Health => _health;
    public Durability Durability => _durability;
    public Animator Animator => _animator;
    public EnemyStateMachine StateMachine => _stateMachine;
    public Transform Target => _target;
    public float DetectionRange => _detectionRange;
    public float AttackRange => _attackRange;
    public float MoveSpeed => _moveSpeed;
    public float AttackDamage => _attackDamage;
    public float AttackCooldown => _attackCooldown;
    public float PreferredAttackDistance => _preferredAttackDistance;
    public float DistanceTolerance => _distanceTolerance;

    private int _facingDirection = -1;  // -1 = 왼쪽 (플레이어를 바라봄)
    public int FacingDirection => _facingDirection;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _health = GetComponent<Health>();
        _durability = GetComponent<Durability>();
        _animator = GetComponent<Animator>();

        _stateMachine = new EnemyStateMachine();

        // 이벤트 구독
        _health.OnDeath += OnDeath;
        _health.OnDamageTaken += OnDamageTaken;
        _durability.OnGroggyStart += OnGroggyStart;
        _durability.OnGroggyEnd += OnGroggyEnd;
    }

    private void Start()
    {
        // 플레이어 찾기
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            _target = player.transform;
        }

        // 시작 상태: Idle
        _stateMachine.Initialize(new EnemyIdleState(this, _stateMachine));
    }

    private void Update()
    {
        _stateMachine.Update();
        UpdateFacing();
    }

    private void FixedUpdate()
    {
        _stateMachine.FixedUpdate();
    }

    private void OnDestroy()
    {
        _health.OnDeath -= OnDeath;
        _health.OnDamageTaken -= OnDamageTaken;
        _durability.OnGroggyStart -= OnGroggyStart;
        _durability.OnGroggyEnd -= OnGroggyEnd;
    }

    /// <summary>
    /// 플레이어 방향으로 스프라이트 회전
    /// </summary>
    private void UpdateFacing()
    {
        if (_target == null) return;

        float direction = _target.position.x - transform.position.x;
        if (Mathf.Abs(direction) > 0.1f)
        {
            _facingDirection = direction > 0 ? 1 : -1;
            transform.localScale = new Vector3(_facingDirection, 1, 1);
        }
    }

    /// <summary>
    /// 플레이어와의 거리
    /// </summary>
    public float GetDistanceToTarget()
    {
        if (_target == null) return float.MaxValue;
        return Vector2.Distance(transform.position, _target.position);
    }

    /// <summary>
    /// 플레이어가 감지 범위 내인지
    /// </summary>
    public bool IsTargetInDetectionRange()
    {
        return GetDistanceToTarget() <= _detectionRange;
    }

    /// <summary>
    /// 플레이어가 공격 범위 내인지
    /// </summary>
    public bool IsTargetInAttackRange()
    {
        return GetDistanceToTarget() <= _attackRange;
    }

    /// <summary>
    /// 공격 쿨다운 확인
    /// </summary>
    public bool CanAttack()
    {
        return Time.time - _lastAttackTime >= _attackCooldown;
    }

    /// <summary>
    /// 공격 수행
    /// </summary>
    public void PerformAttack()
    {
        _lastAttackTime = Time.time;

        // 플레이어에게 데미지
        if (_target != null)
        {
            var playerController = _target.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(_attackDamage);
                Debug.Log($"{gameObject.name} attacked player for {_attackDamage} damage");
            }
        }
    }

    /// <summary>
    /// 타겟 방향으로 이동
    /// </summary>
    public void MoveTowardsTarget()
    {
        if (_target == null) return;

        Vector2 direction = (_target.position - transform.position).normalized;
        _rb.linearVelocity = new Vector2(direction.x * _moveSpeed, _rb.linearVelocity.y);
    }

    /// <summary>
    /// 타겟으로부터 멀어지는 방향으로 이동 (후퇴)
    /// </summary>
    public void MoveAwayFromTarget()
    {
        if (_target == null) return;

        Vector2 direction = (transform.position - _target.position).normalized;
        _rb.linearVelocity = new Vector2(direction.x * _moveSpeed, _rb.linearVelocity.y);
    }

    /// <summary>
    /// 선호 공격 거리에 있는지 확인
    /// </summary>
    public bool IsAtPreferredDistance()
    {
        float distance = GetDistanceToTarget();
        return distance >= _preferredAttackDistance - _distanceTolerance
            && distance <= _preferredAttackDistance + _distanceTolerance;
    }

    /// <summary>
    /// 이동 정지
    /// </summary>
    public void StopMovement()
    {
        _rb.linearVelocity = new Vector2(0, _rb.linearVelocity.y);
    }

    /// <summary>
    /// 피격 시
    /// </summary>
    private void OnDamageTaken(float damage)
    {
        _stateMachine.OnHit(damage);
    }

    /// <summary>
    /// 그로기 시작
    /// </summary>
    private void OnGroggyStart()
    {
        _stateMachine.ChangeState(new EnemyGroggyState(this, _stateMachine));
    }

    /// <summary>
    /// 그로기 종료
    /// </summary>
    private void OnGroggyEnd()
    {
        // 그로기 종료 후 Idle로 복귀
        _stateMachine.ChangeState(new EnemyIdleState(this, _stateMachine));
    }

    /// <summary>
    /// 사망
    /// </summary>
    private void OnDeath()
    {
        Debug.Log($"{gameObject.name} died!");
        _animator?.SetBool("IsDead", true);
        Destroy(gameObject, 1f);
    }

    /// <summary>
    /// 기즈모로 범위 표시 (에디터용)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 감지 범위 (노랑)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);

        // 공격 범위 (빨강)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}
