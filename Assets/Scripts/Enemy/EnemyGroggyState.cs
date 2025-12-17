using UnityEngine;

/// <summary>
/// 적 Groggy 상태 - 내구력 0 시 진입, 처형 가능
/// </summary>
public class EnemyGroggyState : EnemyState
{
    public EnemyGroggyState(EnemyController controller, EnemyStateMachine stateMachine)
        : base(controller, stateMachine)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _controller.StopMovement();
        _controller.Animator?.SetTrigger("GroggyTrigger");

        Debug.Log($"{_controller.gameObject.name}: GROGGY! Can be executed!");
    }

    public override void OnUpdate()
    {
        // 그로기 상태에서는 아무 행동 안함
        // Durability 컴포넌트가 타이머 관리하고 OnGroggyEnd 이벤트 발생
    }

    public override void CheckTransitions()
    {
        // 상태 전환은 EnemyController의 OnGroggyEnd 이벤트에서 처리
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log($"{_controller.gameObject.name}: Recovered from groggy");
    }

    /// <summary>
    /// 처형당했을 때 (외부에서 호출)
    /// </summary>
    public void OnExecuted()
    {
        Debug.Log($"{_controller.gameObject.name}: EXECUTED!");
        _controller.Durability.OnExecute();
        _controller.Health.TakeDamage(float.MaxValue);  // 즉사
    }
}
