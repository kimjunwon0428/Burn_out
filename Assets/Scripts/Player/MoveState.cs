using UnityEngine;

/// <summary>
/// Move 상태 - 플레이어가 이동 중
/// </summary>
public class MoveState : PlayerState
{
    private PlayerMovement _movement;

    public MoveState(PlayerController controller, PlayerStateMachine stateMachine, PlayerMovement movement)
        : base(controller, stateMachine)
    {
        _movement = movement;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        // Move 애니메이션 재생 (나중에 추가)
    }

    public override void HandleInput()
    {
        // 입력을 PlayerMovement에 전달
        Vector2 moveInput = InputManager.Instance.MoveInput;
        _movement.SetMoveInput(moveInput);
    }

    public override void CheckTransitions()
    {
        // 점프 입력 체크 (바닥에 있을 때만)
        if (InputManager.Instance.JumpPressed && _controller.IsGrounded)
        {
            _stateMachine.ChangeState(new JumpState(_controller, _stateMachine, _movement));
            return;
        }

        // 약공격 입력 체크 (좌클릭)
        if (InputManager.Instance.AttackPressed)
        {
            _stateMachine.ChangeState(new AttackState(_controller, _stateMachine, _movement, AttackType.Light));
            return;
        }

        // 강공격 입력 체크 (우클릭)
        if (InputManager.Instance.HeavyAttackPressed)
        {
            _stateMachine.ChangeState(new AttackState(_controller, _stateMachine, _movement, AttackType.Heavy));
            return;
        }

        // 닷지 입력 체크
        if (InputManager.Instance.DodgePressed)
        {
            _stateMachine.ChangeState(new DodgeState(_controller, _stateMachine, _movement));
            return;
        }

        // 가드 입력 체크
        if (InputManager.Instance.GuardHeld)
        {
            _stateMachine.ChangeState(new GuardState(_controller, _stateMachine, _movement));
            return;
        }

        // 이동 입력이 없으면 Idle 상태로 전환
        if (InputManager.Instance.MoveInput.magnitude < 0.1f)
        {
            _stateMachine.ChangeState(new IdleState(_controller, _stateMachine, _movement));
        }
    }
}
