using UnityEngine;

/// <summary>
/// Jump 상태 - 플레이어 점프
/// 점프력을 적용하고 공중에서 좌우 이동 가능
/// </summary>
public class JumpState : PlayerState
{
    private PlayerMovement _movement;
    private Rigidbody2D _rb;

    // 점프 설정
    private float _jumpForce = 19f;
    private bool _hasJumped;

    public JumpState(PlayerController controller, PlayerStateMachine stateMachine, PlayerMovement movement)
        : base(controller, stateMachine)
    {
        _movement = movement;
        _rb = controller.GetComponent<Rigidbody2D>();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        _hasJumped = false;

        // 점프 입력 소비
        InputManager.Instance.ConsumeJumpInput();

        // 점프 애니메이션 트리거 (Animator에서 IsGrounded == false로 자동 전환)
        Debug.Log("Jump started");
    }

    public override void HandleInput()
    {
        // 공중에서도 좌우 이동 가능
        Vector2 moveInput = InputManager.Instance.MoveInput;
        _movement.SetMoveInput(moveInput);
    }

    public override void OnFixedUpdate()
    {
        // 첫 프레임에만 점프력 적용
        if (!_hasJumped)
        {
            _rb.linearVelocityY = _jumpForce;
            _hasJumped = true;
            Debug.Log($"Jump force applied: {_jumpForce}");
        }
    }

    public override void CheckTransitions()
    {
        // 점프 후 착지 시 전환 (하강 중이어야 함)
        if (_hasJumped && _controller.IsGrounded && _rb.linearVelocityY <= 0)
        {
            // 이동 입력이 있으면 Move, 없으면 Idle
            if (InputManager.Instance.MoveInput.magnitude > 0.1f)
            {
                _stateMachine.ChangeState(new MoveState(_controller, _stateMachine, _movement));
            }
            else
            {
                _stateMachine.ChangeState(new IdleState(_controller, _stateMachine, _movement));
            }
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log("Jump ended - landed");
    }
}
