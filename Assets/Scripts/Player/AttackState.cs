using UnityEngine;

/// <summary>
/// 공격 타입 열거형
/// </summary>
public enum AttackType
{
    Light,  // 약공격 (좌클릭)
    Heavy   // 강공격 (우클릭)
}

/// <summary>
/// Attack 상태 - 플레이어 공격 (약공격/강공격)
/// </summary>
public class AttackState : PlayerState
{
    private PlayerMovement _movement;
    private AttackType _attackType;

    // 약공격 기본값
    private float _baseLightAttackDuration = 0.4f;
    private float _baseLightDamageTimingStart = 0.1f;
    private float _baseLightDamageTimingEnd = 0.25f;
    private float _lightDamageMultiplier = 1.0f;

    // 강공격 기본값
    private float _baseHeavyAttackDuration = 0.6f;
    private float _baseHeavyDamageTimingStart = 0.15f;
    private float _baseHeavyDamageTimingEnd = 0.35f;
    private float _heavyDamageMultiplier = 1.5f;
    private float _heavyDurabilityMultiplier = 1.5f;

    private float _attackTimer;
    private bool _hasDealtDamage;

    // 공격 설정 (기본값)
    private float _baseAttackDamage = 10f;
    private float _attackRange = 1.5f;
    private LayerMask _enemyLayer;

    // 스탯 기반 수치
    private float AttackDamage => PlayerStats.Instance != null
        ? PlayerStats.Instance.GetStat(StatType.AttackPower)
        : _baseAttackDamage;

    private float AttackSpeed => PlayerStats.Instance != null
        ? PlayerStats.Instance.GetStat(StatType.AttackSpeed)
        : 1f;

    private float DurabilityDamageMultiplier => PlayerStats.Instance != null
        ? PlayerStats.Instance.GetStat(StatType.DurabilityDamage)
        : 1f;

    // 공격 타입에 따른 기본값
    private float BaseAttackDuration => _attackType == AttackType.Light
        ? _baseLightAttackDuration
        : _baseHeavyAttackDuration;

    private float BaseDamageTimingStart => _attackType == AttackType.Light
        ? _baseLightDamageTimingStart
        : _baseHeavyDamageTimingStart;

    private float BaseDamageTimingEnd => _attackType == AttackType.Light
        ? _baseLightDamageTimingEnd
        : _baseHeavyDamageTimingEnd;

    private float DamageMultiplier => _attackType == AttackType.Light
        ? _lightDamageMultiplier
        : _heavyDamageMultiplier;

    private float ExtraDurabilityMultiplier => _attackType == AttackType.Light
        ? 1f
        : _heavyDurabilityMultiplier;

    // 공격 속도에 따른 지속 시간 (빠를수록 짧음)
    private float AttackDuration => BaseAttackDuration / AttackSpeed;
    private float DamageTimingStart => BaseDamageTimingStart / AttackSpeed;
    private float DamageTimingEnd => BaseDamageTimingEnd / AttackSpeed;

    public AttackState(PlayerController controller, PlayerStateMachine stateMachine, PlayerMovement movement, AttackType attackType = AttackType.Light)
        : base(controller, stateMachine)
    {
        _movement = movement;
        _attackType = attackType;
        _enemyLayer = LayerMask.GetMask("Enemy");
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _attackTimer = 0f;
        _hasDealtDamage = false;

        // 이동 잠금
        _movement.LockMovement();

        // 공격 타입에 따른 애니메이션 트리거
        string triggerName = _attackType == AttackType.Light ? "AttackTrigger" : "HeavyAttackTrigger";
        _controller.Animator.SetTrigger(triggerName);

        // 공격 입력 소비
        if (_attackType == AttackType.Light)
            InputManager.Instance.ConsumeAttackInput();
        else
            InputManager.Instance.ConsumeHeavyAttackInput();
    }

    public override void OnUpdate()
    {
        _attackTimer += Time.deltaTime;

        // 데미지 판정 타이밍 체크 (스탯 기반)
        if (!_hasDealtDamage && _attackTimer >= DamageTimingStart && _attackTimer <= DamageTimingEnd)
        {
            PerformAttack();
            _hasDealtDamage = true;
        }
    }

    public override void CheckTransitions()
    {
        // 최소 데미지 판정 시간 이후에만 전환 체크
        if (_attackTimer < DamageTimingEnd) return;

        var stateInfo = _controller.Animator.GetCurrentAnimatorStateInfo(0);

        // 현재 애니메이션 상태 확인
        string expectedState = _attackType == AttackType.Light ? "LightAttack" : "HeavyAttack";
        bool isInAttackAnim = stateInfo.IsName(expectedState);

        // 애니메이션 완료 조건: 공격 애니메이션이 아니거나 95% 이상 진행됨
        bool animationComplete = !isInAttackAnim || stateInfo.normalizedTime >= 0.95f;

        if (animationComplete)
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
    }

    public override void OnExit()
    {
        base.OnExit();
        _movement.UnlockMovement();  // 이동 잠금 해제
    }

    /// <summary>
    /// 공격 판정 수행 - 범위 내 적에게 데미지
    /// </summary>
    private void PerformAttack()
    {
        // 플레이어 위치와 방향 기준으로 공격 범위 설정
        Vector2 attackOrigin = (Vector2)_controller.transform.position;
        Vector2 attackDirection = new Vector2(_movement.FacingDirection, 0);
        Vector2 attackCenter = attackOrigin + attackDirection * (_attackRange * 0.5f);

        Debug.Log($"[Player Attack] Origin: {attackOrigin}, Center: {attackCenter}, Range: {_attackRange * 0.5f}, EnemyLayerMask: {_enemyLayer.value}");

        // 범위 내 적 탐색
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackCenter, _attackRange * 0.5f, _enemyLayer);
        Debug.Log($"[Player Attack] Found {hits.Length} enemies in range");

        float damage = AttackDamage * DamageMultiplier;

        foreach (var hit in hits)
        {
            // Health 컴포넌트가 있으면 데미지 적용
            var health = hit.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                string attackTypeName = _attackType == AttackType.Light ? "약공격" : "강공격";
                Debug.Log($"{attackTypeName} hit: {hit.name} for {damage} damage");
            }

            // 일반 공격은 강인도 데미지를 주지 않음
            // (퍼펙트 가드와 특수 공격만 강인도 감소)
        }

        if (hits.Length == 0)
        {
            Debug.Log("Attack missed - no enemies in range");
        }
    }
}
