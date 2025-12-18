using UnityEngine;

/// <summary>
/// 보스 전용 상태 머신
/// </summary>
public class BossStateMachine
{
    public BossState CurrentState { get; private set; }

    /// <summary>
    /// 상태 머신 초기화
    /// </summary>
    public void Initialize(BossState startingState)
    {
        CurrentState = startingState;
        CurrentState.OnEnter();
    }

    /// <summary>
    /// 상태 변경
    /// </summary>
    public void ChangeState(BossState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState?.OnExit();
        CurrentState = newState;
        CurrentState.OnEnter();
    }

    /// <summary>
    /// 매 프레임 업데이트
    /// </summary>
    public void Update()
    {
        CurrentState?.OnUpdate();
        CurrentState?.CheckTransitions();
    }

    /// <summary>
    /// 물리 업데이트
    /// </summary>
    public void FixedUpdate()
    {
        CurrentState?.OnFixedUpdate();
    }

    /// <summary>
    /// 피격 시 현재 상태에 전달
    /// </summary>
    public void OnHit(float damage)
    {
        CurrentState?.OnHit(damage);
    }
}
