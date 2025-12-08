using UnityEngine;

/// <summary>
/// 적 상태 추상 베이스 클래스
/// </summary>
public abstract class EnemyState
{
    protected EnemyController _controller;
    protected EnemyStateMachine _stateMachine;

    public EnemyState(EnemyController controller, EnemyStateMachine stateMachine)
    {
        _controller = controller;
        _stateMachine = stateMachine;
    }

    /// <summary>
    /// 상태 진입 시 호출
    /// </summary>
    public virtual void OnEnter()
    {
        Debug.Log($"[Enemy] Entering state: {GetType().Name}");
    }

    /// <summary>
    /// 매 프레임 업데이트
    /// </summary>
    public virtual void OnUpdate()
    {
    }

    /// <summary>
    /// 물리 업데이트
    /// </summary>
    public virtual void OnFixedUpdate()
    {
    }

    /// <summary>
    /// 상태 종료 시 호출
    /// </summary>
    public virtual void OnExit()
    {
    }

    /// <summary>
    /// 상태 전환 체크
    /// </summary>
    public virtual void CheckTransitions()
    {
    }

    /// <summary>
    /// 피격 시 호출
    /// </summary>
    public virtual void OnHit(float damage)
    {
    }
}
