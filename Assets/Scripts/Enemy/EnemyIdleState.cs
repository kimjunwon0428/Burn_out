using UnityEngine;

/// <summary>
/// 적 Idle 상태 - 대기 중, 플레이어 감지 시 추적
/// </summary>
public class EnemyIdleState : EnemyState
{
    public EnemyIdleState(EnemyController controller, EnemyStateMachine stateMachine)
        : base(controller, stateMachine)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _controller.StopMovement();
        _controller.Animator?.SetFloat("Speed", 0f);
    }

    public override void CheckTransitions()
    {
        // 플레이어가 감지 범위 내면 추적
        if (_controller.IsTargetInDetectionRange())
        {
            _stateMachine.ChangeState(new EnemyChaseState(_controller, _stateMachine));
        }
    }
}
