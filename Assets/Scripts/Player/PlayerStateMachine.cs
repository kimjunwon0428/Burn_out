using UnityEngine;

/// <summary>
/// 플레이어 상태 머신 - 상태 전환 관리
/// </summary>
public class PlayerStateMachine
{
    public PlayerState CurrentState { get; private set; }

    public void Initialize(PlayerState startingState)
    {
        CurrentState = startingState;
        CurrentState.OnEnter();
    }

    public void ChangeState(PlayerState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState?.OnExit();
        CurrentState = newState;
        CurrentState.OnEnter();
    }

    public void Update()
    {
        CurrentState?.HandleInput();
        CurrentState?.OnUpdate();
        CurrentState?.CheckTransitions();
    }

    public void FixedUpdate()
    {
        CurrentState?.OnFixedUpdate();
    }
}
