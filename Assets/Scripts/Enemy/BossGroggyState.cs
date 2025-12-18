using UnityEngine;

/// <summary>
/// 보스 Groggy 상태 - 내구력 0 시 진입, 처형 가능
/// </summary>
public class BossGroggyState : BossState
{
    public BossGroggyState(BossController boss, BossStateMachine stateMachine)
        : base(boss, stateMachine)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _boss.StopMovement();

        // StaggerTrigger 사용 (Boss.controller 파라미터)
        _boss.Animator?.SetTrigger("StaggerTrigger");

        Debug.Log($"[Boss] {_boss.BossName}: GROGGY! Can be executed!");
    }

    public override void OnUpdate()
    {
        // 그로기 상태에서는 아무 행동 안함
        // Durability 컴포넌트가 타이머 관리하고 OnGroggyEnd 이벤트 발생
    }

    public override void CheckTransitions()
    {
        // 상태 전환은 BossController의 OnGroggyEnd 이벤트에서 처리
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log($"[Boss] {_boss.BossName}: Recovered from groggy");
    }

    /// <summary>
    /// 처형당했을 때 (외부에서 호출)
    /// </summary>
    public void OnExecuted()
    {
        Debug.Log($"[Boss] {_boss.BossName}: EXECUTED!");
        _boss.Durability.OnExecute();
        _boss.Health.TakeDamage(float.MaxValue);  // 즉사
    }
}
