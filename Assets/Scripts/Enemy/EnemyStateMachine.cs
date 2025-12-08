using UnityEngine;

/// <summary>
/// 적 상태 머신 - 상태 전환 관리
/// </summary>
public class EnemyStateMachine
{
    public EnemyState CurrentState { get; private set; }

    public void Initialize(EnemyState startingState)
    {
        CurrentState = startingState;
        CurrentState.OnEnter();
    }

    public void ChangeState(EnemyState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState?.OnExit();
        CurrentState = newState;
        CurrentState.OnEnter();
    }

    public void Update()
    {
        CurrentState?.OnUpdate();
        CurrentState?.CheckTransitions();
    }

    public void FixedUpdate()
    {
        CurrentState?.OnFixedUpdate();
    }

    /// <summary>
    /// 피격 이벤트 전달
    /// </summary>
    public void OnHit(float damage)
    {
        CurrentState?.OnHit(damage);
    }
}
