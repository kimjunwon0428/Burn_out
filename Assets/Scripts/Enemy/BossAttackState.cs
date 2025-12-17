using UnityEngine;

/// <summary>
/// 보스 Attack 상태 - 랜덤 공격 패턴 선택
/// </summary>
public class BossAttackState : BossState
{
    // 공격 타이밍 설정 (공격별로 다름)
    private static readonly float[] AttackDurations = { 0.9f, 0.8f, 1.0f };      // Attack_1, 2, 3
    private static readonly float[] DamageTimingStarts = { 0.25f, 0.2f, 0.3f };
    private static readonly float[] DamageTimingEnds = { 0.45f, 0.4f, 0.5f };

    private int _selectedAttack;
    private float _attackTimer;
    private bool _hasDealtDamage;

    private float AttackDuration => _selectedAttack < AttackDurations.Length
        ? AttackDurations[_selectedAttack]
        : 0.9f;

    private float DamageTimingStart => _selectedAttack < DamageTimingStarts.Length
        ? DamageTimingStarts[_selectedAttack]
        : 0.25f;

    private float DamageTimingEnd => _selectedAttack < DamageTimingEnds.Length
        ? DamageTimingEnds[_selectedAttack]
        : 0.45f;

    public BossAttackState(BossController boss, BossStateMachine stateMachine)
        : base(boss, stateMachine)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _attackTimer = 0f;
        _hasDealtDamage = false;

        // 랜덤 공격 선택 (페이즈 기반)
        _selectedAttack = _boss.SelectRandomAttack();

        _boss.StopMovement();

        // 선택된 공격 트리거
        _boss.TriggerAttack(_selectedAttack);

        Debug.Log($"[Boss] Selected attack: {_selectedAttack + 1}");
    }

    public override void OnUpdate()
    {
        _attackTimer += Time.deltaTime;

        // 페이즈별 공격 속도 조절
        float speedMultiplier = _boss.CurrentPhaseData?.attackSpeedMultiplier ?? 1f;
        float adjustedTimer = _attackTimer * speedMultiplier;

        // 데미지 타이밍 체크
        if (!_hasDealtDamage &&
            adjustedTimer >= DamageTimingStart &&
            adjustedTimer <= DamageTimingEnd)
        {
            if (_boss.IsTargetInAttackRange())
            {
                _boss.PerformAttack();
            }
            _hasDealtDamage = true;
        }
    }

    public override void CheckTransitions()
    {
        // 페이즈별 공격 속도 적용
        float speedMultiplier = _boss.CurrentPhaseData?.attackSpeedMultiplier ?? 1f;
        float effectiveDuration = AttackDuration / speedMultiplier;

        if (_attackTimer >= effectiveDuration)
        {
            // 공격 완료 후 상태 전환
            if (_boss.IsTargetInAttackRange() && _boss.CanAttack())
            {
                // 공격 범위 내 + 공격 가능 -> 잠시 대기 후 다시 공격
                _stateMachine.ChangeState(new BossIdleState(_boss, _stateMachine));
            }
            else if (_boss.IsTargetInDetectionRange())
            {
                _stateMachine.ChangeState(new BossChaseState(_boss, _stateMachine));
            }
            else
            {
                _stateMachine.ChangeState(new BossIdleState(_boss, _stateMachine));
            }
        }
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}
