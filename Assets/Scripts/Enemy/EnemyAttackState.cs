using UnityEngine;

/// <summary>
/// 적 Attack 상태 - 플레이어 공격
/// </summary>
public class EnemyAttackState : EnemyState
{
    private float _attackDuration = 0.5f;  // 공격 애니메이션 지속 시간
    private float _attackTimer;
    private float _damageTime = 0.25f;     // 데미지 발생 타이밍
    private bool _hasDealtDamage;

    public EnemyAttackState(EnemyController controller, EnemyStateMachine stateMachine)
        : base(controller, stateMachine)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _attackTimer = 0f;
        _hasDealtDamage = false;

        _controller.StopMovement();
        _controller.Animator?.SetTrigger("AttackTrigger");

        Debug.Log($"{_controller.gameObject.name}: Starting attack!");
    }

    public override void OnUpdate()
    {
        _attackTimer += Time.deltaTime;

        // 데미지 타이밍에 공격 수행
        if (!_hasDealtDamage && _attackTimer >= _damageTime)
        {
            // 공격 범위 내에 있을 때만 데미지
            if (_controller.IsTargetInAttackRange())
            {
                _controller.PerformAttack();
            }
            _hasDealtDamage = true;
        }
    }

    public override void CheckTransitions()
    {
        // 공격 종료 후 상태 전환
        if (_attackTimer >= _attackDuration)
        {
            // 공격 범위 내면 다시 공격 준비, 아니면 추적
            if (_controller.IsTargetInAttackRange())
            {
                _stateMachine.ChangeState(new EnemyIdleState(_controller, _stateMachine));
            }
            else if (_controller.IsTargetInDetectionRange())
            {
                _stateMachine.ChangeState(new EnemyChaseState(_controller, _stateMachine));
            }
            else
            {
                _stateMachine.ChangeState(new EnemyIdleState(_controller, _stateMachine));
            }
        }
    }

    public override void OnExit()
    {
        base.OnExit();
    }
}
