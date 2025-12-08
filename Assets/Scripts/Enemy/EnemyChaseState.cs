using UnityEngine;

/// <summary>
/// 적 Chase 상태 - 플레이어 추적
/// </summary>
public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(EnemyController controller, EnemyStateMachine stateMachine)
        : base(controller, stateMachine)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _controller.Animator?.SetFloat("Speed", 1f);
    }

    public override void OnFixedUpdate()
    {
        float distance = _controller.GetDistanceToTarget();
        float preferred = _controller.PreferredAttackDistance;
        float tolerance = _controller.DistanceTolerance;

        if (distance > preferred + tolerance)
        {
            // 너무 멀면 접근
            _controller.MoveTowardsTarget();
        }
        else if (distance < preferred - tolerance)
        {
            // 너무 가까우면 후퇴
            _controller.MoveAwayFromTarget();
        }
        else
        {
            // 적절한 거리면 정지
            _controller.StopMovement();
        }
    }

    public override void CheckTransitions()
    {
        // 감지 범위 벗어나면 Idle
        if (!_controller.IsTargetInDetectionRange())
        {
            _stateMachine.ChangeState(new EnemyIdleState(_controller, _stateMachine));
            return;
        }

        // 공격 범위 내 + 공격 가능하면 Attack
        if (_controller.IsTargetInAttackRange() && _controller.CanAttack())
        {
            _stateMachine.ChangeState(new EnemyAttackState(_controller, _stateMachine));
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        _controller.StopMovement();
    }
}
