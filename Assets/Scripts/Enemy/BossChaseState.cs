using UnityEngine;

/// <summary>
/// 보스 Chase 상태 - 플레이어 추적
/// </summary>
public class BossChaseState : BossState
{
    public BossChaseState(BossController boss, BossStateMachine stateMachine)
        : base(boss, stateMachine)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _boss.Animator?.SetFloat("Speed", 1f);
    }

    public override void OnFixedUpdate()
    {
        float distance = _boss.GetDistanceToTarget();
        float preferred = _boss.PreferredAttackDistance;
        float tolerance = _boss.DistanceTolerance;

        // 페이즈별 이동 속도 조절
        float speedMultiplier = _boss.CurrentPhaseData?.moveSpeedMultiplier ?? 1f;

        Debug.Log($"[Boss Chase FixedUpdate] Distance: {distance:F2}, Preferred: {preferred}, Tolerance: {tolerance}, Velocity: {_boss.Rigidbody?.linearVelocity}");

        if (distance > preferred + tolerance)
        {
            Debug.Log("[Boss Chase] Moving towards target");
            _boss.MoveTowardsTarget();
        }
        else if (distance < preferred - tolerance)
        {
            Debug.Log("[Boss Chase] Moving away from target");
            _boss.MoveAwayFromTarget();
        }
        else
        {
            _boss.StopMovement();
        }
    }

    public override void CheckTransitions()
    {
        float distance = _boss.GetDistanceToTarget();
        bool inDetection = _boss.IsTargetInDetectionRange();
        bool inAttack = _boss.IsTargetInAttackRange();
        bool canAttack = _boss.CanAttack();

        Debug.Log($"[Boss Chase] Distance: {distance:F2}, InDetection: {inDetection}, InAttack: {inAttack}, CanAttack: {canAttack}");

        // 감지 범위 이탈
        if (!inDetection)
        {
            Debug.Log("[Boss Chase] Target out of detection range, returning to Idle");
            _stateMachine.ChangeState(new BossIdleState(_boss, _stateMachine));
            return;
        }

        // 공격 범위 + 공격 가능
        if (inAttack && canAttack)
        {
            Debug.Log("[Boss Chase] Target in attack range, transitioning to Attack");
            _stateMachine.ChangeState(new BossAttackState(_boss, _stateMachine));
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        _boss.StopMovement();
        _boss.Animator?.SetFloat("Speed", 0f);
    }
}
