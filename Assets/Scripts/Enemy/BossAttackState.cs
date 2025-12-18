using UnityEngine;

/// <summary>
/// 보스 Attack 상태 - 랜덤 공격 패턴 선택
/// </summary>
public class BossAttackState : BossState
{
    // 공격 타이밍 설정 (공격별로 다름)
    private static readonly float[] AttackDurations = { 0.9f, 1.125f, 1.25f };   // Attack_1(10FPS), 2(8FPS), 3(8FPS)

    // 각 공격별 히트 타이밍 (프레임 기반으로 계산된 초 단위)
    // Attack_1: 5프레임 @ 10FPS = 0.5초
    // Attack_2: 2프레임, 5프레임 @ 8FPS = 0.25초, 0.625초 (2타 공격)
    // Attack_3: 6프레임 @ 8FPS = 0.75초
    private static readonly float[][] HitTimings = {
        new float[] { 0.5f },           // Attack_1: 5프레임 (10 FPS)
        new float[] { 0.25f, 0.625f },  // Attack_2: 2프레임, 5프레임 (8 FPS) - 2타
        new float[] { 0.75f }           // Attack_3: 6프레임 (8 FPS)
    };

    private int _selectedAttack;
    private float _attackTimer;
    private bool[] _hitDealt;

    private float AttackDuration => _selectedAttack < AttackDurations.Length
        ? AttackDurations[_selectedAttack]
        : 0.9f;

    private float[] CurrentHitTimings => _selectedAttack < HitTimings.Length
        ? HitTimings[_selectedAttack]
        : new float[] { 0.5f };

    public BossAttackState(BossController boss, BossStateMachine stateMachine)
        : base(boss, stateMachine)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _attackTimer = 0f;

        // 랜덤 공격 선택 (페이즈 기반)
        _selectedAttack = _boss.SelectRandomAttack();

        // 히트 처리 배열 초기화 (공격별 히트 수에 맞게)
        _hitDealt = new bool[CurrentHitTimings.Length];

        _boss.StopMovement();

        // 선택된 공격 트리거
        _boss.TriggerAttack(_selectedAttack);

        Debug.Log($"[Boss] Selected attack: {_selectedAttack + 1} (hits: {CurrentHitTimings.Length})");
    }

    public override void OnUpdate()
    {
        _attackTimer += Time.deltaTime;

        // 페이즈별 공격 속도 조절
        float speedMultiplier = _boss.CurrentPhaseData?.attackSpeedMultiplier ?? 1f;
        float adjustedTimer = _attackTimer * speedMultiplier;

        // 다중 히트 타이밍 체크
        float[] hitTimings = CurrentHitTimings;
        for (int i = 0; i < hitTimings.Length; i++)
        {
            if (!_hitDealt[i] && adjustedTimer >= hitTimings[i])
            {
                if (_boss.IsTargetInAttackRange())
                {
                    _boss.PerformAttack();
                }
                _hitDealt[i] = true;
            }
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
