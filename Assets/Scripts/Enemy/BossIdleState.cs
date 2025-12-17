using UnityEngine;

/// <summary>
/// 보스 Idle 상태 - 대기, 플레이어 감지 시 추적/공격
/// </summary>
public class BossIdleState : BossState
{
    private float _idleTimer;
    private float _idleDuration = 0.5f;  // 공격 후 잠시 대기

    public BossIdleState(BossController boss, BossStateMachine stateMachine)
        : base(boss, stateMachine)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _idleTimer = 0f;

        _boss.StopMovement();
        _boss.Animator?.SetFloat("Speed", 0f);
    }

    public override void OnUpdate()
    {
        _idleTimer += Time.deltaTime;
    }

    public override void CheckTransitions()
    {
        // 최소 대기 시간 후 행동
        if (_idleTimer < _idleDuration) return;

        // 디버그: 거리 및 타겟 확인
        float distance = _boss.GetDistanceToTarget();
        bool inRange = _boss.IsTargetInDetectionRange();
        Debug.Log($"[Boss Idle] Distance: {distance:F2}, InDetectionRange: {inRange}, Target: {_boss.Target}");

        // 플레이어 감지
        if (!inRange) return;

        // 공격 범위 내 + 공격 가능
        if (_boss.IsTargetInAttackRange() && _boss.CanAttack())
        {
            _stateMachine.ChangeState(new BossAttackState(_boss, _stateMachine));
            return;
        }

        // 추적
        _stateMachine.ChangeState(new BossChaseState(_boss, _stateMachine));
    }
}
