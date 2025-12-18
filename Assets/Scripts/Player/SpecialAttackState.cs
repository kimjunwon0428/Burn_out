using UnityEngine;

/// <summary>
/// 특수 공격 상태 - 자원 소모하여 강력한 공격 수행
/// 특수 공격 중 무적 상태 적용
/// </summary>
public class SpecialAttackState : PlayerState
{
    private PlayerMovement _movement;
    private const float RESOURCE_COST = 50f;

    // 특수 공격 설정
    private float _attackDuration = 0.8f;
    private float _damageTimingStart = 0.2f;
    private float _damageTimingEnd = 0.5f;
    private float _attackRange = 2.0f;

    private float _attackTimer;
    private bool _hasDealtDamage;
    private LayerMask _enemyLayer;

    /// <summary>
    /// 특수 공격 중 무적 상태 (항상 true)
    /// </summary>
    public bool IsInvincible => true;

    // 스탯 기반 수치
    private float AttackDamage => PlayerStats.Instance != null
        ? PlayerStats.Instance.GetStat(StatType.AttackPower)
        : 10f;

    private float SpecialAttackMultiplier => PlayerStats.Instance != null
        ? PlayerStats.Instance.GetStat(StatType.SpecialAttackPower)
        : 2f;

    private float DurabilityDamageMultiplier => PlayerStats.Instance != null
        ? PlayerStats.Instance.GetStat(StatType.DurabilityDamage)
        : 1f;

    public SpecialAttackState(PlayerController controller, PlayerStateMachine stateMachine, PlayerMovement movement)
        : base(controller, stateMachine)
    {
        _movement = movement;
        _enemyLayer = LayerMask.GetMask("Enemy");
    }

    /// <summary>
    /// 특수 공격 사용 가능 여부 확인
    /// </summary>
    public static bool CanUseSpecialAttack()
    {
        return PlayerStats.Instance != null &&
               PlayerStats.Instance.CurrentSpecialResource >= RESOURCE_COST;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _attackTimer = 0f;
        _hasDealtDamage = false;

        // 자원 소모
        PlayerStats.Instance.TryConsumeSpecialResource(RESOURCE_COST);

        // 이동 잠금
        _movement.LockMovement();

        // 특수 공격 애니메이션
        _controller.Animator.SetTrigger("SpecialAttackTrigger");

        // 입력 소비
        InputManager.Instance.ConsumeSpecialAttackInput();

        Debug.Log($"Special Attack started! Resource: {PlayerStats.Instance.CurrentSpecialResource}");
    }

    public override void OnUpdate()
    {
        _attackTimer += Time.deltaTime;

        // 데미지 판정 타이밍 체크
        if (!_hasDealtDamage && _attackTimer >= _damageTimingStart && _attackTimer <= _damageTimingEnd)
        {
            PerformSpecialAttack();
            _hasDealtDamage = true;
        }
    }

    public override void CheckTransitions()
    {
        // 최소 데미지 판정 시간 이후에만 전환 체크
        if (_attackTimer < _damageTimingEnd) return;

        var stateInfo = _controller.Animator.GetCurrentAnimatorStateInfo(0);
        bool isInSpecialAttackAnim = stateInfo.IsName("SpecialAttack");
        bool animationComplete = !isInSpecialAttackAnim || stateInfo.normalizedTime >= 0.95f;

        if (animationComplete)
        {
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
        _movement.UnlockMovement();
        Debug.Log("Special Attack ended");
    }

    /// <summary>
    /// 특수 공격 판정 수행
    /// </summary>
    private void PerformSpecialAttack()
    {
        Vector2 attackOrigin = (Vector2)_controller.transform.position;
        Vector2 attackDirection = new Vector2(_movement.FacingDirection, 0);
        Vector2 attackCenter = attackOrigin + attackDirection * (_attackRange * 0.5f);

        Debug.Log($"[Special Attack] Origin: {attackOrigin}, Center: {attackCenter}, Range: {_attackRange * 0.5f}");

        // 범위 내 적 탐색
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, _attackRange * 0.5f, _enemyLayer);
        Debug.Log($"[Special Attack] Found {hits.Length} enemies in range");

        float damage = AttackDamage * SpecialAttackMultiplier;
        float durabilityDamage = damage * 0.5f * DurabilityDamageMultiplier * 2f; // 2배 내구력 데미지

        foreach (var hit in hits)
        {
            // Health 컴포넌트가 있으면 데미지 적용
            var health = hit.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log($"Special Attack hit: {hit.name} for {damage} damage");
            }

            // Durability 컴포넌트가 있으면 내구력 데미지
            var durability = hit.GetComponent<Durability>();
            if (durability != null)
            {
                durability.TakeDurabilityDamage(durabilityDamage);
            }
        }

        if (hits.Length == 0)
        {
            Debug.Log("Special Attack missed - no enemies in range");
        }
    }
}
